using Microsoft.AspNetCore.Identity;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Services;

public class UserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRoleService(UserManager<ApplicationUser> userManager)
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

        ApplicationUser? user = await _userManager.FindByEmailAsync(dto.Email);
        if(user == null)
        {
            return null;
        }
        IList<string> currentRoles = await _userManager.GetRolesAsync(user);

        // rimuoviamo i ruoli classici già presenti
        for(int i = 0; i < currentRoles.Count; i++)
        {
            string currentRole = currentRoles[i];
            if(currentRole == UserRoles.Admin || currentRole == UserRoles.Editor || currentRole == UserRoles.User)
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