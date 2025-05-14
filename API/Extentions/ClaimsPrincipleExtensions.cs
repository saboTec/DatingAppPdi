using System;
using System.Security.Claims;

namespace API.Extentions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
    {   
        var username = (user.FindFirstValue(ClaimTypes.NameIdentifier)) 
        ?? throw new Exception("Username not found in the Claims");
        return username;
    }

}
