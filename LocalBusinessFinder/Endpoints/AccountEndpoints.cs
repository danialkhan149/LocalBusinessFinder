using System.Security.Claims;
using LocalBusinessFinder.Constants;
using LocalBusinessFinder.Data;
using LocalBusinessFinder.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LocalBusinessFinder.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account");

        group.MapPost("/external-login", ([FromForm] string provider, [FromForm] string? returnUrl, [FromForm] string? role, SignInManager<ApplicationUser> signInManager) =>
        {
            var redirectUrl = $"/account/external-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}&role={Uri.EscapeDataString(role ?? AppRoles.User)}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Results.Challenge(properties, [provider]);
        }).DisableAntiforgery();

        group.MapGet("/external-callback", async (string? returnUrl, string? role, string? remoteError, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) =>
        {
            if (remoteError != null)
                return Results.Redirect($"/account/login?error=Error from external provider: {remoteError}");

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Results.Redirect("/account/login?error=Error loading external login information.");

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            
            if (signInResult.Succeeded)
            {
                var userObj = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (userObj != null)
                {
                    var roles = await userManager.GetRolesAsync(userObj);
                    var dest = roles.FirstOrDefault() switch
                    {
                        AppRoles.Admin => "/admin",
                        AppRoles.BusinessOwner => "/owner",
                        AppRoles.User => "/user",
                        _ => "/"
                    };
                    return Results.Redirect(string.IsNullOrEmpty(returnUrl) || returnUrl == "/" ? dest : returnUrl);
                }
                return Results.Redirect(returnUrl ?? "/");
            }
            if (signInResult.IsLockedOut)
            {
                return Results.Redirect("/account/login?error=Account is locked out.");
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";
                
                if (string.IsNullOrEmpty(email))
                    return Results.Redirect("/account/login?error=Email claim not received from provider.");

                var user = await userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true, IsActive = true };
                    var createResult = await userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return Results.Redirect($"/account/login?error=Failed to create account: {createResult.Errors.FirstOrDefault()?.Description}");

                    var assignedRole = role == AppRoles.BusinessOwner ? AppRoles.BusinessOwner : AppRoles.User;
                    await userManager.AddToRoleAsync(user, assignedRole);
                }
                else
                {
                    if (!user.IsActive)
                        return Results.Redirect("/account/login?error=Account is disabled.");
                }

                var addLoginResult = await userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                    return Results.Redirect($"/account/login?error=Failed to link login: {addLoginResult.Errors.FirstOrDefault()?.Description}");

                await signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                
                var rolesList = await userManager.GetRolesAsync(user);
                var destUrl = rolesList.FirstOrDefault() switch
                {
                    AppRoles.Admin => "/admin",
                    AppRoles.BusinessOwner => "/owner",
                    AppRoles.User => "/user",
                    _ => "/"
                };

                return Results.Redirect(string.IsNullOrEmpty(returnUrl) || returnUrl == "/" ? destUrl : returnUrl);
            }
        });
    }
}
