using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using API.DTO;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")] //account/register
    public async Task<ActionResult<DtoUser>> Register(DtoRegister registerDto)
    {
        if (await UserExists(registerDto.UserName)) { return BadRequest("User is existing"); }

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.UserName.ToLower();
        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);


        return new DtoUser
        {
            UserName = user.UserName,
            Token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }
    [HttpPost("login")] //account/login
    public async Task<ActionResult<DtoUser>> Login(DtoLogin loginDto)
    {

        // var user = await userManager.Users
        // .Include(p=>p.Photos)
        // .FirstOrDefaultAsync( x =>
        //     x.UserName == loginDto.UserName.ToUpper());

        var user = await userManager.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x =>
                x.NormalizedUserName == loginDto.UserName.ToUpper());

        if (user == null || user.UserName == null) return Unauthorized("User is not registered");
        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) return Unauthorized();

        return new DtoUser
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Token = await tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            Gender = user.Gender
        };
    }


    private async Task<bool> UserExists(string username)
    {

        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper()); //Bob != bob
    }
}
