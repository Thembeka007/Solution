namespace Analytics.Models
{
    public class ChampionHealthResult
    {
        public string StrategyVersion { get; set; } = string.Empty;

        public string StrategyFamily { get; set; } = string.Empty;

        public decimal ROI { get; set; }

        public decimal Accuracy { get; set; }

        public bool IsMature { get; set; }

        public bool IsActive { get; set; }

        public bool IsElite { get; set; }

        public decimal MinimumRequiredROI { get; set; }

        public bool IsHealthy { get; set; }
    }
}
