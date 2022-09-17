using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Database;

namespace OnlineJudge.Controllers;

public class CodeController : Controller
{
    public Context Ctx { get; }

    public CodeController(Context ctx)
    {
        Ctx = ctx;
    }

    public IActionResult Index()
    {
        return View();
    }
}
