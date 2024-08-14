using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Interfaces;

public interface ICostCalculator
{
    decimal CalculateCostPerMWh(Powerplant powerplant, Fuels fuels);
}