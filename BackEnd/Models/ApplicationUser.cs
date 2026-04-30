using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RubricaSemplice.Api.Models;

[Table("Users")] // decorator che permette di definire a quale tabella appartiene la tabella.
public class ApplicationUser : IdentityUser
{
    // IdentityUser ha già: Id, UserName, Email, PasswordHas, PhoneNumber ecc

    [Required]
    [StringLength(100)]
    public string NomeCompleto { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Abilitato {get;set;} = true;

    // un utente può avere molti interessi

    public List<Interest> Interests { get; set; } = new List<Interest>();
}