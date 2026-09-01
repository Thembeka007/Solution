using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class DiversityService
{
    private readonly ILogger<DiversityService> _logger;

    public DiversityService(
        ILogger<DiversityService> logger)
    {
        _logger = logger;
    }

    public bool IntroduceRandomGenome()
    {
        bool introduce =
            Random.Shared.NextDouble() < 0.10;

        if (introduce)
        {
            _logger.LogInformation(
                """
                =========================================

                RANDOM IMMIGRANT SELECTED

                Fresh genetic material will be introduced.

                =========================================
                """);
        }

        return introduce;
    }
}
