# WEBAPI RUBRICA COMPLETA V1

- ApplicationUser che estende Identity User
- Tabella Interest collegata all'utente
- Authservice
- Interest service
- Controller semplici con operazioni crud

```bash
Rubrica.Api
|─── Controllers
     |─── AuthController.cs
     |─── InterestsController.cs
|─── Data
     |───  ApplicationDbContext.cs
|
|───  Dtos
|     |───  AuthResponseDto.cs
|     |───  InterestCreateDto.cs
|     |─── InterestDto.cs
|     |───  LoginDto.cs
|     |───  RegisterDto.cs
|
|
|───  Helpers
|     |───  ApplicationUser.cs
|     |───  Interest.cs
|
|
|───  Services
|    |───  AuthService.cs
|    |─── InterestService.cs
|
|─── Program.cs
|─── appsettings.json
```

# Modelli

ApplicationUser.cs
Estende IdentityUser, che è la classe base di Identity per rappresentare un utente. Aggiungiamo alcune proprietà personalizzate. Viene mappata alla tabella Users e ha una relazione una a molti
con la tabella interests.
```C#
using MicroSoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rubrica.Api.Dtos;

[Table("Users")] // decorator che permette di definire a quale tabella appartiene la tabella.
public class ApplicationUser : IdentityUser
{
    // IdentityUser ha già: Id, UserName, Email, PasswordHas, PhoneNumber ecc

    [Required]
    [StringLength(100)]
    public string NomeCompleto {get; set;} = string.Empty;

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    // un utente può avere molti interessi

    public List<Interest> Interest {get;set;} = new List<Interest>();
}
```


Interest.cs
Rappresenta un oggetto dell'utente con un nome e un collegamento all'utente a cui appartiene. Viene mappato alla tabella interests nel database e ha una realzione molti a uno con ApplicationUser.
```C#
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rubrica.Api.Dtos;

[Table("Interests")]
public class Interest
{
    public int Id {get;set;}

    [Required]
    [StringLength(100)]
    public string Nome {get;set;} = string.Empty;

    // Con identity l'id utente è string
    [Required]
    public string UserId {get;set;} = string.Empty;

    // collegamento all'utente
    [ForeignKey("UserId")]
    public ApplicationUser? User {get;set;}
}

```
# DTO

RegisterDto.cs

Serve per fornire i dati necessari alla registrazione di un nuovo utente. Viene usato come input per l'endpoint di registrazione nell'AuthController.

```C#

using System.ComponentModel.DataAnnotations;

namespace  Rubrica.Api.Dtos;

public class RegisterDto
{
    [Required]
    [EmailAdress]
    public string Email {get; set;} = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password {get;set;} = string.Empty;

    [Required]
    [StringLegnght(100)]
    public string NomeCompleto {get; set;} = string.Empty;

    public string? PhoneNumber {get; set;}
}
```

LoginDto.cs

Serve per fornire i dati necessari per il login nell'AuthController

```C#

using System.ComponentModel.DataAnnotations;

namespace  Rubrica.Api.Dtos;

public class LoginDto
{
    [Required]
    [EmailAdress]
    public string Email {get; set;} = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password {get;set;} = string.Empty;

}
```

AuthResponseDto.cs

Serve per restituire i dati di risposta dopo una registrazione o un login riusciti. Viene usato come output per gli endpoind di registrazione e login nell'AuthController

```C#

using System.ComponentModel.DataAnnotations;

namespace  Rubrica.Api.Dtos;

public class AuthResponseDto
{
    public string Token {get; set;}        = string.Empty;
    public string UserId {get; set;}       = string.Empty;
    public string Email {get; set;}        = string.Empty;
    public string NomeCompleto {get; set;} = string.Empty;

}
```

InterestCreateDto.cs

Serve per fornire i dati necessari alla creazione o aggiornamento di un interesse. Viene usato come input per gli endpoint di creazione a aggiornamento degli interessi nell'InterestsController
```C#

using System.ComponentModel.DataAnnotations;

namespace  Rubrica.Api.Dtos;

public class InterestCreateDto
{
 [Required]
 [StringLength(100)]
 public string Nome {get;set;} = string.Empty;
}
```

InterestDto.cs
Serve per restituire i dati di un interesse. Contiene l'id e il nome dell'interesse. Viene usato come output per gli endpoint di lettura degli interessi nell'InterestsController.
```C#

using System.ComponentModel.DataAnnotations;

namespace  Rubrica.Api.Dtos;

public class InterestDto
{
 public int Id {get;set;}
 public string Nome {get;set;} = string.Empty;
}
```

