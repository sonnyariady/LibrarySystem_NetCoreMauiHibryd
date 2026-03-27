using Library.UI.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Net.Http;

namespace Library.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

#if ANDROID || IOS || MACCATALYST
        using var stream = FileSystem
            .OpenAppPackageFileAsync("appsettings.json")
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        builder.Configuration.AddJsonStream(stream);
#else
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#endif

#if ANDROID && DEBUG
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:65090/";
            var localhostReplace = builder.Configuration["ApiSettings:LocalhostReplace"];

            localhostReplace = string.IsNullOrWhiteSpace(localhostReplace)
                ? "10.0.2.2"
                : localhostReplace;

            if (apiBase.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                apiBase = apiBase.Replace("localhost", localhostReplace, StringComparison.OrdinalIgnoreCase);
            }

            System.Diagnostics.Debug.WriteLine($"Api BaseAddress = {apiBase}");
            client.BaseAddress = new Uri(apiBase);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
#else
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:65090/";
            var localhostReplace = builder.Configuration["ApiSettings:LocalhostReplace"];

#if ANDROID
            localhostReplace = string.IsNullOrWhiteSpace(localhostReplace)
                ? "10.0.2.2"
                : localhostReplace;
#else
            localhostReplace = string.IsNullOrWhiteSpace(localhostReplace)
                ? "localhost"
                : localhostReplace;
#endif

            if (apiBase.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                apiBase = apiBase.Replace("localhost", localhostReplace, StringComparison.OrdinalIgnoreCase);
            }

            System.Diagnostics.Debug.WriteLine($"Api BaseAddress = {apiBase}");
            client.BaseAddress = new Uri(apiBase);
        });
#endif

        return builder.Build();
    }
}