using Microsoft.AspNetCore.Identity;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Data;
using RubricaSemplice.Api.Helpers;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Services;

// Questa classe si occupa della logica di business per la registrazione e il login.


// Questa classe si occupa della logica di business per la registrazione e il login.
public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager; // si occupa di creazione e gestione degli utenti.
    private readonly SignInManager<ApplicationUser> _signInManager; // si occupa della registrazione degli utenti

    private readonly JwtHelper _jwtHelper; // rimando all'helper per la generazione di un token JWT.

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtHelper jwtHelper, ApplicationDbContext context)
    {
        _userManager = userManager;  // Dependency injection
        _signInManager = signInManager;
        _jwtHelper = jwtHelper;

    }

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto) // Task è una classe di sistema per le operazioni asincrone( non bloccano il thread principale) che accetta un tipo, in questo caso la classe IdentityResult.
    {
        // Cerchiamo se la mail esiste già
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            // qui gestiamo l'errore nel caso in cui si inserisca una mail già registrata.
            IdentityError error = new IdentityError();
            error.Description = "Email già registrata.";

            List<IdentityError> errors = new List<IdentityError>();
            errors.Add(error);

            return IdentityResult.Failed(errors.ToArray());
        }

        // Creiamo l'utente nuovo se la mail non è presente istanziando un oggetto del modello ApplicationUser e assegnando alle sue proprietà ciò che vogliamo mostrare attraverso i DTO.
        ApplicationUser user = new ApplicationUser();
        user.UserName        = dto.Email;
        user.Email           = dto.Email;
        user.NomeCompleto    = dto.NomeCompleto;
        user.PhoneNumber     = dto.PhoneNumber;
        user.CreatedAt       = DateTime.UtcNow;
        user.Abilitato       = dto.Abilitato;

        // Identity salva l'utente e crea l'hash sicuro della password
        IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

        if(!result.Succeeded)
        {
            return result;
        }

        IdentityResult addRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.User);
        
        if(!addRoleResult.Succeeded)
        {
            return addRoleResult;
        }
        
        return result;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        // Cerchiamo l'utente con la mail
        ApplicationUser? user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return null;
        }

        // Controlliamo se la password è corretta
        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

        if (!result.Succeeded)
        {
            return null;
        }
        IList<string> roles = await _userManager.GetRolesAsync(user);
        // Nel caso in cui la password sia corretta chiamiamo il metodo per la generazione del token e costruiamo il Dto per la risposta
        string token = _jwtHelper.GenerateToken(user, roles);

        AuthResponseDto response    = new AuthResponseDto();
        response.Token              = token;
        response.UserId             = user.Id;
        response.Email              = user.Email ?? string.Empty;
        response.NomeCompleto       = user.NomeCompleto;
        response.Abilitato          = user.Abilitato;

        // nel progetto scegliamo un solo ruolo "user" quindi se c'è almeno un ruolo restituiamo il primo
        if(roles.Count >0)
        {
            response.Role = roles[0];
        }
        else
        {
            response.Role = "";
        }

        return response;
    }
     public async Task<UserProfileDto?> GetUserByIdAsync(string userId) // metodo asincrono per ottenere un utente.
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        UserProfileDto dto  = new UserProfileDto();
        dto.UserId          = user.Id;
        dto.NomeCompleto    = user.NomeCompleto;
        dto.Email           = user.Email ?? string.Empty;
        dto.PhoneNumber     = user.PhoneNumber;
        dto.Abilitato       = user.Abilitato;

        return dto;
    }

    public async Task<IdentityResult> UpdateAsync(UpdateUserDto dto, string userId) // metodo per la modifica di un utente
    {
        
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            IdentityError error = new IdentityError();
            return IdentityResult.Failed(error);
        }


        user.NomeCompleto        = dto.NomeCompleto;
        user.PhoneNumber         = dto.PhoneNumber;
        user.Abilitato           = dto.Abilitato;
        IdentityResult result = await _userManager.UpdateAsync(user); // salva le modifiche apportate al database.


        return result;
    }

    public async Task<IdentityResult> DeleteAsync(string userId) // metodo per la cancellazione di un interesse.
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            IdentityError error = new IdentityError();
            return IdentityResult.Failed(error);
        }


        IdentityResult result = await _userManager.DeleteAsync(user);

        return result;
    }
}

