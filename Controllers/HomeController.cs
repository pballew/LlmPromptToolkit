using Microsoft.AspNetCore.Mvc;
using OllamaClient.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OllamaClient.Controllers;

public class HomeController : Controller
{
    private readonly OllamaService _ollamaService;
    private readonly PromptService _promptService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(OllamaService ollamaService, PromptService promptService, ILogger<HomeController> logger)
    {
        _ollamaService = ollamaService;
        _promptService = promptService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var prompts = await _promptService.GetAllPromptsAsync();
            var viewModel = new HomeIndexViewModel
            {
                AvailablePrompts = prompts
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading prompts");
            return View(new HomeIndexViewModel { ErrorMessage = "Failed to load prompts" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest([FromForm] string prompt, [FromForm] string? selectedPromptId)
    {
        try
        {
            // If a prompt was selected from the dropdown, use it instead
            if (!string.IsNullOrWhiteSpace(selectedPromptId))
            {
                try
                {
                    var selectedPrompt = await _promptService.GetPromptAsync(selectedPromptId);
                    prompt = selectedPrompt.Content;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load selected prompt");
                    // Fall through to use manual prompt entry
                }
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                ModelState.AddModelError("", "Please enter a prompt or select one from your saved prompts");
                var prompts = await _promptService.GetAllPromptsAsync();
                var viewModel = new HomeIndexViewModel
                {
                    AvailablePrompts = prompts,
                    ErrorMessage = "Prompt is required"
                };
                return View("Index", viewModel);
            }

            var startTime = DateTime.Now;
            var response = await _ollamaService.GetLlmResponseAsync(prompt);
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;

            var responseViewModel = new RequestResponseViewModel
            {
                Prompt = prompt,
                Response = response.Response,
                Model = response.Model,
                TimingMs = (int)elapsed.TotalMilliseconds,
                TokenCount = response.Context?.Length ?? 0,
                CreatedAt = DateTime.Now
            };

            return View("Response", responseViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request");
            ModelState.AddModelError("", $"Error: {ex.Message}");
            var prompts = await _promptService.GetAllPromptsAsync();
            var viewModel = new HomeIndexViewModel
            {
                AvailablePrompts = prompts,
                Prompt = prompt,
                ErrorMessage = ex.Message
            };
            return View("Index", viewModel);
        }
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
