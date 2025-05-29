using System;
using API.Interfaces;
using API.Extentions;
using Microsoft.AspNetCore.SignalR;
using API.DTO;
using API.Entities;
using AutoMapper;
using System.Security.AccessControl;

namespace API.SignalR;

public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
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

        var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

        if (unitOfWork.HasChanges()) await unitOfWork.Complete();
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
        var sender = await unitOfWork.UserRepository.GetUserByUserNameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);
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
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

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
        /// we use unitOfWork.MessageRepository to do that.
        unitOfWork.MessageRepository.AddMessage(message);

        /// Finally to save the changes on the database
        /// and map the message to the MessageDto (like a saving function of the modified values
        /// since it is injected only in one instance)

        if (await unitOfWork.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
        ;

        /// The point where it should not be at the last to have a clean exit
    }
    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username");
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection
        {
            ConnectionId = Context.ConnectionId,
            Username = username
        };
        if (group == null)
        {
            group = new Group { Name = groupName };
            unitOfWork.MessageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);

        if (await unitOfWork.Complete()) return group;
        throw new HubException("Failed to join group");
    }
    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        if (connection != null && group != null)
        {
            unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await unitOfWork.Complete()) return group;

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
//     private readonly IunitOfWork.MessageRepository _unitOfWork.MessageRepository;
//     private readonly IMapper _mapper;
//     private readonly IunitOfWork.UserRepository _unitOfWork.UserRepository;

//     public MessageHub(IunitOfWork.MessageRepository unitOfWork.MessageRepository, IMapper mapper, IunitOfWork.UserRepository unitOfWork.UserRepository)
//     {
//         _unitOfWork.MessageRepository = unitOfWork.MessageRepository;
//         _mapper = mapper;
//         _unitOfWork.UserRepository = unitOfWork.UserRepository;
//     }

//     public override async Task OnConnectedAsync()
//     {
//         var httpContext = Context.GetHttpContext();
//         var otherUser = httpContext?.Request.Query["user"];
//         if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
//         var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
//         await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

//         var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);
//         await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
//     }

//     public async Task SendMessage(CreateMessageDto createMessageDto)
//     {
//         var username = Context.User?.GetUsername() ?? throw new Exception("Could not get the user");

//         if (username == createMessageDto.RecipientUsername.ToLower())
//             throw new HubException("You cannot message yourself");

//         var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
//         var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

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

//         _unitOfWork.MessageRepository.AddMessage(message);

//         if (await _unitOfWork.Complete())
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