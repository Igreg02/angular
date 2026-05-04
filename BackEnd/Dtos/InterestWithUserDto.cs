namespace RubricaSemplice.Api.Dtos;

public class InterestWithUserDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}