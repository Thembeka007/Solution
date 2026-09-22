using Ingestion.FootballData.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Ingestion.FootballData.Services;

public class FootballDataService
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _config;

    private readonly ILogger<FootballDataService>
        _logger;

    public FootballDataService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<FootballDataService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<List<CompetitionItemDto>>
        GetCompetitionsAsync()
    {
        var apiKey =
            _config["FootballData:Api_Key"];

        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.football-data.org/v4/competitions");

        request.Headers.Add(
            "X-Auth-Token",
            apiKey);

        var response =
            await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        _logger.LogInformation(
            "FootballData returned {length} chars",
            json.Length);

        var result =
            JsonSerializer.Deserialize<
                CompetitionsResponseDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


        return result?.Competitions
               ?? new List<CompetitionItemDto>();
    }

    public async Task<List<MatchDto>> GetMatchesAsync(
        string competitionCode)
    {
        var apiKey =
            _config["FootballData:Api_Key"];

        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.football-data.org/v4/competitions/" +
                $"{competitionCode}/matches");

        request.Headers.Add(
            "X-Auth-Token",
            apiKey);

        _logger.LogInformation(
            "Requesting competition {competition}",
            competitionCode);

        var response =
            await _httpClient.SendAsync(
                request);

        _logger.LogInformation(
            "Response status {status}",
            response.StatusCode);

        if (response.StatusCode ==
            HttpStatusCode.TooManyRequests)
        {
            var retryAfter =
                    response.Headers.RetryAfter;

            var responseBody =
                await response.Content
                    .ReadAsStringAsync();

            _logger.LogWarning(
                """
                FootballData rate limit reached.

                Competition={competition}

                RetryAfter={retryAfter}

                ResponseBody={responseBody}
                """,
                competitionCode,
                retryAfter?.ToString() ?? "Not provided",
                responseBody);

            return new List<MatchDto>();
        }

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        var result =
            JsonSerializer.Deserialize<
                CompetitionMatchesResponseDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        _logger.LogInformation(
            "FootballData returned {count} matches",
            result?.Matches.Count ?? 0);

        return result?.Matches
            ?? new List<MatchDto>();
    }

    public async Task<MatchDto?> GetMatchAsync(
        int fixtureId)
    {
        var apiKey =
            _config["FootballData:Api_Key"];

        var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.football-data.org/v4/matches/{fixtureId}");

        request.Headers.Add(
            "X-Auth-Token",
            apiKey);

        var response =
            await _httpClient.SendAsync(
                request);

        if (response.StatusCode ==
            HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning(
                "FootballData rate limit reached.");

            return null;
        }


        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer.Deserialize<MatchDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

}
