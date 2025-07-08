using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext dataContext):BaseApiController
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuth(){
        return "secret text";
    }
    [HttpGet("not-found")]
     public ActionResult<AppUser> GetNotFound(){
        var thing = dataContext.Users.Find(-1);
        if(thing==null) return NotFound("User not found");
        return thing;
    }
    [HttpGet("server-error")]
     public ActionResult<AppUser> GetServerError(){
        var thing = dataContext.Users.Find(-1) ?? throw new Exception("Something bad has happened");
        return thing;
    }
    [HttpGet("bad-request")]
     public ActionResult<string> GetBadRequestError(){
        return BadRequest("Server not found");
    }
}
