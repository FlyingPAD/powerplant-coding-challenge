using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Interfaces;

public interface IProductionCalculator
{
    decimal CalculateProduction(Powerplant powerplant, decimal load, decimal windPercentage);
}