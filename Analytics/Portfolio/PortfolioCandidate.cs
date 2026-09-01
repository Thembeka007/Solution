using SharedKernel.Entities;

namespace Analytics.Portfolio;

public class PortfolioCandidate
{
    public Prediction Prediction { get; set; } = null!;

    public decimal Score { get; set; }

    public decimal Confidence { get; set; }

    public string Strategy { get; set; } = "";

    public decimal ExpectedValue { get; set; }
}
