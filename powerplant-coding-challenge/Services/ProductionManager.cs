using powerplant_coding_challenge.Features;

namespace powerplant_coding_challenge.Services;

public class ProductionManager
{
    // Adjusts the final production of all power plants to exactly match the load, ensuring no overproduction or underproduction.
    public static void EnsureTotalProductionMatchesLoad(List<ProductionPlanCommandResponse> responseList, decimal totalLoad)
    {
        // Calculate the current total production from all power plants.
        decimal totalProduction = responseList.Sum(r => r.Power);

        // If the total production is less than or equal to the load, adjust upwards.
        if (totalProduction < totalLoad)
        {
            foreach (var response in responseList.OrderBy(response => response.Power))
            {
                if (totalProduction >= totalLoad) break;

                decimal underProduction = totalLoad - totalProduction;
                decimal adjustment = Math.Min(underProduction, response.Power);

                response.Power += adjustment;
                totalProduction += adjustment;
            }
        }
        // If the total production exceeds the load, reduce production.
        else if (totalProduction > totalLoad)
        {
            foreach (var response in responseList.OrderByDescending(response => response.Power))
            {
                if (totalProduction <= totalLoad) break;

                decimal overProduction = totalProduction - totalLoad;
                decimal adjustment = Math.Min(overProduction, response.Power);

                response.Power -= adjustment;
                totalProduction -= adjustment;
            }
        }
    }
}