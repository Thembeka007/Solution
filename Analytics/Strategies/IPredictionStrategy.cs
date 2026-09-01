using SharedKernel.Entities;

namespace Analytics.Strategies;

public interface IPredictionStrategy
{
    string Name { get; }

    Task<List<Prediction>>
        GeneratePredictionsAsync(
            Match match);
}
