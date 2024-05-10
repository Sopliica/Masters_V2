﻿using System.Text;
using OnlineJudge.Miscs;
using OnlineJudge.Consts;
using OnlineJudge.Parsing;
using OnlineJudge.Services;
using OnlineJudge.Models.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OnlineJudge.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using OnlineJudge.Models.Miscs;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OnlineJudge.Controllers;

public class CodeController : Controller
{
    private CodeService _CodeService { get; }
    private ICodeExecutorService _executor { get; }

    public CodeController(CodeService cs, ICodeExecutorService executor)
    {
        _CodeService = cs;
        _executor = executor;
    }

    [HttpGet("/Code/Libs/{langName}")]
    public async Task<IActionResult> GetLibs([FromRoute] string langName)
    {
        var libsResult = await _executor.GetLibraries(langName);

        if (!libsResult.Success)
            return Ok(new List<LibraryDetails>());

        return Ok(libsResult.Value);
    }

    [HttpGet("/Code/View/{Id}")]
    public async Task<IActionResult> View([FromRoute] Guid Id)
    {
        var assignment = _CodeService.GetAssignment(Id);

        var vm = new AssignmentViewModel(assignment.Value);
        var languagesResult = await _executor.GetLangsAndCompilers();

        if (languagesResult.Success)
        {
            vm.AvailableLanguages = languagesResult.Value;
        }
        else
        {
            // some default
            vm.AvailableLanguages = new List<LanguageDetails>
            {
                new LanguageDetails { LanguageName = "csharp", CompilerName = ".NET (main)" },
                new LanguageDetails { LanguageName = "csharp", CompilerName = ".NET 7.0.100" },
                new LanguageDetails { LanguageName = "fsharp", CompilerName = ".NET (main)" },
                new LanguageDetails { LanguageName = "fsharp", CompilerName = ".NET 7.0.100" },
                new LanguageDetails { LanguageName = "go", CompilerName = "386 gc (tip)" },
                new LanguageDetails { LanguageName = "rust", CompilerName = "rustc nightly" },
                new LanguageDetails { LanguageName = "c", CompilerName = "x86-64 gcc (trunk)" },
                new LanguageDetails { LanguageName = "c++", CompilerName = "x86-64 gcc (trunk)" },
                new LanguageDetails { LanguageName = "java", CompilerName = "jdk 18.0.0" },
                new LanguageDetails { LanguageName = "kotlin", CompilerName = "kotlinc 1.8.0" },
                new LanguageDetails { LanguageName = "haskell", CompilerName = "x86-64 ghc 9.2.2" },
            };
        }

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
        var assignment = _CodeService.GetAssignment(Id);
        return View(assignment);
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

    [HttpPost("/Code/Submission/")]
    [Authorize]
    public IActionResult SubmitSubmission([FromBody] SubmissionInput input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = _CodeService.SaveSubmission(input, Guid.Parse(userId));
        return result.Success ? Ok(new { Id = result.Value.Id }) : BadRequest(result.Error);
    }

    [Authorize]
    [HttpGet("/Code/Submission/{Id}")]
    public IActionResult GetSubmission(Guid Id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = User.IsInRole(Roles.Administrator);
        var result = _CodeService.GetSubmission(Id, userId, isAdmin);

        if (!result.Success)
        {
            return View("Error", result.Error);
        }

        bool isOutputOK = CompareResultsWithExpectedOutputs(result);
        //var expectedOutputs = result.Value.Assignment.AssignmentOutputs.Select(x => x.Text).ToList();
        //isOutputOK = OutputComparer.Compare(result.Value.Result.Output, expectedOutputs);
        return View("SubmissionView", new SubmissionViewModel { Submission = result.Value, IsOutputOK = isOutputOK });
    }

    private static bool CompareResultsWithExpectedOutputs(Result<Submission> result)
    {
        var isOutputOK = false;
        if (result.Value.Results.Count == result.Value.Assignment.TestCases.Count)
        {
            isOutputOK = true;
            for (int i = 0; i < result.Value.Results.Count; i++)
            {
                if (result.Value.Results[i].Output != result.Value.Assignment.TestCases[i].Output)
                {
                    isOutputOK = false;
                    break;
                }
            }
        }
        return isOutputOK;
    }

    [Authorize]
    [HttpGet]
    public IActionResult History()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isAdmin = this.User.IsInRole(Roles.Administrator);

        var tasks = isAdmin ?
        _CodeService.GetAllSubmissions() :
        _CodeService.GetSubmissions(userId);

        if (tasks.Success)
        {
            var ordered = isAdmin ?
                tasks.Value.OrderBy(x => x.User.Login).ToList() :
                tasks.Value.OrderByDescending(x => x.Submitted).ToList();

            return View(ordered);
        }

        return View(tasks.Value);
    }
}
