using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using B2B.Core;
using B2B.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.Web.Controllers;

public class LoginViewModel
{
    [Required, Display(Name = "User name or e-mail")]
    public string UserName { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class AccountController(AuthService auth) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Only users with the Agent role may use the ordering portal (Task 3.1).
        var (result, user) = auth.Login(vm.UserName, vm.Password, Roles.Agent);
        if (result != LoginResult.Success || user?.AgentID is null)
        {
            ModelState.AddModelError("", LockoutPolicy.Message(result == LoginResult.Success ? LoginResult.WrongRole : result));
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("AgentID", user.AgentID.Value.ToString()),
            new("AgentName", user.Agent?.AgentName ?? ""),
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return Url.IsLocalUrl(vm.ReturnUrl) ? Redirect(vm.ReturnUrl!) : RedirectToAction("Index", "Catalog");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
