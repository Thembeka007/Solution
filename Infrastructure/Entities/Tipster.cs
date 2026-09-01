namespace Infrastructure.Entities;

public class Tipster
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public decimal WinRate { get; set; }

    public decimal ROI { get; set; }

    public decimal Profit { get; set; }

    public int TotalPredictions { get; set; }

    public bool IsSmartMoney { get; set; }

    public decimal Weight { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
