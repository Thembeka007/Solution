using SharedKernel.Entities;

namespace AI.Evolution.Laboratory;

public class LaboratoryDatasetEvaluator
{
    public LaboratoryDatasetMetrics Evaluate(
        IEnumerable<Prediction> predictions)
    {
        var data =
            predictions
                .ToList();
        
        if (!data.Any())
        {
            return new LaboratoryDatasetMetrics();
        }
        
        int total =
            data.Count;
        
        int correct =
            data.Count(x =>
                x.IsCorrect == true);
        
        decimal accuracy =
            ((decimal)correct / total) * 100m;
        
        decimal profit =
            data.Sum(x =>
                x.ProfitLoss);
        
        decimal stake =
            data.Sum(x =>
                x.Stake);
        
        decimal roi =
            stake > 0
                ? (profit / stake) * 100m
                : 0m;
        
        return new LaboratoryDatasetMetrics
        {
            TotalPredictions =
                total,
            
            CorrectPredictions =
                correct,
            
            IncorrectPredictions =
                total - correct,
            
            Accuracy =
                accuracy,
            
            Profit =
                profit,
            
            TotalStake =
                stake,
            
            ROI =
                roi
        };
    }
}
