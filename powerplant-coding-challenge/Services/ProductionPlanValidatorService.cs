using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class ProductionPlanValidatorService
{
    public void ValidateTotalCapacity(List<Powerplant> powerplants, decimal load)
    {
        var totalCapacity = powerplants.Sum(powerplant => powerplant.Pmax);

        if (load > totalCapacity)
        {
            throw new InvalidOperationException("The requested load exceeds the total capacity of the available power plants.");
        }
    }

    public void ValidateLoadAgainstPmin(List<Powerplant> powerplants, decimal load)
    {
        var isLoadBelowAllPmin = powerplants
            .Where(p => p.Type != PowerplantTypeEnumeration.WindTurbine)
            .All(p => load < p.Pmin);

        if (isLoadBelowAllPmin)
        {
            throw new InvalidOperationException("The requested load is below the minimum production capacity (Pmin) of all non-wind power plants.");
        }
    }
}