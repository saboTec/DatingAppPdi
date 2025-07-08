using System;
using System.Security.Claims;

namespace API.Extentions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        var userName = (user.FindFirstValue(ClaimTypes.Name))
        ?? throw new Exception("Username not found in the Claims");
        return userName;
    }
    
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("User not found in the Claims"));
        return userId;
    }
}
