namespace Betting.Configuration;

public class BankrollOptions
{
    public const string SectionName = "Bankroll";

    public decimal TotalBankroll { get; set; }

    public decimal DailyBudget { get; set; }
}
