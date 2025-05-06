using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class DtoRegister
{
  [Required]
  public string UserName { get; set; } = string.Empty;

  [Required]
  [StringLength(8, MinimumLength = 4)]
  public string Password { get; set; } = string.Empty;


}
