using Microsoft.AspNetCore.Mvc;
using LlmPromptToolkit.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LlmPromptToolkit.Controllers;

public class HomeController : Controller
{
    private readonly OllamaService _ollamaService;
    private readonly PromptService _promptService;
    private readonly JsonValidationService _validationService;
    private readonly LoggingService _loggingService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(OllamaService ollamaService, PromptService promptService, JsonValidationService validationService, LoggingService loggingService, ILogger<HomeController> logger)
    {
        _ollamaService = ollamaService;
        _promptService = promptService;
        _validationService = validationService;
        _loggingService = loggingService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _loggingService.Log("🏠 Home page loaded", "INFO");
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
            _loggingService.Log($"Error loading prompts: {ex.Message}", "ERROR");
            return View(new HomeIndexViewModel { ErrorMessage = "Failed to load prompts" });
        }
    }

    public async Task<IActionResult> Comms()
    {
        try
        {
            _loggingService.Log("💬 Comms page loaded", "INFO");
            var prompts = await _promptService.GetAllPromptsAsync();
            var viewModel = new CommsViewModel
            {
                AvailablePrompts = prompts
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading prompts");
            _loggingService.Log($"Error loading prompts: {ex.Message}", "ERROR");
            return View(new CommsViewModel { ErrorMessage = "Failed to load prompts" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendRequest([FromForm] string prompt, [FromForm] string? selectedPromptId)
    {
        try
        {
            Prompt? selectedPrompt = null;
            // If a prompt was selected from the dropdown, use it instead
            if (!string.IsNullOrWhiteSpace(selectedPromptId))
            {
                try
                {
                    selectedPrompt = await _promptService.GetPromptAsync(selectedPromptId);
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
            
            // Ensure the model is loaded before sending the request
            await _ollamaService.EnsureModelIsLoadedAsync(_ollamaService.GetModelName());
            
            var response = await _ollamaService.GetLlmResponseAsync(prompt);
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;

            // Validate the response
            var requiredFields = selectedPrompt?.RequiredFields ?? new();
            var validationResult = await _validationService.ValidateResponseAsync(response.Response, requiredFields);

            var responseViewModel = new RequestResponseViewModel
            {
                Prompt = prompt,
                Response = response.Response,
                Model = response.Model,
                TimingMs = (int)elapsed.TotalMilliseconds,
                TokenCount = response.Context?.Length ?? 0,
                CreatedAt = DateTime.Now,
                ValidationResult = validationResult
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

    [HttpPost]
    public async Task<IActionResult> SendCommsRequest([FromBody] CommsRequestPayload payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload.Prompt))
            {
                return BadRequest(new { error = "Prompt is required" });
            }

            var startTime = DateTime.Now;
            
            // Ensure the model is loaded before sending the request
            await _ollamaService.EnsureModelIsLoadedAsync(_ollamaService.GetModelName());
            
            var response = await _ollamaService.GetLlmResponseAsync(payload.Prompt, payload.Context);
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;

            // Validate against schema if provided
            bool isSchemaValid = true;
            var validationErrors = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(payload.Schema))
            {
                try
                {
                    var (isValid, errors) = await _validationService.ValidateAgainstSchemaAsync(
                        response.Response, 
                        payload.Schema
                    );
                    isSchemaValid = isValid;
                    validationErrors = errors;
                    
                    if (!isSchemaValid)
                    {
                        _loggingService.Log($"⚠️ Response failed schema validation: {string.Join("; ", errors)}", "WARN");
                    }
                    else
                    {
                        _loggingService.Log($"✅ Response passed schema validation", "SUCCESS");
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Log($"Error validating schema: {ex.Message}", "ERROR");
                    validationErrors.Add($"Schema validation error: {ex.Message}");
                    isSchemaValid = false;
                }
            }

            return Ok(new
            {
                response = response.Response,
                timing_ms = (int)elapsed.TotalMilliseconds,
                model = response.Model,
                context = response.Context,
                isSchemaValid = isSchemaValid,
                validationErrors = validationErrors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending comms request");
            _loggingService.Log($"Error: {ex.Message}", "ERROR");
            return BadRequest(new { error = ex.Message });
        }
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// API endpoint to get logs
    /// </summary>
    [HttpGet]
    public IActionResult GetLogs()
    {
        try
        {
            var logs = _loggingService.GetLogs();
            return Json(logs);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}
