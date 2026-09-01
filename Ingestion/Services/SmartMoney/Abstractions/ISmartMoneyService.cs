using Ingestion.Services.SmartMoney.DTOs;

namespace Ingestion.Services.SmartMoney.Abstractions;

public interface ISmartMoneyService
{
    Task<List<SmartMoneyConsensusDto>>
        GetConsensusPredictionsAsync(
        DateTime date);
}
