using Ingestion.DTOs;
using SharedKernel.Entities;


namespace Ingestion.Normalizers;

public class MatchNormalizer
{
    public Match Normalize(ExternalMatchDto dto)
    {
        return new Match
        {
            Id = Guid.NewGuid(),

            HomeTeamId = dto.HomeTeamId,
            AwayTeamId = dto.AwayTeamId,

            KickoffUtc = dto.KickoffUtc
                .ToUniversalTime(),

            HomeScore = dto.HomeScore,
            AwayScore = dto.AwayScore,

            Status = MatchStatusMapper
                .Map(dto.Status)

        };
    }
}