#  DbContext
---
Il DBContext è la classe principale di Entity Framework che gestisce la connessione dal database e le operazioni CRUD che vengono eseguite sulle entità dai services dell'applicazione.
In questo caso ApplicationDbContext estende IdentityUsserContext per integrare Identity con il nostro modello di utente personalizzato e aggiunge un DbSet per la tabella interessi.

## Creazione DbContext:

creare un file ApplicationDbContext.cs in /Data:

```C#
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rubrica.Api.Models;

public class ApplicationDbContext : IdentityUserContext<ApplicationUser>
{
    // costruttore che accetta le opzioni di configurazione di DbContext
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
    {
 
    
    }
    // DbSet per la tabella Interessi
    public DbSet<Interest> Interests {get;set;}

}
```

# Helpers

JwtHelper.cs

JwtHelper è una classe di utilità che si occupa di generare token JWT per l'autenticazione degli utenti. Legge la chiave segreta, l'emittente e chi lo sta ricevendo dal file di configurazione appsettings.json e crea un token JWT che include informazioni dell'utente come ID, Username ecc. Il token viene firmato con HMAC SHA256 per garantire la sicurezza.

Il token viene generato automaticamente quando viene effettuato il login, e poi viene restituito al client Angular che lo userà per autenticarsi nelle richieste successive.
```C#
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Rubrica.Api.Models;

namespace Rubrica.Api;

public class JwtHelper
{
  private readonly IConfiguration _configuration;
  public JwtHelper(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public string GenerateToken(ApplicationUser user)
  {
    // leggiamo i dati dal file appsettings.sjson

    string? key = _configuration["Jwt:Key"];
    string? issuer = _configuration["Jwt:Issuer"];
    string? audience = _configuration["Jwt:audience"];

    if((string.IsNullOrEmpty(key)) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
    {
        throw new Exception("Configurazione JWT mancante.");
    }


    // dentro il token mettiamo alcune informazioni utili
    Claim[] claims = new Claim[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? "")

    };
    
    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    SigningCredentials credentials   = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    JwtSecurityToken token = new JwtSecurityToken(
        issuer   : issuer,
        audience : audience,
        claims   : claims,
        expires : DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
```

# Services

AuthService.cs
Gestisce la logica di registrazione e login degli utenti, utilizzando UserManager e SignInManager di Identity per interagire
con il database degli utenti e JwtHelper per generare i token JWT.

```C#
using Microsoft.AspNetCore.Identity;
using Rubrica.Api.Dtos;
using Rubrica.Api.Helpers;
using Rubrica.Api.Models;

namespace Rubrica.Api.Services;

using Microsoft.AspNetCore.Identity;
using Rubrica.Api.Dtos;
using Rubrica.Api.Helpers;
using Rubrica.Api.Models;

namespace Rubrica.Api.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtHelper _jwtHelper;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtHelper jwtHelper)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _jwtHelper     = jwtHelper;
    }

    /*questo è un metodo asincrono che restituisce un IdentityResult, che indica se la registrazione è riuscita o no, e contiene eventuali errori eun metodo asicrono che è un metodo che può essere
     eseguito in modo non bloccante cioè puo fare operazioni che richiedono tempo senza bloccare il thread principale dell'applicazione
    */

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
    {
        // controlliamo se esiste già un utente con questa email (await fa restare in attesa il thread finchè l'operazione non è completa)
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if(existingUser != null)
        {
            IdentityError error = new IdentityError();
            error.Description = "Email già registrata.";

            List<IdentityError> errors = new List<IdentityError>();
            errors.Add(error);
        }

        // creiamo il nuovo utente
        ApplicationUser user = new ApplicationUser();
        user.UserName     = dto.Email; // usiamo la mail anche come username
        user.Email        = dto.Email;
        user.NomeCompleto = dto.NomeCompleto;
        user.PhoneNumber = dto.PhoneNumber;
        user.CreatedAt    = DateTime.UtcNow;

        // Identity salva l'utente e crea l'has sicuro della password
        IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

        return result;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        // cerchiamo l'utente per email
        ApplicationUser? user = await _userManager.FindByEmailAsync(dto.Email);

        if(user == null)
        { 
            return null;
        }

        // controlliamo se la paswword è giusta
        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        
        if(!result.Succeeded)
        {
            return null;
        }

        // se tutto va bene creiamo il token
        string token = _jwtHelper.GenerateToken(user);

        AuthResponseDto response = new AuthResponseDto();
        response.Token        = token;
        response.UserId       = user.Id;
        response.Email        = user.Email ?? "";
        response.NomeCompleto = user.NomeCompleto;
        return response;
    }
}
```

