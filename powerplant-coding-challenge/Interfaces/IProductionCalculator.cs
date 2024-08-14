using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Interfaces;

public interface IProductionCalculator
{
    double CalculateProduction(Powerplant powerplant, double load, double windPercentage);
}