using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LlmPromptToolkit.Models;

namespace LlmPromptToolkit.Controllers
{
    /// <summary>
    /// Controller for managing stored prompts
    /// </summary>
    public class PromptsController : Controller
    {
        private readonly PromptService _promptService;

        public PromptsController(PromptService promptService)
        {
            _promptService = promptService;
        }

        /// <summary>
        /// GET: List all prompts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Prompt> prompts = await _promptService.GetAllPromptsAsync();
            return View(prompts);
        }

        /// <summary>
        /// GET: Show create prompt form
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new Prompt());
        }

        /// <summary>
        /// POST: Create a new prompt
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Prompt prompt, [FromForm] string? RequiredFieldsInput)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", prompt);
            }

            try
            {
                // Parse required fields from comma-separated input
                if (!string.IsNullOrWhiteSpace(RequiredFieldsInput))
                {
                    prompt.RequiredFields = new List<string>(RequiredFieldsInput
                        .Split(',')
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f)));
                }

                await _promptService.SavePromptAsync(prompt);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating prompt: {ex.Message}");
                return View("Form", prompt);
            }
        }

        /// <summary>
        /// GET: Show edit prompt form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var prompt = await _promptService.GetPromptAsync(id);
                return View("Form", prompt);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error loading prompt: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: Update an existing prompt
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Prompt prompt, [FromForm] string? RequiredFieldsInput)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", prompt);
            }

            try
            {
                // Parse required fields from comma-separated input
                if (!string.IsNullOrWhiteSpace(RequiredFieldsInput))
                {
                    prompt.RequiredFields = new List<string>(RequiredFieldsInput
                        .Split(',')
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f)));
                }

                await _promptService.SavePromptAsync(prompt);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating prompt: {ex.Message}");
                return View("Form", prompt);
            }
        }

        /// <summary>
        /// GET: Show delete confirmation (returns partial view or modal)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var prompt = await _promptService.GetPromptAsync(id);
                return View(prompt);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error loading prompt: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: Confirm and delete a prompt
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            try
            {
                await _promptService.DeletePromptAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error deleting prompt: {ex.Message}");
                return RedirectToAction("Delete", new { id });
            }
        }

        /// <summary>
        /// GET: API endpoint to get a single prompt by ID (for AJAX loading in builder)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrompt(string id)
        {
            try
            {
                var prompt = await _promptService.GetPromptAsync(id);
                return Json(prompt);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET: API endpoint to list all prompts (for AJAX dropdown loading)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPrompts()
        {
            try
            {
                var prompts = await _promptService.GetAllPromptsAsync();
                return Json(prompts);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
