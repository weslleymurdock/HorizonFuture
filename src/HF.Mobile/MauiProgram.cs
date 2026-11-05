using HF.Mobile.Auth;
using HF.Shared;
using HF.Shared.Auth;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace HF.Mobile;

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

		// Host-specific native secure storage for MAUI
		builder.Services.AddScoped<ITokenStore, MauiSecureTokenStore>();

		builder.Services.AddPageModels();

		return builder.Build();
	}
}
