using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Data;

// definizione del contesto del database, i due punti indicano l'ereditarietà ovvero ApplicationDbContext che eredità tutto ciò che fa IdentityUserContext



public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
{
    /* Questo DbContext ora gestisce:
    - utenti
    - ruoli
    - user-roles
    - claims, logins, tokens di Identity
    - la nostra tabella custom Interests 
    */

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
    {
     
    }

    public DbSet<Interest> Interests {get;set;}


    // Configura le relazioni tra tabelle
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Prima lasciamo a Identity configurare le sue tabelle standard
        base.OnModelCreating(builder);

        // Configura il collegamento tra utente e interessi
        builder.Entity<Interest>()
            .HasOne(i => i.User)
            .WithMany(u => u.Interests)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}