# Services

InterestService.cs
InterestService gestisce la logica di business per le operazioni CRUD sugli interessi degli utenti. Utilizza ApplicationDBContext per interagire con il database e implemeta
metodi asincroni per ottenere,creare aggiornare e cancellare interessi, assicurandosi che ogni operazione sia autorizzata solo per l'utente a cui appartiene l'interesse.

```C#
using Rubrica.Api.Data;
using Rubrica.Api.Dtos;
using Rubrica.Api.Models;

namespace Rubrica.Api.Services;

public class InterestService
{
    private readonly ApplicationDbContext _context;

    public InterestService(ApplicationDbContext context)
    {
         _context = context;
    }

    public async Task<List<InterestDto>> GetAllByUserIdAsync(string userId)
    {
        List<InterestDto> result = new List<InterestDto>();
        //prendiamo tutti gli interessi dal database
        List<Interest> allInterests = _context.Interests.ToList();

        //filtriamo a mano solo quelli dell'utente loggato
        for(int i = 0; i < allInterests.Count; i++)
        {
            Interest currentInterest = allInterests[i];

            if(currentInterest.UserId == userId)
            {
                InterestDto dto = new InterestDto();
                dto.Id          = currentInterest.Id;
                dto.Nome        = currentInterest.Nome;

                result.Add(dto);
            }
        }
        return await Task.FromResult(result);
    }

    public async Task<InterestDto?> GetByIdAsync (int id, string userId)
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if(interest == null)
        {
            return null;
        }

        // controlliamo che l'interesse appartenga all'utente giusto
        if(interest.UserId != userId)
        {
            return null;
        }

        InterestDto dto = new InterestDto();
        dto.Id   = interest.Id;
        dto.Nome = interest.Nome;
        
        return dto;

    }

    public async Task<InterestDto?> CreateAsync(InterestCreateDto dto, string UserId)
    {
        // Controllo semplice per evitare doppioni

        List<Interest> allInterests = _context.Interests.ToList();

        for (int i = 0; i < allInterests.Count; i++)
        {

            Interest currentInterest = allInterests[i];
            if(currentInterest.UserId == UserId && currentInterest.Nome == dto.Nome)
            {
                return null;
            }
        }
    
       Interest interest = new Interest();
       interest.Nome = dto.Nome;
       interest.UserId = UserId;

       _context.Interests.Add(interest);
       await _context.SaveChangesAsync();

       InterestDto result = new InterestDto();
       result.Id = interest.Id;
       result.Nome = interest.Nome;

       return result;
    }

    public async Task<InterestDto?> UpdateAsync(int id, InterestCreateDto dto, string userid)
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if(interest == null)
        {
            return null;
        }

        if(interest.UserId != null)
        {
            return null;
        }

        interest.Nome = dto.Nome;

        await _context.SaveChangesAsync();

        InterestDto result = new InterestDto();
        result.Id = interest.Id;
        result.Nome = interest.Nome;

        return result;
    }

    public async Task<bool> DeleteAsync (int id, string userId)
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if(interest == null)
        {
            return false;
        }

        if(interest.UserId != userId)
        {
            return false;
        }

        _context.Interests.Remove(interest);

        await _context.SaveChangesAsync();

        return true;
    }

}
```

# Controllers

AuthController

In questa applicazione i controller gesticono le richieste HTTP e restituiscono risposte. AuthController si occupa di gestire le operazioni di registrazione e login degli utenti,
utilizzando AuthService per eseguire la logica di business e restituendo i risultati al client Angular.

```C#

using Microsoft.AspNetCore.Mvc;
using Rubrica.Api.Dtos;
using Rubrica.Api.Services;

namespace Rubrica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if(!result.Succeeded)
        {
            List<string> errors = new List<string>();
            foreach(var error in result.Errors)
            {
                errors.Add(error.Description);
            }
            
            return BadRequest(errors);
        }

        return Ok( new { message = "Registrazione completata"});
    }

    [HttpPost("login")]
    public async Task <IActionResult> login([FromBody] LoginDto dto)
    {
        AuthResponseDto? response = await _authService.LoginAsync(dto);

        if(response == null)
        {
            return Unauthorized ( new { message = "Email o password non validi."});
        }

        return Ok(response);
    }
}

```
InterestController.cs

Gestisce le operazioni CRUD sugli interessi degli utenti. Utilizza InterestService per eseguire la logica di business e restiruisce i risultati al client Angular. Tutti gli endpoint sono
protetti con l'attributo [Authorize], quindi è necessario essere autenticati con un token JWT valido per accedervi.

