using Microsoft.AspNetCore.Mvc;
using OllamaClient.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OllamaClient.Controllers;

public class HomeController : Controller
{
    private readonly OllamaService _ollamaService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(OllamaService ollamaService, ILogger<HomeController> logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest([FromForm] string prompt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                ModelState.AddModelError("", "Please enter a prompt");
                return View("Index");
            }

            var startTime = DateTime.Now;
            var response = await _ollamaService.GetLlmResponseAsync(prompt);
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;

            var viewModel = new RequestResponseViewModel
            {
                Prompt = prompt,
                Response = response.Response,
                Model = response.Model,
                TimingMs = (int)elapsed.TotalMilliseconds,
                TokenCount = response.Context?.Length ?? 0,
                CreatedAt = DateTime.Now
            };

            return View("Response", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request");
            ModelState.AddModelError("", $"Error: {ex.Message}");
            return View("Index");
        }
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
