using System.Security.Claims;
using API.DTO;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace API.Controllers;

[Authorize] // If here then for all
public class UsersController(IUserRepository userRepository,IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentName = User.GetUsername();
        var users = await userRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }
    [HttpGet("{username}")] //api/users/
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);
        if (user == null) return NotFound();
        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());
        if (user == null) return BadRequest("User is not found");
        mapper.Map(memberUpdateDto, user);
        if (await userRepository.SaveAllAsync()) return NoContent(); //Basically says it is ok but no return
        return BadRequest("Did not update");

    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());
        if (user == null) return BadRequest("User is not found");
        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,

        };
        if(user.Photos.Count == 0) photo.IsMain = true;
        
        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync()) return 
            CreatedAtAction(nameof(GetUser),
            new {username = user.UserName},mapper.Map<PhotoDto>(photo));


        return BadRequest("Problem adding photo");
    }
    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId){
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());
        if(user == null) return BadRequest("User is not found");
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo ==null || photo.IsMain) return BadRequest("Photo is already main");
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if(currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

       if (await userRepository.SaveAllAsync()) return NoContent();   
       return  BadRequest("Problem setting wrong") ;
    }
    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId){
        var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());
        if(user == null) return BadRequest("User is not found");
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if(photo ==null || photo.IsMain) return BadRequest("Photo is main or no photo");

        if(photo.PublicId != null){
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);
        if(await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Not deleted");


        





    }
}
