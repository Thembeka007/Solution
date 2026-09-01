using SharedKernel.Base;
using SharedKernel.Enums;

namespace SharedKernel.Entities;

public class Bet : Base.Entity
{
    public Guid PredictionId { get; set; }

    public decimal Stake { get; set; }

    public decimal Odds { get; set; }

    public decimal PotentialPayout { get; set; }

    public decimal ActualPayout { get; set; }

    public bool Won { get; set; }

    public BetCategory Category { get; set; }

    public bool CashedOut { get; set; }
}