using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class CostCalculatorService : ICostCalculator
{
    public double CalculateCostPerMWh(Powerplant powerplant, Fuels fuels)
    {
        return powerplant.Type.ToLower() switch
        {
            // Cost for gas-fired powerplants includes fuel cost and CO2 emission cost.
            "gasfired" => (fuels.Gas / powerplant.Efficiency) + (0.3 * fuels.Co2),

            // Cost for turbojet powerplants is based only on kerosine consumption.
            "turbojet" => fuels.Kerosine / powerplant.Efficiency,

            // Wind turbines have no fuel cost, so their cost per MWh is effectively zero.
            "windturbine" => 0,

            // Return a very high value for unsupported powerplant types.
            _ => double.MaxValue,
        };
    }
}
