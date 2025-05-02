using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class DtoRegister
{
    [Required]
    public required string UserName{get;set;}

  [Required]
    public required string Password{get;set;}


}
