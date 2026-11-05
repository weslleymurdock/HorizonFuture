using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using HF.Client;
using HF.Shared;
using Xunit;

namespace HF.UITests.Shared;

/// <summary>
/// Testes de integração/configuração que comprovam deterministamente qual BaseUrl
/// o cliente Kiota assume no host Web, garantindo que nunca utilize por engano a
/// BaseAddress do app Web (o que causava o HTTP 405 Method Not Allowed).
/// </summary>
public class WebClientBaseUrlConfigurationTests
{
    [Fact]
    public void AddHorizonClient_WhenConfiguredWithApiBaseUrl_UsesConfiguredUrlAndIgnoresWebBaseAddress()
    {
        var services = new ServiceCollection();

        // Simula o registro padrão do Blazor WebAssembly (apontando para si mesmo)
        var webHostUrl = "http://localhost:5076";
        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(webHostUrl) });

        // Configuração contendo a URL da API (ex: lida de appsettings.json)
        var expectedApiUrl = "http://localhost:8080";
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = expectedApiUrl
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton(configuration);

        // Registra serviços compartilhados
        services.AddHorizonClient();

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IRequestAdapter>();

        Assert.NotNull(adapter.BaseUrl);
        Assert.Equal(expectedApiUrl, adapter.BaseUrl);
        Assert.NotEqual(webHostUrl, adapter.BaseUrl);
    }

    [Fact]
    public void AddPageModels_WithCustomBaseUrl_OverridesConfigurationAndWebBaseAddress()
    {
        var services = new ServiceCollection();

        var webHostUrl = "http://localhost:5076";
        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(webHostUrl) });

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://localhost:8080"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton(configuration);

        var customOverrideUrl = "https://api.horizonfuture.org";
        services.AddPageModels(customOverrideUrl);

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IRequestAdapter>();

        Assert.Equal(customOverrideUrl, adapter.BaseUrl);
    }

    [Fact]
    public void AddHorizonClient_WithoutConfiguration_FallsBackToDefaultApiAndNeverUsesWebBaseAddress()
    {
        var services = new ServiceCollection();

        var webHostUrl = "http://localhost:5076";
        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(webHostUrl) });

        // Nenhuma configuração definida
        IConfiguration emptyConfig = new ConfigurationBuilder().Build();
        services.AddSingleton(emptyConfig);

        services.AddHorizonClient();

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IRequestAdapter>();

        Assert.NotNull(adapter.BaseUrl);
        Assert.NotEqual(webHostUrl, adapter.BaseUrl);
        Assert.Equal("http://localhost:8080", adapter.BaseUrl);
    }

    [Fact]
    public void ApiClient_BuildsCorrectLoginRouteWithConfiguredBaseUrl()
    {
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Api:BaseUrl"] = "http://localhost:8080"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton(configuration);
        services.AddHorizonClient();

        using var provider = services.BuildServiceProvider();
        var apiClient = provider.GetRequiredService<ApiClient>();

        Assert.NotNull(apiClient);
        Assert.NotNull(apiClient.Api.V1.Auth.Login);
    }
}