```C#

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rubrica.Api.Dtos;
using Rubrica.Api.Services;

namespace Rubrica.Api.Controllers;

[ApiController]
[Route("api/[controler]")]
[Authorize]

public class InterestsController : ControllerBase
{
    private readonly InterestService _interestService;

    public InterestsController(InterestService interestService)
    {

        _interestService = interestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string userId = GetUserIdFromToken();

        List<InterestDto> interests = await _interestService.GetAllByUserIdAsync(userId);

        return Ok(interests);
    }

    [HttpGet("{id}")]
    public async Task <IActionResult> GetById(int id)
    {
        string userId = GetUserIdFromToken();

        InterestDto? interest = await _interestService.GetByIdAsync(id, userId);

        if(interest == null)
        {
            return NotFound (new { message = "Interesse non trovato."});
        }

        return Ok(interest);
    }

    [HttpPost]
    public async Task <IActionResult> Create([FromBody] InterestCreateDto dto)
    {
        string userId = GetUserIdFromToken();

        InterestDto? result = await _interestService.CreateAsync(dto, userId);

        if(result == null)
        {
            return BadRequest( new { message = "Interesse già presente oppure non valido"});
        }

        return CreatedAtAction(nameof(GetById),new{ id = result.Id}, result);
    }

    [HttpPut("{id}")]

    public async Task<IActionResult> Update (int id, [FromBody] InterestCreateDto dto)
    {
        string userId = GetUserIdFromToken();

        InterestDto? result = await _interestService.UpdateAsync(id, dto, userId);
        if( result == null)
        {
            return NotFound(new {message = "Interesse non trovato"});
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete (int id)
    {
        string userId = GetUserIdFromToken();

        bool deleted = await _interestService.DeleteAsync(id, userId);

        if(!deleted)
        {
            return NotFound(new {message = "Interesse non trovato."});
        }

        return NoContent();
    }

    private string GetUserIdFromToken()
    {

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId non trovato nel token");
        }

        return userId;
    }
}
```

# Program Cs

```C#
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rubrica.Api.Data;
using Rubrica.Api.Helpers;
using Rubrica.Api.Models;
using Rubrica.Api.Services;
using Rubrica.Api.Dtos;
using Rubrica.Api.Seed;

var builder = WebApplication.CreateBuilder(args);

// aggiunge i controller
builder.Services.AddControllers();

// Configurazione DbContext con SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurazione CORS per permettere al frontend Angular di accedere all'api
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

//Configura Identity per gli utenti

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    //Regole password semplice per fare pratica
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
})
.AddSignInManager<SignInManager<ApplicationUser>>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//configurazione JWT

string? jwtKey      = builder.Configuration["Jwt:Key"];
string? jwtIssuer   = builder.Configuration["Jwt:Issuer"];
string? jwtAudience = builder.Configuration["Jwt: Audience"];

if(string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new Exception("Configurazione JWT mancante in appsettings.json");
}

// Configurazione autenticazione JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options=>
{
  options.TokenValidationParameters = new TokenValidationParameters
 {
    // controlla che il token sia stato emesso dall'issuer corretto
    ValidateIssuer = true,

    // controlla che il token sia destinato all'audience corretta
    ValidateAudience = true,

    //controlla che il token non sia scaduto
    ValidateLifetime = true,

    //controlla la firma del Token
    ValidateIssuerSigningKey = true,

    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))

 };
});

//abilita l'autorizzazione con [Authorize]
builder.Services.AddAuthorization();

// Dependency Injection : registriamo  services e helper

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InterestService>();
builder.Services.AddScoped<JwtHelper>();

var app = builder.Build();

app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// applica automaticamente le migration all'avvio

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


// Richiama il seed iniziale con alcuni utenti demo e i loro interessi. Se i dati esistono già non vengono duplicati.

await DataSeeder.SeedAsync(app.Services);
app.Run();

```

# Seed/DataSeeder.cs

DataSeeder è una classe statica che si occupa di popolare il database con dati iniziali per facilitare i test e lo sviluppo. il metodo SeedAsync crea alcuni utenti demo e interessi associati
a quegli utenti, ma prima controlla se esistono già per evitare duplicazioni. Viene chiamato all'avvio dell'applicazione dopo aver applicato le migrazioni al database.

