using SharedKernel.Base;

namespace SharedKernel.Entities;

public class ConfidenceCalibration : AuditableEntity
{
    public decimal ConfidenceBand { get; set; }

    public int TotalPredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public decimal ActualAccuracy { get; set; }

    public decimal CalibrationAdjustment { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
