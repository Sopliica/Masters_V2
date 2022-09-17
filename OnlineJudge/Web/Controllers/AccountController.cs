using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Models.IO;
using OnlineJudge.Services;
using System.Security.Claims;

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
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn([FromForm] SignInInput input)
    {
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
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
