using System;
using API.DTO;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        /// Getting the user from other sources like from the User (it is own method for)
        /// the handling the tokens, then we check if the user is existing and laying it on new 
        /// variable username
        /// 
        var username = User.GetUsername();
        if (username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("you can not send yourself a message");

        /// we take the repository to search for the sender (App USER) and the recipient (App User)
        var sender = await userRepository.GetUserByUserNameAsync(username);
        var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);
        if (sender == null || recipient == null) return BadRequest("Can not send");

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

        if (await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));

        /// The point where it should not be at the last to have a clean exit
        return BadRequest("Failed to save the message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(messages);

        return messages;

    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {

        var currentUser = User.GetUsername();

        return Ok(await messageRepository.GetMessageThread(currentUser, username));

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await messageRepository.GetMessage(id);

        if (message == null) return BadRequest("Can not delete this message");

        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Forbid();
        if (message.SenderUsername == username)
        {
            message.SenderDeleted = true;
        }
        if (message.RecipientUsername == username)
        {
            message.RecipientDeleted = true;
        }
        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            messageRepository.DeleteMessage(message);
        }
        if (await messageRepository.SaveAllAsync()) return Ok();
        return BadRequest("Problem deleting the message");
   




    }
}


