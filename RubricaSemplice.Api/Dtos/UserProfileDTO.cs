namespace RubricaSemplice.Api.Dtos;

public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public bool Abilitato {get;set;} = true;

}