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

#if ANDROID
        var useEmulator = bool.TryParse(
            builder.Configuration["ApiSettings:AndroidUseEmulator"],
            out var emulatorFlag) && emulatorFlag;

        var baseUrl = useEmulator
            ? builder.Configuration["ApiSettings:AndroidEmulatorBaseUrl"] ?? "https://10.0.2.2:65090/"
            : builder.Configuration["ApiSettings:AndroidDeviceBaseUrl"] ?? "http://192.168.100.63:65091/";

        System.Diagnostics.Debug.WriteLine($"[ANDROID] Emulator: {useEmulator}");
        System.Diagnostics.Debug.WriteLine($"[ANDROID] API: {baseUrl}");

#if DEBUG
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
#else
        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });
#endif

#elif WINDOWS
        var windowsUrl = builder.Configuration["ApiSettings:WindowsBaseUrl"]
                         ?? "https://localhost:65090/";

        System.Diagnostics.Debug.WriteLine($"[WINDOWS] API: {windowsUrl}");

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(windowsUrl);
        });

#else
        var defaultUrl = builder.Configuration["ApiSettings:WindowsBaseUrl"]
                         ?? "https://localhost:65090/";

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(defaultUrl);
        });
#endif

        return builder.Build();
    }
}