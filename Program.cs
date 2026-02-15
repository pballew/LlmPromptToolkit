using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register OllamaService with configuration from appsettings
builder.Services.AddScoped<OllamaService>(sp =>
{
    var baseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    var modelName = builder.Configuration["Ollama:DefaultModel"] ?? "llama3.1:8b";
    return new OllamaService(baseUrl, modelName);
});

builder.Services.AddScoped<PromptService>();

var app = builder.Build();

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

