using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class ProductionCalculatorService : IProductionCalculator
{
    public decimal CalculateProduction(Powerplant powerplant, decimal load, decimal windPercentage)
    {
        decimal production;

        if (powerplant.Type == "windturbine")
        {
            production = powerplant.Pmax * (windPercentage / 100.0m);
        }
        else
        {
            decimal maxLoad = Math.Max(powerplant.Pmin, load);

            production = Math.Min(powerplant.Pmax, maxLoad);
        }

        return production;
    }
}
