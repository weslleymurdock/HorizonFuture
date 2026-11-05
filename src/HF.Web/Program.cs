using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HF.Web;
using MudBlazor.Services;
using HF.Shared;
using HF.Shared.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Host-specific persistent and encrypted credential storage for Blazor WebAssembly
builder.Services.AddScoped<ITokenStore, EncryptedBrowserTokenStore>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? builder.Configuration["ApiBaseUrl"];
builder.Services.AddPageModels(apiBaseUrl);
await builder.Build().RunAsync();
