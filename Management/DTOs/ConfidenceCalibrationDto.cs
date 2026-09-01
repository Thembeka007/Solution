namespace Management.DTOs;

public class ConfidenceCalibrationDto
{
    public decimal ConfidenceBand { get; set; }

    public int TotalPredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public decimal ActualAccuracy { get; set; }

    public decimal CalibrationAdjustment { get; set; }

    public DateTime LastUpdatedUtc { get; set; }

    // =====================================
    // DIAGNOSTICS
    // =====================================

    public bool OverConfident { get; set; }

    public bool UnderConfident { get; set; }
}
