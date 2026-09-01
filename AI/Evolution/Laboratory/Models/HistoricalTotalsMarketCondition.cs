using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Evolution.Laboratory.Models
{
    public class HistoricalTotalsMarketCondition
    {
        public Guid MatchId { get; set; }
        public DateTime KickoffUtc { get; set; }
        public string LeagueName { get; set; } = "";

        // =========================================
        // MARKET
        // =========================================
        public decimal Line { get; set; }

        // ========================================= // COVERAGE // =========================================
        public int BookmakerCount { get; set; }

        public int FreshObservationCount { get; set; }

        public double MaximumObservationAgeMinutes { get; set; }

        // ========================================= // OVER 2.5 ODDS // =========================================
        public decimal MinimumOverOdds { get; set; }

        public decimal MaximumOverOdds { get; set; }

        public decimal MedianOverOdds { get; set; }

        public decimal MeanOverOdds { get; set; }

        public decimal OverOddsRange { get; set; }

        // ========================================= // UNDER 2.5 ODDS // =========================================

        public decimal MinimumUnderOdds { get; set; }

        public decimal MaximumUnderOdds { get; set; }

        public decimal MedianUnderOdds { get; set; }

        public decimal MeanUnderOdds { get; set; }

        public decimal UnderOddsRange { get; set; }

        // ========================================= // IMPLIED PROBABILITIES // =========================================
        public decimal MedianOverImpliedProbability { get; set; }

        public decimal MedianUnderImpliedProbability { get; set; }

        // ========================================= // PROBABILITY DISPERSION // =========================================
        public decimal OverProbabilityRange { get; set; }

        public decimal UnderProbabilityRange { get; set; }

        public decimal AverageProbabilityRange { get; set; }

        // ========================================= // OVERROUND // =========================================
        public decimal MinimumOverround { get; set; }

        public decimal MaximumOverround { get; set; }

        public decimal MedianOverround { get; set; }

        public decimal MeanOverround { get; set; }

        // ========================================= // PROVIDER TIMING // =========================================
        public DateTime? EarliestProviderUpdateUtc { get; set; }

        public DateTime? LatestProviderUpdateUtc { get; set; }

        public double ProviderUpdateSpreadMinutes { get; set; }

        // ========================================= // CLASSIFICATION // =========================================
        public string CoverageCondition { get; set; } = "";
        
        public string DispersionCondition { get; set; } = "";
        
        public string OverroundCondition { get; set; } = "";



    }
}
