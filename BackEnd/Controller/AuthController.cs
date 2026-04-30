using Microsoft.AspNetCore.Mvc;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Services;
using System.Security.Claims;


namespace RubricaSemplice.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // definisce il percorso dell'api. [controller] viene eliminato lasciando /api/Auth/ [e qui le operazioni che eseguirà].

public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        // dependency injection: le dipendenze(i services) vengono fornite in modo automatico.
        _authService = authService;
    }

    [HttpPost("register")] // mappa l'url di "register" ad un metodo di AuthService. Qui Register definisce l'endpoint
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) // IActionResult è una classe di Identity. Frombody significa che riceve il json e lo converte in ciò che il dto farà vedere
    {
        var result = await _authService.RegisterAsync(dto);

        if (!result.Succeeded)
        {
            List<string> errors = new List<string>();
            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }

            return BadRequest(errors); // metodo ereditato da ControllerBase per la gestione di una specifica tipologia di errore.
        }

        return Ok(new { message = "Registrazione completata" }); // Ritorno se la richiesta ha successo.
    }

    [HttpPost("login")] // mappa l'url di "login" ad un metodo.
    public async Task<IActionResult> login([FromBody] LoginDto dto)
    {
        AuthResponseDto? response = await _authService.LoginAsync(dto);

        if (response == null)
        {
            return Unauthorized(new { message = "Email o password non validi." });
        }

        return Ok(response); // ritorno se il login ha successo. 
    }

    [HttpGet("profile")] // la rotta profile permette la visualizzazione del profilo, in console
    public async Task<IActionResult> GetUserById()
    {
        string userId = GetUserIdFromToken(); // otteniamo l'id dell'utente

        UserProfileDto? dto = await _authService.GetUserByIdAsync(userId); // chiamata al service che ci torna il DTO del profilo

        if (dto == null)
        {
            return NotFound(new { message = "Utente non trovato" });
        }
        
        return Ok(dto);
    }

    [HttpPut("update")]

    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        string userId = GetUserIdFromToken();

        var result = await _authService.UpdateAsync(dto, userId);
        if (!result.Succeeded)
        {
            return NotFound(new { message = "Utente non trovato" });
        }

        return Ok(result);
    }

    [HttpDelete("delete")]

    public async Task<IActionResult> Delete()
    {
        string userId = GetUserIdFromToken();

        var result = await _authService.DeleteAsync(userId);

        if (!result.Succeeded)
        {
            return NotFound(new { message = "Utente non trovato." });
        }

        return Ok(result);
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