using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Database;
using OnlineJudge.Parsing;
using OnlineJudge.Services;
using System.Text;

namespace OnlineJudge.Controllers;

public class CodeController : Controller
{
    private CodeService _Cs { get; }

    public CodeController(CodeService cs)
    {
        _Cs = cs;
    }

    [HttpGet("{Id}")]
    public IActionResult View([FromRoute] Guid Id)
    {
        var assignment = _Cs.Get(Id);

        return View(assignment);
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Parse([FromForm] IFormFile file)
    {
        if (file == null)
            return BadRequest("File is null");

        var content = new StringBuilder();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
                content.AppendLine(reader.ReadLine());
        }

        var result = new Parser().Parse(content.ToString());

        if (result.Success)
        {
            var added = _Cs.Add(result.Value);

            if (added.Success)
                return RedirectToAction(nameof(View), new { Id = added.Value });
        }    

        return View(result);
    }
}
