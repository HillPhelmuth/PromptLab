using PromptLab.Core;
using PromptLab.Core.Helpers;
using PromptLab.Core.Services;
using PromptLab.RazorLib.ChatModels;
using PromptLab.RazorLib.Shared;
using PromptLab.Web;
using PromptLab.Web.Components;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var services = builder.Services;
services.AddScoped<AppState>();
services.AddPromptLab();
services.AddRadzenComponents();
services.AddScoped<ChatStateCollection>().AddTransient<AppJsInterop>();
services.AddScoped<IFileService, BrowserStorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
