using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class ProductionCalculatorService : IProductionCalculator
{
    public double CalculateProduction(Powerplant powerplant, double load, double windPercentage)
    {
        if (powerplant.Type == "windturbine")
        {
            return powerplant.Pmax * (windPercentage / 100.0);
        }
        else
        {
            return Math.Min(powerplant.Pmax, Math.Max(powerplant.Pmin, load));
        }
    }
}
