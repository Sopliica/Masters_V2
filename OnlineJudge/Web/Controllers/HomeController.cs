using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Services;

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
        var tasks = _cs.GetAllTasks();
        return View(tasks.Value);
    }
}