```C#
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rubrica.Api.Data;
using Rubrica.Api.Models;

namespace Rubrica.Api.Seed;

public static class DataSeeder
{
    // questo metodo crea utenti e interessi iniziali. Se i dati esistono già, non li duplica.
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // creiamo il database se non esiste ancora

        await context.Database.EnsureCreatedAsync();

        // creiamo alcuni utenti demo
        ApplicationUser utente1 = await CreateUserIfNotExistsAsync(
            userManager,
            "utente1@gmail.com",
            "123456",
            "Utente uno",
            "3331234567");

            ApplicationUser utente2 = await CreateUserIfNotExistsAsync(
            userManager,
            "utente2@gmail.com",
            "123456",
            "utente due",
            "3332354567");

            ApplicationUser utente3 = await CreateUserIfNotExistsAsync(
            userManager,
            "untente3@gmail.com",
            "123456",
            "utente tre",
            "3331894567");

            // creiamo alcuni interessi per ogni utente

            await CreateInterestIfNotExistsAsync(context, utente1.Id, "Calcio");
            await CreateInterestIfNotExistsAsync(context, utente1.Id, "Csharp");
            await CreateInterestIfNotExistsAsync(context, utente1.Id, "Cinema");

            await CreateInterestIfNotExistsAsync(context, utente2.Id, "Libri");
            await CreateInterestIfNotExistsAsync(context, utente2.Id, "Angular");
            await CreateInterestIfNotExistsAsync(context, utente2.Id, "Musica");

            await CreateInterestIfNotExistsAsync(context, utente3.Id, "Nuoto");
            await CreateInterestIfNotExistsAsync(context, utente3.Id, "Viaggi");
            await CreateInterestIfNotExistsAsync(context, utente3.Id, "Cucina");

        
    }

    private static async Task<ApplicationUser> CreateUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string nomeCompleto,
        string? phoneNumber)
    {
        // controlliamo se l'utente esiste già tramite email
        ApplicationUser? existingUser = await userManager.FindByEmailAsync(email);

        if( existingUser != null)
        {
            return existingUser;
        }

        ApplicationUser user = new ApplicationUser();
        user.UserName = email;
        user.Email = email;
        user.NomeCompleto = nomeCompleto;
        user.PhoneNumber = phoneNumber;
        user.CreatedAt = DateTime.UtcNow;

    

    IdentityResult result = await userManager.CreateAsync(user, password);

    if(!result.Succeeded)
    {
      List<string> errors = new List<string>();

      foreach( IdentityError error in result.Errors)
      {
        errors.Add(error.Description);
      }
      string message = string.Join("|", errors);
      throw new Exception($"Errore durante la creazione dell'utente {email} : {message}");
    }
     return user;
  }

   private static async Task CreateInterestIfNotExistsAsync(
    ApplicationDbContext context,
    string userId,
    string nome)
    {
      //leggiamo tutti gli interessi e controlliamo a mano
      // see questo interesse esiste già per quell'utente.

      List<Interest> interests = await context.Interests.ToListAsync();

      for(int i = 0; i < interests.Count; i++)
      {
        Interest currentInterest = interests[i];

        bool sameUser = currentInterest.UserId == userId;
        bool sameName = string.Equals(currentInterest.Nome, nome, StringComparison.OrdinalIgnoreCase);

        if(sameUser && sameName)
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

```
# appsettings.json

```json

{
    "ConnectionStrings" :{
        "DefaultConnection": "Data Source=rubrica.db"
    },
    "Jwt":{
        "Key": "questa-e-una-chiave-molto-lunga-di-almeno-32-caratteri",
        "Issuer": "RubricaApi",
        "Audience": "RubricaAngular"
    },

    "Logging" :{
        "LogLevel":{
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
}
```

# MIGRAZIONI

Per come abbiamo impostato il program cs quello che avviene è questo:

- Prima di avviare l'applicazione la prima volta dobbiamo eseguire la migrazione iniziale per creare il database.
Questo processo genera una migrazione che descrive le modifiche al database e poi applica quelle modifiche al database stesso.

```bash
dotnet ef migrations add InitialCreate
dotnet ef database updata
```
Il database viene creato senza dati perchè non è statom ancora invocato il seed, che viene eseguito nel program.cs quando avviamo l'applicazione con dotnet run.
Da questo momento ogni modifica alle tabelle del database con aggiunta di campi deve essere segita da una nuova migrazione con i relativi comandi di configurazione e applicazione.


# Pacchetti da installare
```bash
dotnet tool install --global dotnet-ef ( basta installare una volta per sistema)

// Entity Framework Core e SQLite
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

// strumenti per migrazione
dotnet add package Microsoft.EntityFrameworkCore.Design
// una volta che aggiungi qualcosa al database
dotnet ef migrations add NomeMigration
dotnet ef database update

// JWT e autenticazione
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt

```

