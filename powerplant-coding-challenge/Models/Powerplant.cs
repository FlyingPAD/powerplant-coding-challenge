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
        decimal production = Pmax * (windPercentage / 100.0m);

        // Si la production dépasse la charge restante, on ajuste à la charge.
        return Math.Min(production, load);
    }

    private decimal CalculateThermalProduction(decimal load)
    {
        // Si la charge restante est inférieure à Pmin, on ne produit rien.
        if (load < Pmin) return 0m;

        // Produire autant que possible, mais ne pas dépasser Pmax ni la charge restante.
        return Math.Min(Pmax, load);
    }

    public decimal CalculateCostPerMWh(Fuels fuels)
    {
        return Type switch
        {
            PowerplantType.gasfired => (fuels.Gas / Efficiency) + (0.3m * fuels.Co2),
            PowerplantType.turbojet => fuels.Kerosine / Efficiency,
            PowerplantType.windturbine => 0m,
            _ => throw new NotImplementedException($"Cost calculation for {Type} is not implemented.")
        };
    }
}