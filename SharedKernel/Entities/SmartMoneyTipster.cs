namespace SharedKernel.Entities;

public class SmartMoneyTipster
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public decimal ROI { get; set; }

    public decimal WinRate { get; set; }

    public decimal Profit { get; set; }

    public int TotalBets { get; set; }

    public bool IsTrusted { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}