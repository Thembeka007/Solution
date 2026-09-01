using Ingestion.Services.PredictionEngine.DTOs;

namespace Ingestion.Services.PredictionEngine.Abstractions;

public interface IPredictionEngine
{
    Task<List<MatchPredictionDto>>
        GeneratePredictionsAsync(DateTime date);
}
