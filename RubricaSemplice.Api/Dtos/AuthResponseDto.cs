

namespace  RubricaSemplice.Api.Dtos;

public class AuthResponseDto
{
    public string Token {get; set;}        = string.Empty;
    public string UserId {get; set;}       = string.Empty;
    public string Email {get; set;}        = string.Empty;
    public string NomeCompleto {get; set;} = string.Empty;

    public int Eta {get;set;}
    public bool Abilitato{get;set;} = true;
    public string Role {get; set;} = string.Empty;

}