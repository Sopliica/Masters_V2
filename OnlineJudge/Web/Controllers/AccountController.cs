using OnlineJudge.Services;
using OnlineJudge.Models.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using OnlineJudge.Consts;

namespace OnlineJudge.Controllers;

public class AccountController : Controller
{
    private AccountService _service { get; }

    public AccountController(AccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult SignIn()
    {
        if (User.Identity.IsAuthenticated)
            return LocalRedirect("/");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn([FromForm] SignInInput input)
    {
        if (User.Identity.IsAuthenticated)
            return LocalRedirect("/");

        var result = await _service.TrySignIn(input);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(SignIn));
        }

        await HttpContext.SignInAsync
        (
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(result.Value.Claims),
            result.Value.Properties
        );

        return Redirect("/");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] SignInInput input)
    {
        var result = await _service.TryRegister(input);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(SignIn));
        }

        await HttpContext.SignInAsync
        (
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(result.Value.Claims),
            result.Value.Properties
        );

        return Redirect("/");
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public IActionResult ChangeRole()
    {
        var users = _service.GetNonActivatedUsers();

        return View(users);
    }

    [HttpPost("/Account/Activate")]
    [Authorize(Roles = Roles.Administrator)]
    public IActionResult ActivateUser([FromBody] ActiveUserInput input)
    {
        if (input != null)
            _service.ActivateUser(input.Id);

        return Ok(input);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
