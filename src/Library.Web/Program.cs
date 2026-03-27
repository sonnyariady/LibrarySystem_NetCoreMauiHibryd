using Library.UI.Pages;
using Library.UI.Services;
using Library.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// load API base url from appsettings
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? "https://localhost:65090/";

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/ping", () => "web ok");
app.MapGet("/whoami", () => "Library.Web latest build");

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(Home).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();