# COMANDI CURL

Aprire il terminale con gitbash, scaricare chocolatey e poi fare choco install jq nel powershell come amministratore.

Gli attributi:
- -H indica che stiamo inviando i dati in formato JSON.
- -d contiene i dati, in questo caso l'email e la password dell'utente che vogliamo loggare.

```bash
TOKEN=$(curl -s -X POST "http://localhost:5067/api/Auth/login" \
-H "Content-Type: application/json" \
-d '{"email": "utente1@gmail.com", "password":"123456"}' | jq -r '.token')
```

controlla il token: 
```bash
echo $TOKEN
```

Leggi interessi
```bash
curl -X GET "http://localhost:5067/api/Interests" \
-H "Authorization: Bearer $TOKEN"
```

Crea Interesse
```bash
curl -X POST "http://localhost:5067/api/Interests" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer $TOKEN" \
-d '{"nome":"Calcio"}'
```

Modifica interesse
```bash
curl -X PUT "http://localhost:5067/api/Interests"/1 \
-H "Content-Type: application/json" \
-H "Authorization: Bearer $TOKEN" \
-d '{"nome":"Nuoto"}'
```

Elimina interesse
```bash
curl -X DELETE "http://localhost:5067/api/Interests"/1 \
-H "Authorization: Bearer $TOKEN"
```
Crea Utente
```bash
curl -X POST "http://localhost:5067/api/Auth/register" \
-H "Content-Type: application/json" \
-d '{"email":"mario@email.com","password":"123456","nomeCompleto":"Mario Rossi","phoneNumber":"334343454", "abilitato":"true"}'
```
Modifica utente.
```bash
curl -X PUT "http://localhost:5067/api/Auth/update" \
-H "Content-Type: application/json"  \
-H "Authorization: Bearer $TOKEN" \
-d '{"nomeCompleto":"Mario Rossi Aggiornato","phoneNumber":"3346548732", "abilitato": false}'
```
Elimina utente(non implementato, ma si potrebbe fare aggiungendo un endpoint DELETE in AuthController)

```bash
curl -X DELETE "http://localhost:5067/api/Auth/delete" \
-H "Authorization: Bearer $TOKEN"
```

Stampa utente aggiornato:
```bash
curl -X GET "http://localhost:5067/api/Auth/profile" \
-H "Authorization: Bearer $TOKEN"
```

# App WebApi - Ruoli con Identity

Per aggiungere i ruoli "classici" con Identity:

- Il DbContext deve supportare i ruoli, quindi non più IdentityUserContext ma IdentityDbContext<ApplicationUser, IdentityRole, string>
- In Program.Cs si deve registrare Identity con .AddRoles()
- Fare il seed di ruoli Admin, Editor, User con RoleManager
- Quando registri o crei utenti, devi assegnarli a un ruolo con UserManager.AddToRoleAsync(...)
- Visto che nella tua Api usi un JWT Custom, devi mettere anche i ruoli dentro al token altrimenti [Authorize(Roles = "")] non funzionerà con il bearer token che emetti tu.
  In Asp.NET Core i ruoli vengono usati dall'autorizzazione role-based tramite il parametro Roles di [Authorize] e i servizi ruolo si attivano con AddRoles.
- Non serve creare una classe ApplicationRole: per i ruoli clasici Admin, Editor, User basta IdentityRole.

## Cosa cambia nella pratica

Con queste modifiche succede questo:
- Chi si registra normalmente entra nel ruolo User.

Il seed crea i tre ruoli:

- Admin.
- Editor.
- User.
- Il seed crea utenti demo e assegna il ruolo giusto-
- Il login genera un token che contiene anche il ruolo.

puoi proteggere endpoint cosi:
- solo admin: [Authorize(Roles = UserRoles.Admin)].
- admin o editor: [Authorize(Roles = UserRoles.AdminOrEditor)]

## Model/UserRoles.cs

```C#

namespace RubricaSemplice.Api.Models;

public static class UserRoles
{
    //Costanti semplici per evitare errori di scrittura nei nomi ruolo
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string User   = "User";

    // Comanda costante da usare in [Authroize(Roles = ""]
    public const string AdminOrEditor = "Admin, Editor";
}
```

## Data/ApplicationDbContext.cs (modificato)

Prima usavamo IdentityUserContet, che va bene per utenti senza ruoli. Per usare i ruoli serve passare a IdentityDbContext<ApplicationUser,IdentityRole,string>

```C#
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Data;

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
}

```

