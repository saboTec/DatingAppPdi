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
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
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
        var query = context.Messages
            .Where(x =>
                x.Recipient.UserName == currentUsername
                    && x.RecipientDeleted == false
                    && x.SenderUsername == recipientUsername ||
                x.Sender.UserName == recipientUsername
                    && x.SenderDeleted == false
                    && x.RecipientUsername == recipientUsername
            )
            .OrderBy(x => x.MessageSent)
            .AsQueryable();
            // .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            // .ToListAsync();

        ///Checkin for the unread messages. to get all the empty times
        var unredMessages = query.Where(x => x.DateRead == null &&
            x.RecipientUsername == currentUsername).ToList();

        /// for those found we are setting the datetime now since we open the thread
        if (unredMessages.Count != 0)
        {
            unredMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
        }
        /// return with the mapper the messages into MessageDto 
        return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();

    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }


}
