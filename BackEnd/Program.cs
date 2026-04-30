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