## Dtos/AuthResponseDto.cs (modificato)

Per includere i ruoli nel token, dobbiamo modificare il DTO di risposta del login per restituire anche i ruoli dell'utente.
Aggiungiamo Role così al login vedi anche il ruolo corrente.

```C#

namespace RubricaSemplice.Api.Dtos;

public class AuthResponseDto
{
    public string Token{get;set;}        = string.Empty;
    public string UserId{get;set;}       = string.Empty;
    public string Email{get;set;}        = string.Empty;
    public string NomeCompleto{get;set;} = string.Empty;
    public string Role{get;set;}         = string.Empty;

}
```
## Dtos/ChangeUserRoleDto.cs

Questo DTO serve per cambiare ruolo ad un utente da un endpoint admin.

```C#
using System.ComponentModel.DataAnnotations;
namespace RubricaSemplice.Api.Dtos;

public class ChangeUserRoleDto
{
    [Required]
    [EmailAdress]
    public string Email {get;set;} = string.Empty;

    [Required]
    public string NewRole {get;set;} = string.Empty;

}
```
## Helpers/JwtHelper.cs (modificato)

Il cambiamento importante è che il metodo ora riceve anche i ruoli e li inserisce nel token come claim di ruolo. Con l'autorizzazione role-based di ASP.NET Core, [Authorize(Roles = "")]
funziona in base ai role claims presenti nel principal/ticket; nel tuo caso, dato che il principal arriva da un JWT custom emesso dalla tua API, i ruoli devono essere inclusi nel JWT al login.

```C#
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
```

# Services/AuthService.cs (modificato)

Qui facciamo due cose:
- In register assegniamo sempre il ruolo User
- in login leggiamo i ruoli con GetRolesAsymc e li mettiamo nel token

UserManager.AddtoRoleAsync è il meotdo standard per aggiungere un utente a un ruolo.

```C#
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


```

## Services/UserRoleService.cs

Questo servizio serve per cambiare il ruolo ad un utente esistente. Lo facciamo semplice: rimuoviamo gli eventuali ruoli classici già presenti e assegnamo quello nuovo.

```C#
using Microsoft.AspNetCore.Identity;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Services;

public class UserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public userRoleService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> ChangeUserRoleAsync(ChangeUserRoleDto dto)
    {
        //controllo base sul nome ruolo
        if(dto.NewRole != UserRoles.Admin && dto.NewRole != UserRoles.Editor && dto.NewRole != UserRoles.User)
        {
            return null;
        }

        ApplicationUser? user = await _uerManager.FindByeEmalAsync(dto.email);
        if(user == null)
        {
            return null;
        }
        IList<string> currentRoles = await _userManager.GetRolesAsync(user);

        // rimuoviamo i ruoli classici già presenti
        for(int i = 0; i < currentRoles.Count>; i++)
        {
            string currentRole = currentRoles[i];
            if(currentRole == userRoles.Admin || currentRole == userRoles.Editor || currentRole == userRoles.User)
             {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
             }
        }

        // assegniamo il nuovo ruolo

        IdentityResult addResult = await _userManager.AddToRoleAsync(user, dto.NewRole);
        if(!addResult.Succeeded)
        {
            return null;
        }

        return dto.NewRole;
    }
}
```

## Controllers/AdminUsersController.cs

Questo controller serve per gestire gli utenti da parte di un admin, in particolare per cambiare il ruolo di un utente.

```C#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Models;
using RubricaSemplice.Api.Services;

namespace RubricaSemplice.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authroize(Roles = UserRoles.Admin)]

public class AdminUsersController : ControllerBase
{
    private readonly UserRoleService _userRoleService;
    
    public AdminUsersController(UserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpPut("change-role")]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeUserRoleDto dto)
    {
        string? newRole = await _userRoleService.ChangeUserRoleAsync(dto);
        if(newRole == null)
        {
            return BadRequest(new{message = "utente o ruolo non valido."}):
        }
        return Ok(new
        {messsage ="Ruolo aggiornato correttamente",
        email= dto.Email,
        role = newRole});
    }
}

```

## Controllers/InterestsContoller.cs(modificato)
- GET lo lasciamo a tutti gli utenti autenticati.
- POST, PUT, DELETE li facciamo fare solo ad admin o editor.
Questo è solo un esempio di uso dei ruoli; la sintassi Roles = "Admin, Editor" consente accesso a uno dei due ruoli.

