using System.ComponentModel.DataAnnotations;

namespace  RubricaSemplice.Api.Dtos;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password {get;set;} = string.Empty;

    [Required]
    [StringLength(100)]
    public string NomeCompleto {get; set;} = string.Empty;

    public string? PhoneNumber {get; set;}

    public int Eta{get;set;}
    public bool Abilitato{get;set;} = true;
}
