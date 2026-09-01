namespace Analytics.Services.Calibration;

public class ConfidenceBand
{
    public decimal LowerBound { get; set; }

    public decimal UpperBound { get; set; }

    public int Predictions { get; set; }

    public int Correct { get; set; }

    public decimal Accuracy
    {
        get
        {
            if (Predictions == 0)
                return 0m;

            return
                ((decimal)Correct /
                Predictions) * 100m;
        }
    }
}
