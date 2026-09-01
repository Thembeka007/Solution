using SharedKernel.Entities;

namespace Analytics.Portfolio;

public class PortfolioService
{
    public List<Prediction> BuildPortfolio(
        IEnumerable<Prediction> predictions)
    {
        return predictions
            .OrderByDescending(x => x.Confidence)
            .ToList();
    }
}
