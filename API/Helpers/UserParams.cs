using System;
using Microsoft.Net.Http.Headers;

namespace API.Helpers;

public class UserParams : PaginationParams
{
  

    public string? Gender { get; set; }
    public string? CurrentName { get; set; }

    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";



}
