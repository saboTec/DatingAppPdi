using System;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        /// We built up our query and give to it some options.
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        ///Here is the query checkec for the container, if it is inbox or outbox 
        /// depending of the Container we are returning the recipient or the sender
        /// like we are returning Inbox when the recipient username matches the messageparams username
        /// else if it is it from sender then outbox, default is returned when the username of recipient matches and date read null
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false),
            "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false),
            _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false)
        };

        /// we are projecting the query to Message Dto and we want to return the property of this
        /// like the queryable option of the mapper to var messages
        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        ///finally we return our PagedList.
        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        ///get our messages and include other classes like AppUser and Photos since we are having these variables in 
        /// the Messages.
        var messages = await context.Messages
            .Include(x => x.Sender).ThenInclude(x => x.Photos)
            .Include(x => x.Recipient).ThenInclude(x => x.Photos)
            .Where(x =>
                x.Recipient.UserName == currentUsername
                    && x.RecipientDeleted == false
                    && x.SenderUsername == recipientUsername ||
                x.Sender.UserName == recipientUsername
                    && x.SenderDeleted == false
                    && x.RecipientUsername == recipientUsername
            )
            .OrderBy(x => x.MessageSent)
            .ToListAsync();

        ///Checkin for the unread messages. to get all the empty times
        var unredMessages = messages.Where(x => x.DateRead == null &&
            x.RecipientUsername == currentUsername).ToList();

        /// for those found we are setting the datetime now since we open the thread
        if (unredMessages.Count != 0)
        {
            unredMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }
        /// return with the mapper the messages into MessageDto 
        return mapper.Map<IEnumerable<MessageDto>>(messages);

    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
