using Ingestion.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Ingestion.Services;

public class FootballApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<FootballApiService>
    _logger;

    public FootballApiService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<FootballApiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }



    public async Task<string>
        GetFixturesAsync(DateTime date)
    {
        _logger.LogInformation(
            $"UTC NOW = {DateTime.UtcNow}");

        _logger.LogInformation(
            $"DATE ARG = {date:yyyy-MM-dd}");        

        var apiKey =
            _config["ApiSports:Key"];

        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"https://v3.football.api-sports.io/fixtures?date={date:yyyy-MM-dd}");

        request.Headers.Add(
            "x-apisports-key",
            apiKey);

        var response =
            await _httpClient.SendAsync(request);

        var content = await response.Content
            .ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);

        if (doc.RootElement.TryGetProperty("errors", out var errors))
        {
            if (errors.ValueKind == JsonValueKind.Object &&
                errors.TryGetProperty("plan", out var planError))
            {
                throw new Exception(
                    $"API-Sports plan restriction: {planError.GetString()}");
            }
        }

        return content;
    }
}

