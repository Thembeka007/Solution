using SharedKernel.Base;

namespace SharedKernel.Entities;

public class Strategy : Base.Entity
{
    public string Version { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal ROI { get; set; }

    public decimal WinRate { get; set; }

    public decimal Drawdown { get; set; }

    public bool IsActive { get; set; }
}