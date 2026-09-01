namespace AI.Evolution;

public class StrategyFamilyService
{
    public string GetFamily(string strategyVersion)
    {
        const string marker = "_MUTATED_";

        var index =
            strategyVersion.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return strategyVersion;

        return strategyVersion[..index];
    }
}