```C#
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Services;

namespace RubricaSemplice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class InterestsController : ControllerBase
{
    private readonly InterestService _interestService;

    public InterestsController(InterestService interestService)
    {

        _interestService = interestService; // rende pubblica la variabile alla creazione di un nuovo oggetto di tipo InterestsController
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string userId = GetUserIdFromToken(); // serve per capire chi è autenticato

        List<InterestDto> interests = await _interestService.GetAllByUserIdAsync(userId);

        return Ok(interests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        string userId = GetUserIdFromToken();

        InterestDto? interest = await _interestService.GetByIdAsync(id, userId);

        if (interest == null)
        {
            return NotFound(new { message = "Interesse non trovato." });
        }

        return Ok(interest);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.AdminOrEditor)]
    public async Task<IActionResult> Create([FromBody] InterestCreateDto dto) // IActionResult è una classe di Identity. Frombody significa che riceve il json e lo converte in ciò che il dto farà vedere
    {
        string userId = GetUserIdFromToken(); 

        InterestDto? result = await _interestService.CreateAsync(dto, userId);

        if (result == null)
        {
            return BadRequest(new { message = "Interesse già presente oppure non valido" }); // BadRequest è uno dei metodi di ControllerBase che tornano degli status di errore.
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.AdminOrEditor)]
    
    public async Task<IActionResult> Update(int id, [FromBody] InterestCreateDto dto)
    {
        string userId = GetUserIdFromToken();

        InterestDto? result = await _interestService.UpdateAsync(id, dto, userId);
        if (result == null)
        {
            return NotFound(new { message = "Interesse non trovato" });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.AdminOrEditor)]
    
    public async Task<IActionResult> Delete(int id)
    {
        string userId = GetUserIdFromToken();

        bool deleted = await _interestService.DeleteAsync(id, userId);

        if (!deleted)
        {
            return NotFound(new { message = "Interesse non trovato." });
        }

        return NoContent();
    }

    private string GetUserIdFromToken()
    {

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId non trovato nel token");
        }

        return userId;
    }
}
```
## DataSeeder(modificato)
Questo è il file più importante per i ruoli.
Con RoleManager crei i ruoli se mancano, con UserManager crei gli utenti e li assegni ai ruoli. L'uso di RoleManager per gestire i ruoli e UserManager.AddToRoleAsync per assegnare
utenti ai ruoli è il pattern standard di Identity.

```C#
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


```

## Program.cs(modificato)

Qui ci sono tre cambiamenti fondamentali:
- AddRoles()
- Registrazione del nuovo UserRoleService
- Chiamata al seed ruoli + utenti

```C#
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RubricaSemplice.Api.Data;
using RubricaSemplice.Api.Helpers;
using RubricaSemplice.Api.Models;
using RubricaSemplice.Api.Services;
using RubricaSemplice.Api.Seed;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i controller MVC / Web API
builder.Services.AddControllers();

// Configura il database SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configura Identity per usare ApplicationUser
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Regole password semplici per fare pratica
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>() // fondamentale per i ruoli.
.AddSignInManager<SignInManager<ApplicationUser>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Leggiamo i dati JWT dal file appsettings.json
string? jwtKey = builder.Configuration["Jwt:Key"];
string? jwtIssuer = builder.Configuration["Jwt:Issuer"];
string? jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new Exception("Configurazione JWT mancante in appsettings.json");
}

// Configura l'autenticazione con token JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Permette ad Angular locale di chiamare l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Registrazione dei servizi custom
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InterestService>();
builder.Services.AddScoped<UserRoleService>(); // necessario per i ruoli.

var app = builder.Build();

app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Richiama il seed iniziale con alcuni utenti demo e i loro interessi.
// Se i dati esistono già, non vengono duplicati.

using (var scope =app.Services.CreateScope())
{
    var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

}

await DataSeeder.SeedAsync(app.Services);
app.Run();
```

## Comandi migration
```bash
dotnet ef migrations add AddIdentityRoles
dotnet ef database udpate
```

Utenti seedati consigliati
Con il seed sopra puoi usare:

Login admin:

```bash
TOKEN=$(curl -s -X POST "http://localhost:5067/api/Auth/login" \
-H "Content-Type: application/json" \
-d '{"email":"utente1@gmail.com", "password":"123456"}' \
| jq -r '.token')
```
cambiare ruolo ad un utente, come admin

```bash
curl -X PUT "http://localhost:5067/api/AdminUsers/change-role" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer $TOKEN" \
-d '{"email":"utente3@gmail.com", "newRole": "Editor"}'
```
Endpoint protetto per admin o editor

```bash
curl -X POST "http://localhost:45067067/api/interests" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer IL_TOKEN_ADMIN" \
-d '{"nome":"Cinema"}'
```

