using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Services;
using BuyAndCook.Data;
using BuyAndCook.Infrastructure.Integrations;
using BuyAndCook.Infrastructure.Persistence;
using BuyAndCook.Infrastructure.Services;
using BuyAndCook.PlatformServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BuyAndCook;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif
		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<IRecipeDataSource, MauiRecipeDataSource>();
		builder.Services.AddSingleton<IRecipeProvider, RecipeDataService>();
		builder.Services.AddSingleton<ShoppingListGenerator>();

		builder.Services.AddSingleton<IExternalLinkService, MauiExternalLinkService>();
		builder.Services.AddSingleton<IClipboardService, MauiClipboardService>();
		builder.Services.AddSingleton<IDatabasePathProvider, MauiDatabasePathProvider>();
		builder.Services.AddSingleton<SqliteConnectionFactory>();
		builder.Services.AddSingleton<IMealPlanRepository, SqliteMealPlanRepository>();
		builder.Services.AddSingleton<IIngredientMappingRepository, SqliteIngredientMappingRepository>();

		builder.Services.AddSingleton<IGroceryProvider, SilpoGroceryProvider>();
		builder.Services.AddSingleton<IGroceryProviderRegistry, GroceryProviderRegistry>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
