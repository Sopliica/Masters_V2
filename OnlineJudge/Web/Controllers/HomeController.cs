using OnlineJudge.Services;
using Microsoft.AspNetCore.Mvc;

namespace OnlineJudge.Controllers;

public class HomeController : Controller
{
    private readonly CodeService _cs;

    public HomeController(CodeService cs)
    {
        _cs = cs;
    }

    public IActionResult Index()
    {
        var tasks = _cs.GetAllAssignments();
        return View(tasks.Value);
    }
}
