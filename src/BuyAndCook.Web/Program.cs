using BuyAndCook.Application.Abstractions;
using BuyAndCook.Application.Services;
using BuyAndCook.Infrastructure.Integrations;
using BuyAndCook.Infrastructure.Persistence;
using BuyAndCook.Infrastructure.Services;
using BuyAndCook.Web.Components;
using BuyAndCook.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IRecipeDataSource, WebRecipeDataSource>();
builder.Services.AddSingleton<IRecipeProvider, RecipeDataService>();
builder.Services.AddSingleton<ShoppingListGenerator>();

builder.Services.AddScoped<IExternalLinkService, WebExternalLinkService>();
builder.Services.AddScoped<IClipboardService, WebClipboardService>();
builder.Services.AddSingleton<IDatabasePathProvider, WebDatabasePathProvider>();
builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<IMealPlanRepository, SqliteMealPlanRepository>();
builder.Services.AddSingleton<IIngredientMappingRepository, SqliteIngredientMappingRepository>();

builder.Services.AddSingleton<IGroceryProvider, SilpoGroceryProvider>();
builder.Services.AddSingleton<IGroceryProviderRegistry, GroceryProviderRegistry>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
