using Microsoft.AspNetCore.Mvc;

namespace OnlineJudge.Controllers;

public class HomeController : Controller
{
    public HomeController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }
}
