using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Evolution;

public class MutationFeedbackService
{
    private readonly SportsBettingDbContext _db;
    
    private readonly ILogger<MutationFeedbackService> _logger;

    public MutationFeedbackService(
        SportsBettingDbContext db,
        ILogger<MutationFeedbackService> logger)
    {
        _db = db;
        
        _logger = logger;
    }

    public async Task EvaluateAsync()
    {
        var pending =
            await _db.Set<PendingMutation>()
                .Where(x => !x.Evaluated)
                .ToListAsync();
        
        foreach (var mutation in pending)
        {
            var performance =
                await _db.StrategyPerformances
                    .FirstOrDefaultAsync(x =>
                    x.StrategyVersion ==
                    mutation.StrategyVersion);
            
            if (performance == null)
                continue;
            
            decimal improvement =
                performance.ROI - mutation.StartingROI;
            
            var outcome =
                new MutationOutcome
                {
                    StrategyVersion =
                        mutation.StrategyVersion,
                    
                    ParameterName =
                        mutation.ParameterName,
                    
                    OldValue =
                        mutation.OldValue,
                    
                    NewValue =
                        mutation.NewValue,
                    
                    PreviousROI =
                        mutation.StartingROI,
                    
                    NewROI =
                        performance.ROI,
                    
                    Improvement =
                        improvement,
                    
                    Successful =
                        improvement > 0,
                    
                    MutationUtc =
                        mutation.AppliedUtc,
                    
                    EvaluatedUtc =
                        DateTime.UtcNow
                
                };
            
            _db.Set<MutationOutcome>().Add(outcome);
            
            mutation.Evaluated =
                true;
            
            mutation.EvaluatedUtc =
                DateTime.UtcNow;
            
            _logger.LogInformation(
                
                """
                Mutation evaluated Strategy={strategy}
                Parameter={parameter}
                PreviousROI={previous}
                CurrentROI={current}
                Improvement={improvement}
                Successful={success}
                
                """,
                mutation.StrategyVersion,
                mutation.ParameterName,
                mutation.StartingROI,
                performance.ROI,
                improvement,
                improvement > 0);
        }

        await _db.SaveChangesAsync();

    }
}
