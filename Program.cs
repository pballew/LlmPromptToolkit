using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register LoggingService as singleton
builder.Services.AddSingleton<LoggingService>();

// Register OllamaService with configuration from appsettings
builder.Services.AddScoped<OllamaService>(sp =>
{
    var cloudBaseUrl = builder.Configuration["Ollama:CloudModel:BaseUrl"] ?? "https://ollama.com";
    var cloudModelName = builder.Configuration["Ollama:CloudModel:Name"] ?? "glm-5:cloud";
    var apiKey = builder.Configuration["Ollama:CloudModel:ApiKey"];
    var loggingService = sp.GetRequiredService<LoggingService>();
    // Initialize with cloud model by default
    return new OllamaService(cloudBaseUrl, cloudModelName, apiKey, loggingService);
});

builder.Services.AddScoped<PromptService>();
builder.Services.AddScoped<JsonValidationService>();

var app = builder.Build();

// Warm up models on startup
_ = Task.Run(async () =>
{
    await Task.Delay(1000); // Wait for app to start
    var loggingService = app.Services.GetRequiredService<LoggingService>();
    var ollamaService = app.Services.GetRequiredService<OllamaService>();
    
    loggingService.Log("🚀 Initializing models...", "INFO");
    
    // Get configuration
    var localModelName = builder.Configuration["Ollama:LocalModel:Name"] ?? "llama3.1:8b";
    var localBaseUrl = builder.Configuration["Ollama:LocalModel:BaseUrl"] ?? "http://localhost:11434";
    var cloudModelName = builder.Configuration["Ollama:CloudModel:Name"] ?? "glm-5:cloud";
    var cloudBaseUrl = builder.Configuration["Ollama:CloudModel:BaseUrl"] ?? "https://ollama.com";
    var cloudApiKey = builder.Configuration["Ollama:CloudModel:ApiKey"];
    
    // Check cloud model availability
    loggingService.Log($"Checking cloud model '{cloudModelName}' availability...", "INFO");
    bool cloudAvailable = await ollamaService.CheckModelAvailabilityAsync(cloudBaseUrl, cloudModelName, cloudApiKey);
    if (cloudAvailable)
    {
        loggingService.Log("✓ Cloud model is ready", "SUCCESS");
    }
    else
    {
        loggingService.Log("⚠ Cloud model check failed", "WARN");
    }
    
    // Try to warm up local model
    loggingService.Log($"Checking local model '{localModelName}' availability...", "INFO");
    bool localAvailable = await ollamaService.CheckModelAvailabilityAsync(localBaseUrl, localModelName, null);
    if (!localAvailable)
    {
        loggingService.Log($"Local model not available. Attempting to load '{localModelName}'...", "INFO");
        var tempService = new OllamaService(localBaseUrl, localModelName, null, loggingService);
        bool pulled = await tempService.PullModelAsync(localModelName);
        if (pulled)
        {
            loggingService.Log("✓ Local model warmed up and ready", "SUCCESS");
        }
        else
        {
            loggingService.Log("⚠ Local model warmup started (may take a while)", "WARN");
        }
    }
    else
    {
        loggingService.Log("✓ Local model is ready", "SUCCESS");
    }
});

// Configure URLs
app.Urls.Add("http://localhost:5123");
app.Urls.Add("https://localhost:5124");

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

