using Ingestion.Services.IngestionEngine.Abstractions;
using Ingestion.Services.IngestionEngine.DTOs;
using SharedKernel.Entities;
using SharedKernel.Enums;

namespace Services.IngestionEngine.Normalizers;

public class MatchNormalizer
{
    private readonly ITeamResolver _teamResolver;

    public MatchNormalizer(
        ITeamResolver teamResolver)
    {
        _teamResolver = teamResolver;
    }

    public async Task<Match> NormalizeAsync(
        ExternalMatchDto dto)
    {
        var homeTeamId =
            await _teamResolver.ResolveTeamIdAsync(
                dto.HomeTeam);

        var awayTeamId =
            await _teamResolver.ResolveTeamIdAsync(
                dto.AwayTeam);

        return new Match
        {
            Id = Guid.NewGuid(),

            HomeTeamId = homeTeamId,

            AwayTeamId = awayTeamId,

            KickoffUtc = dto.KickoffUtc
                .ToUniversalTime(),

            HomeScore = dto.HomeScore,

            AwayScore = dto.AwayScore,

            Status = MapStatus(dto.Status)
        };
    }

    private MatchStatus MapStatus(
        string status)
    {
        return status
            .ToLower()
            .Trim() switch
        {
            "live" => MatchStatus.Live,

            "finished" => MatchStatus.Completed,

            "completed" => MatchStatus.Completed,

            "ft" => MatchStatus.Completed,

            "cancelled" => MatchStatus.Cancelled,

            _ => MatchStatus.Scheduled
        };
    }
}