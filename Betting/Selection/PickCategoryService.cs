using Betting.DTOs;
using SharedKernel.Enums;

namespace Betting.Selection;

public class PickCategoryService
{
    public string Determine(
        DailyPickDto pick)
    {
        if (pick.Score >= 85)        
            return "BREAD";
        

        if (pick.Score >= 70)        
            return "AVERAGE";
        

        return "LOTTERY";
    }
}
