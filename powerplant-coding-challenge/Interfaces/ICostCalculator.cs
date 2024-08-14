using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Interfaces;

public interface ICostCalculator
{
    double CalculateCostPerMWh(Powerplant powerplant, Fuels fuels);
}