namespace powerplant_coding_challenge.Models;

public class Powerplant
{
    public string Name { get; set; } = string.Empty;
    public PowerplantType Type { get; set; }
    public decimal Efficiency { get; set; }
    public decimal Pmin { get; set; }
    public decimal Pmax { get; set; }

    public decimal CalculateProduction(decimal load, decimal windPercentage)
    {
        return Type switch
        {
            PowerplantType.windturbine => CalculateWindProduction(load, windPercentage),
            PowerplantType.gasfired or PowerplantType.turbojet => CalculateThermalProduction(load),
            _ => throw new NotImplementedException($"Production calculation for {Type} is not implemented.")
        };
    }

    private decimal CalculateWindProduction(decimal load, decimal windPercentage)
    {
        // Calculate potential production based on wind percentage
        decimal potentialProduction = Pmax * (windPercentage / 100.0m);

        // If potential production exceeds the load, turn off the wind turbine
        if (potentialProduction > load)
        {
            return 0m; // Disable the wind turbine because it would overproduce
        }

        // Otherwise, return the calculated production
        return Math.Min(potentialProduction, load);
    }

    private decimal CalculateThermalProduction(decimal load)
    {
        // If the load is less than Pmin, produce nothing
        if (load < Pmin) return 0m;

        // Produce as much as needed but no more than Pmax
        return Math.Min(Pmax, load);
    }

    public decimal CalculateCostPerMWh(Fuels fuels)
    {
        return Type switch
        {
            PowerplantType.gasfired => (fuels.Gas * (1 / Efficiency)) + (0.3m * fuels.Co2),
            PowerplantType.turbojet => fuels.Kerosine * (1 / Efficiency),
            PowerplantType.windturbine => 0m,
            _ => throw new NotImplementedException($"Cost calculation for {Type} is not implemented.")
        };
    }
}