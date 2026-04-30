using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RubricaSemplice.Api.Data;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Seed;

public static class DataSeeder
{
  // questo metodo crea utenti e interessi iniziali. Se i dati esistono già, non li duplica.
  public static async Task SeedAsync(IServiceProvider serviceProvider)
  {
    using IServiceScope scope = serviceProvider.CreateScope(); // creazione scope per i servizi necessari. Serve per aprire e chiudere la connessione al database.

    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // creiamo i ruoli se non esistono
    await EnsureRoleExistsAsync(roleManager, UserRoles.Admin);
    await EnsureRoleExistsAsync(roleManager, UserRoles.Editor);
    await EnsureRoleExistsAsync(roleManager, UserRoles.User);

    // creiamo alcuni utenti demo attraverso userManager che controlla in automatico che non ci siano doppioni negli inserimenti
    ApplicationUser admin = await EnsureUserExistsAsync(
        userManager,
        "utente1@gmail.com",
        "123456",
        "Utente uno",
        "3331234567",
      true);


    ApplicationUser editor = await EnsureUserExistsAsync(
    userManager,
    "utente2@gmail.com",
    "123456",
    "utente due",
    "3332354567",
    true);

    ApplicationUser normalUser = await EnsureUserExistsAsync(
    userManager,
    "utente3@gmail.com",
    "123456",
    "utente tre",
    "3331894567",
    true);

    // assegniamo i ruoli

    await EnsureSingleRoleAsync(userManager, admin, UserRoles.Admin);
    await EnsureSingleRoleAsync(userManager, editor, UserRoles.Editor);
    await EnsureSingleRoleAsync(userManager, normalUser, UserRoles.User);

    // creiamo alcuni interessi per ogni utente

    await EnsureInterestExistsAsync(context, admin.Id, "Calcio");
    await EnsureInterestExistsAsync(context, admin.Id, "Csharp");
    await EnsureInterestExistsAsync(context, admin.Id, "Cinema");

    await EnsureInterestExistsAsync(context, editor.Id, "Libri");
    await EnsureInterestExistsAsync(context, editor.Id, "Angular");
    await EnsureInterestExistsAsync(context, editor.Id, "Musica");

    await EnsureInterestExistsAsync(context, normalUser.Id, "Nuoto");
    await EnsureInterestExistsAsync(context, normalUser.Id, "Viaggi");
    await EnsureInterestExistsAsync(context, normalUser.Id, "Cucina");

  }

  private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
  {
    bool exists = await roleManager.RoleExistsAsync(roleName);
    if (!exists)
    {
      IdentityRole role = new IdentityRole();
      role.Name = roleName;

      await roleManager.CreateAsync(role);
    }
  }

  private static async Task<ApplicationUser> EnsureUserExistsAsync(
      UserManager<ApplicationUser> userManager,
      string email,
      string password,
      string nomeCompleto,
      string? phoneNumber,
      bool abilitato)
  {
    // controlliamo se l'utente esiste già tramite email
    ApplicationUser? existingUser = await userManager.FindByEmailAsync(email);

    if (existingUser != null)
    {
      return existingUser;
    }

    ApplicationUser user = new ApplicationUser();
    user.UserName = email;
    user.Email = email;
    user.NomeCompleto = nomeCompleto;
    user.PhoneNumber = phoneNumber;
    user.CreatedAt = DateTime.UtcNow;
    user.Abilitato = abilitato;



    IdentityResult result = await userManager.CreateAsync(user, password);

    if (!result.Succeeded)
    {
      List<string> errors = new List<string>();

      foreach (IdentityError error in result.Errors)
      {
        errors.Add(error.Description);
      }
      string message = string.Join("|", errors);
      throw new Exception($"Errore durante il seed dell'utente {email} : {message}");
    }
    return user;
  }

  private static async Task EnsureSingleRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string targetRole)
  {
    IList<string> currentRoles = await userManager.GetRolesAsync(user);
    // rimuoviamo i ruoli classici se diversi da quello target

    for (int i = 0; i < currentRoles.Count; i++)
    {
      string currentRole = currentRoles[i];

      if (currentRole == UserRoles.Admin || currentRole == UserRoles.Editor || currentRole == UserRoles.User)
      {
        await userManager.RemoveFromRoleAsync(user, currentRole);
      }
    }
    bool alreadyInTargetRole = await userManager.IsInRoleAsync(user, targetRole);

    if (!alreadyInTargetRole)
    {
      await userManager.AddToRoleAsync(user, targetRole);
    }

  }


  private static async Task EnsureInterestExistsAsync(
   ApplicationDbContext context,
   string userId,
   string nome)
  {
    //leggiamo tutti gli interessi e controlliamo a mano
    // see questo interesse esiste già per quell'utente.

    List<Interest> interests = await context.Interests.ToListAsync();

    for (int i = 0; i < interests.Count; i++)
    {
      Interest currentInterest = interests[i];

      bool sameUser = currentInterest.UserId == userId;
      bool sameName = string.Equals(currentInterest.Nome, nome, StringComparison.OrdinalIgnoreCase);

      if (sameUser && sameName)
      {
        return;
      }
    }

    Interest interest = new Interest();
    interest.UserId = userId;
    interest.Nome = nome;

    context.Interests.Add(interest);
    await context.SaveChangesAsync();
  }

}
