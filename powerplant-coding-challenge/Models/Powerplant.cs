namespace powerplant_coding_challenge.Models;

public class Powerplant
{
    public string Name { get; set; } = string.Empty;
    public PowerplantTypeEnumeration Type { get; set; }
    public decimal Efficiency { get; set; }
    public decimal Pmin { get; set; }
    public decimal Pmax { get; set; }

    public decimal CalculateProduction(decimal load, decimal windPercentage)
    {
        return Type switch
        {
            PowerplantTypeEnumeration.WindTurbine => CalculateWindProduction(windPercentage),
            PowerplantTypeEnumeration.GasFired or PowerplantTypeEnumeration.TurboJet => CalculateThermalProduction(load),
            _ => throw new NotImplementedException($"Production calculation for {Type} is not implemented.")
        };
    }

    private decimal CalculateWindProduction(decimal windPercentage)
    {
        return Pmax * (windPercentage / 100m);
    }

    private decimal CalculateThermalProduction(decimal load)
    {
        return Math.Min(Pmax, load);
    }

    public decimal CalculateCostPerMWh(Fuels fuels)
    {
        return Type switch
        {
            PowerplantTypeEnumeration.GasFired => fuels.Gas * (1 / Efficiency),
            PowerplantTypeEnumeration.TurboJet => fuels.Kerosine * (1 / Efficiency),
            PowerplantTypeEnumeration.WindTurbine => 0m,
            _ => throw new NotImplementedException($"Cost calculation for {Type} is not implemented.")
        };
    }
}