using System;
using API.Interfaces;
using API.Extentions;
using Microsoft.AspNetCore.SignalR;
using API.DTO;
using API.Entities;
using AutoMapper;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepository,IMapper mapper) : Hub
{

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];
        if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);

    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto,IUserRepository userRepository)
    {
        /// Getting the user from other sources like from the User (it is own method for)
        /// the handling the tokens, then we check if the user is existing and laying it on new 
        /// variable username
        /// 
        var username = Context.User?.GetUsername() ?? throw new Exception("Could not get the user");
        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot message yourself");

        /// we take the repository to search for the sender (App USER) and the recipient (App User)
        var sender = await userRepository.GetUserByUserNameAsync(username);
        var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);
        if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
                        throw new HubException("Could not send");


        /// the Message is beeing created and all the required variables need to be populated
        /// and our content is been handded over from the parameter createMessageDto.
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content

        };
        /// After that the message need to be updated on the database, 
        /// we use messageRepository to do that.
        messageRepository.AddMessage(message);

        /// Finally to save the changes on the database
        /// and map the message to the MessageDto (like a saving function of the modified values
        /// since it is injected only in one instance)

        if (await messageRepository.SaveAllAsync())
        {
            var group = GetGroupName(sender.UserName, recipient.UserName);
            await Clients.Group(group).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        };

        /// The point where it should not be at the last to have a clean exit
    }
    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
