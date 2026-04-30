using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using RubricaSemplice.Api.Models;



namespace RubricaSemplice.Api.Helpers;

// si occupa di generare i token jwt per gli utenti autenticati.
public class JwtHelper
{
  private readonly IConfiguration _configuration; // leggere e gestire i valori di configurazione dell'applicazione.
  public JwtHelper(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public string GenerateToken(ApplicationUser user, IList<string> roles) // il token che dobbiamo creare deve contenere le informazioni dell'utente.
  {
    // leggiamo i dati dal file appsettings.json

    string? key = _configuration["Jwt:Key"];
    string? issuer = _configuration["Jwt:Issuer"];
    string? audience = _configuration["Jwt:audience"];

    if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
    {
      throw new Exception("Configurazione JWT mancante.");
    }


    // dentro il token mettiamo alcune informazioni utili
    List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName ?? ""));
        claims.Add(new Claim(ClaimTypes.Email, user.Email ?? ""));
    
    //Claim di Ruolo
    for(int i = 0; i < roles.Count; i++)
    {
      claims.Add(new Claim(ClaimTypes.Role, roles[i] ));
    }

       
    

    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)); // creazione chiave segreta per la firma del token.
    SigningCredentials credentials   = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // utilizzo della chiave segreta per la firma del token.

    JwtSecurityToken token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}

