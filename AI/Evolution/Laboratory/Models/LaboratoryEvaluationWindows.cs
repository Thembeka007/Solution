namespace AI.Evolution.Laboratory.Models;

public class LaboratoryEvaluationWindows
{
    public int InSampleStartIndex { get; set; }

    public int InSampleEndIndex { get; set; }

    public int ValidationStartIndex { get; set; }

    public int ValidationEndIndex { get; set; }

    public int OutOfSampleStartIndex { get; set; }

    public int OutOfSampleEndIndex { get; set; }

    public DateTime InSampleStartUtc { get; set; }

    public DateTime InSampleEndUtc { get; set; }

    public DateTime ValidationStartUtc { get; set; }

    public DateTime ValidationEndUtc { get; set; }

    public DateTime OutOfSampleStartUtc { get; set; }

    public DateTime OutOfSampleEndUtc { get; set; }

}
