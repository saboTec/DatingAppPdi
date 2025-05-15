using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using API.DTO;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using API.Interfaces;
using AutoMapper;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")] //account/register
    public async Task<ActionResult<DtoUser>> Register(DtoRegister dtoRegister){
        if (await UserExists(dtoRegister.UserName)){return BadRequest("User is existing");}
        using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(dtoRegister);
        user.UserName = dtoRegister.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dtoRegister.Password));
        user.PasswordSalt = hmac.Key;
       
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return new DtoUser{
            UserName = user.UserName,
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
   }
    [HttpPost("login")] //account/login
    public async Task<ActionResult<DtoUser>> Login(DtoLogin dtoLogin){
        // if (!await UserExists(dtoLogin.UserName)){return Unauthorized("User is NOT existing");}
        var user = await context.Users
        .Include(p=>p.Photos)
        .FirstOrDefaultAsync( x =>
            x.UserName == dtoLogin.UserName.ToLower());

        if (user == null) return Unauthorized("User is not registered");
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dtoLogin.Password));
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (user.PasswordHash[i]!=computedHash[i]) return Unauthorized("Password is not correct");
        }

        return new DtoUser{
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }
    

   private async Task<bool> UserExists(string username ){

        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower()); //Bob != bob
   }
}
