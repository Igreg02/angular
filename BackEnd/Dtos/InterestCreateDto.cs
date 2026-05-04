

namespace RubricaSemplice.Api.Dtos;

public class InterestCreateDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? UserId { get; set; }
}