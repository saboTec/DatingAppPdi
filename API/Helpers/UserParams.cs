using System;
using Microsoft.Net.Http.Headers;

namespace API.Helpers;

public class UserParams
{
    private const int MaxPageSize = 50;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 10;


    public int PageSize
    {   
        //old way of writing
        // get { return _pageSize; }
        // set { _pageSize = value; }
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? Gender {get; set;}
    public string? CurrentName  {get; set;}

    public int MinAge {get; set;} = 18;
    public int MaxAge  {get; set;} = 100;



}
