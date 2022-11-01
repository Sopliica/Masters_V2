using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Consts;
using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;
using OnlineJudge.Parsing;
using OnlineJudge.Services;
using System.Text;

namespace OnlineJudge.Controllers;

public class CodeController : Controller
{
    private CodeService _CodeService { get; }

    public CodeController(CodeService cs)
    {
        _CodeService = cs;
    }

    [HttpGet("/Code/View/{Id}")]
    public IActionResult View([FromRoute] Guid Id)
    {
        var assignment = _CodeService.GetTask(Id);

        var vm = new AssignmentViewModel(assignment.Value);
        vm.AvailableLanguages = new List<string> { "csharp", "c++" };
        var resultMapped = new Result<AssignmentViewModel>(vm, assignment.Success, assignment.Error);
        return View(resultMapped);
    }

    [Authorize(Roles = Roles.Administrator)]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpGet("/Code/Delete/{Id}")]
    [Authorize(Roles = Roles.Administrator)]
    public IActionResult Delete([FromRoute] Guid Id)
    {
        var assignment = _CodeService.GetTask(Id);

        return View(assignment);;
    }

    [HttpPost("/Code/Delete/{Id}")]
    [Authorize(Roles = Roles.Administrator)]
    public IActionResult DeleteAction([FromForm] Guid Id)
    {
        var result = _CodeService.Remove(Id);
        return LocalRedirect("/");
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
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
            var added = _CodeService.Add(result.Value);

            if (added.Success)
                return RedirectToAction(nameof(View), new { Id = added.Value });
        }

        return View(result);
    }
}
