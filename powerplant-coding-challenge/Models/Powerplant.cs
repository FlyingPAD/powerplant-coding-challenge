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
        if (windPercentage == 0)
        {
            return 0m;
        }

        decimal potentialProduction = Pmax * (windPercentage / 100.0m);

        if (potentialProduction > load)
        {
            return 0m;
        }

        return Math.Min(potentialProduction, load);
    }

    private decimal CalculateThermalProduction(decimal load)
    {
        if (load < Pmin) return 0m;
        return Math.Min(Pmax, load);
    }

    public decimal CalculateCostPerMWh(Fuels fuels)
    {
        return Type switch
        {
            PowerplantType.gasfired => fuels.Gas * (1 / Efficiency),
            PowerplantType.turbojet => fuels.Kerosine * (1 / Efficiency),
            PowerplantType.windturbine => 0m,
            _ => throw new NotImplementedException($"Cost calculation for {Type} is not implemented.")
        };
    }
}