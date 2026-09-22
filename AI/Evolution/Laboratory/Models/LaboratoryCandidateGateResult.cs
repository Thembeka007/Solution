namespace AI.Evolution.Laboratory.Models;

public class LaboratoryCandidateGateResult
{
    public bool InSampleDataPassed { get; set; }

    public bool ValidationDataPassed { get; set; }

    public bool OutOfSampleDataPassed { get; set; }

    public bool DataPassed { get; set; }

    public bool ValidationProfitabilityPassed { get; set; }

    public bool OutOfSampleProfitabilityPassed { get; set; }

    public bool ProfitabilityPassed { get; set; }

    public bool ValidationAccuracyPassed { get; set; }

    public bool OutOfSampleAccuracyPassed { get; set; }

    public bool AccuracyPassed { get; set; }

    public bool InSampleRiskPassed { get; set; }

    public bool ValidationRiskPassed { get; set; }

    public bool OutOfSampleRiskPassed { get; set; }

    public bool RiskPassed { get; set; }

    public bool ValidationConsistencyPassed { get; set; }

    public bool OutOfSampleConsistencyPassed { get; set; }

    public bool ConsistencyPassed { get; set; }

    public bool Passed { get; set; }

    public List<string> Failures { get; set; }
        = new();

}
