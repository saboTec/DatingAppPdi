using System;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
 
[Authorize] // If here then for all
public class UsersController(IUserRepository userRepository) : BaseApiController
{   
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(){   
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }
    [AllowAnonymous] // If here then
    [HttpGet("{username}")] //api/users/
    public async Task<ActionResult<MemberDto>> GetUser(string username){   
        var user = await userRepository.GetMemberAsync(username);
        if (user == null) return NotFound();
        return user; 
    }
    
}
