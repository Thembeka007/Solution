using Analytics.Models;
using Microsoft.Extensions.Logging;

namespace Analytics.Services;

public class OddsAnalysisService
{
    private readonly ILogger<OddsAnalysisService>
        _logger;

    public OddsAnalysisService(
        ILogger<OddsAnalysisService> logger)
    {
        _logger = logger;
    }

    public OddsProfile Analyse(
        decimal homeOdds,
        decimal drawOdds,
        decimal awayOdds)
    {
        var profile =
            new OddsProfile();

        profile.HomeOdds =
            homeOdds;

        profile.DrawOdds =
            drawOdds;

        profile.AwayOdds =
            awayOdds;

        return profile;
    }
}
