using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Models;
using RubricaSemplice.Api.Services;

namespace RubricaSemplice.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)]

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
            return BadRequest(new{message = "utente o ruolo non valido."});
        }
        return Ok(new
        {messsage ="Ruolo aggiornato correttamente",
        email= dto.Email,
        role = newRole});
    }
}