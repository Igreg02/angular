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
    private readonly InterestService _interestService;

    
    public AdminUsersController(UserRoleService userRoleService, InterestService interestService)
    {
        _userRoleService = userRoleService;
        _interestService = interestService;
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
        {message = "Ruolo aggiornato correttamente",
        email = dto.Email,
        role = newRole});
    }

    [HttpGet("showallinterests")]
    public async Task<IActionResult> GetAllInterestsWithUsers()
    {
        List<InterestWithUserDto> interests = await _interestService.GetAllWithUsersAsync();
        return Ok(interests);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRoleService.GetAllUsersAsync();
        return Ok(users);
    }}