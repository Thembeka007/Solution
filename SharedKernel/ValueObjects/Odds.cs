namespace SharedKernel.ValueObjects;    

public class Odds
{
    public decimal Home { get; }

    public decimal Draw { get; }

    public decimal Away { get; }

    public Odds(
        decimal home,
        decimal draw,
        decimal away)
    {
        // prevent crashing system if zero, negative aor invalid odds entered
        if (home <= 1 ||
           draw <= 1 ||
           away <= 1)
        {
            throw new ArgumentException(
                "Odds must be greater than 1.");
        }

        Home = home;
        Draw = draw;
        Away = away;
    }
}


