using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Services;
using RubricaSemplice.Api.Models;

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