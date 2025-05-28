using System;
using API.Interfaces;
using API.Extentions;
using Microsoft.AspNetCore.SignalR;
using API.DTO;
using API.Entities;
using AutoMapper;
using System.Security.AccessControl;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, IHubContext<PresenceHub> presenceHub) : Hub
{

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];
        if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

    }
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
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

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messageRepository.GetMessageGroup(groupName);

        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }
        /// After that the message need to be updated on the database, 
        /// we use messageRepository to do that.
        messageRepository.AddMessage(message);

        /// Finally to save the changes on the database
        /// and map the message to the MessageDto (like a saving function of the modified values
        /// since it is injected only in one instance)

        if (await messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
        ;

        /// The point where it should not be at the last to have a clean exit
    }
    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username");
        var group = await messageRepository.GetMessageGroup(groupName);
        var connection = new Connection
        {
            ConnectionId = Context.ConnectionId,
            Username = username
        };
        if (group == null)
        {
            group = new Group { Name = groupName };
            messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);

        if (await messageRepository.SaveAllAsync()) return group;
        throw new HubException("Failed to join group");
    }
    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        if (connection != null && group != null)
        {
            messageRepository.RemoveConnection(connection);
            if (await messageRepository.SaveAllAsync()) return group;

        }
        throw new Exception("Failed to remove from group");
    }
    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}



/// <summary>
/// CHAT GPT
/// </summary>
/// 
// public class MessageHub : Hub
// {
//     private readonly IMessageRepository _messageRepository;
//     private readonly IMapper _mapper;
//     private readonly IUserRepository _userRepository;

//     public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository)
//     {
//         _messageRepository = messageRepository;
//         _mapper = mapper;
//         _userRepository = userRepository;
//     }

//     public override async Task OnConnectedAsync()
//     {
//         var httpContext = Context.GetHttpContext();
//         var otherUser = httpContext?.Request.Query["user"];
//         if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
//         var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
//         await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

//         var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);
//         await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
//     }

//     public async Task SendMessage(CreateMessageDto createMessageDto)
//     {
//         var username = Context.User?.GetUsername() ?? throw new Exception("Could not get the user");

//         if (username == createMessageDto.RecipientUsername.ToLower())
//             throw new HubException("You cannot message yourself");

//         var sender = await _userRepository.GetUserByUserNameAsync(username);
//         var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

//         if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
//             throw new HubException("Could not send");

//         var message = new Message
//         {
//             Sender = sender,
//             Recipient = recipient,
//             SenderUsername = sender.UserName,
//             RecipientUsername = recipient.UserName,
//             Content = createMessageDto.Content
//         };

//         _messageRepository.AddMessage(message);

//         if (await _messageRepository.SaveAllAsync())
//         {
//             var group = GetGroupName(sender.UserName, recipient.UserName);
//             await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
//         }
//     }

//     private string GetGroupName(string caller, string? other)
//     {
//         var stringCompare = string.CompareOrdinal(caller, other) < 0;
//         return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
//     }
// }