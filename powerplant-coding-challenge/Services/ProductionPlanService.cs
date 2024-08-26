using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class ProductionPlanService(ProductionPlanValidatorService validator)
{
    private readonly ProductionPlanValidatorService _validator = validator;

    public List<ProductionPlanCommandResponse> GenerateProductionPlan(ProductionPlanCommand command)
    {
        // Validation: Check if the command has no powerplants or if the load is zero.
        if (command.Powerplants.Count == 0 || command.Load == 0)
        {
            return command.Powerplants.Select(p => new ProductionPlanCommandResponse(p.Name, 0m)).ToList();
        }

        _validator.ValidateTotalCapacity(command.Powerplants, command.Load);
        _validator.ValidateLoadAgainstPmin(command.Powerplants, command.Load);

        var response = AllocateProduction(command);

        return response;
    }

    private static List<ProductionPlanCommandResponse> AllocateProduction(ProductionPlanCommand command)
    {
        decimal remainingLoad = command.Load;
        var response = new List<ProductionPlanCommandResponse>();

        // Sort powerplants by cost per MWh in ascending order.
        var sortedByCostPowerplants = command.Powerplants
            .OrderBy(powerplant => powerplant.CalculateCostPerMWh(command.Fuels))
            .ToList();
        LoggingHelper.LogSortedPowerplantsByCost(sortedByCostPowerplants, command.Fuels);

        // Allocation.
        AllocateWindPower(sortedByCostPowerplants, ref remainingLoad, response, command.Fuels.Wind);
        AllocateThermalPower(sortedByCostPowerplants, ref remainingLoad, response);

        // Final check.
        if (remainingLoad != 0)
        {
            LoggingHelper.LogRemainingLoadError(remainingLoad);
            throw new InvalidOperationException($"The remaining load is not zero after the calculation: {remainingLoad} MWh.");
        }

        return response;
    }

    private static void AllocateWindPower(List<Powerplant> powerplants, ref decimal remainingLoad, List<ProductionPlanCommandResponse> response, decimal windPercentage)
    {
        var windPlants = powerplants.Where(powerplant => powerplant.Type == PowerplantTypeEnumeration.windturbine).ToList();
        var thermalPlants = powerplants.Where(powerplant => powerplant.Type != PowerplantTypeEnumeration.windturbine).ToList();

        foreach (var plant in windPlants)
        {
            // Calculate potential production from the wind plant
            decimal windProduction = plant.CalculateProduction(remainingLoad, windPercentage);
            decimal potentialRemainingLoad = remainingLoad - windProduction;

            // Simulate allocation of thermal plants with the potential remaining load
            bool isWindBeneficial = SimulateThermalAllocation(thermalPlants, potentialRemainingLoad);

            if (isWindBeneficial && windProduction <= remainingLoad)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, windProduction));
                remainingLoad -= windProduction;
            }
            else
            {
                // Log the decision not to use this wind plant
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
            }
        }
    }

    private static bool SimulateThermalAllocation(List<Powerplant> thermalPlants, decimal remainingLoad)
    {
        foreach (var plant in thermalPlants)
        {
            if (remainingLoad <= 0)
            {
                return true;
            }

            if (remainingLoad < plant.Pmin)
            {
                // If the remaining load is too low to efficiently use this plant, wind power is not beneficial
                return false;
            }

            decimal production = Math.Min(plant.Pmax, remainingLoad);
            remainingLoad -= production;
        }

        return remainingLoad <= 0;
    }

    private static void AllocateThermalPower(List<Powerplant> powerplants, ref decimal remainingLoad, List<ProductionPlanCommandResponse> response)
    {
        var thermalPlants = powerplants.Where(powerplant => powerplant.Type != PowerplantTypeEnumeration.windturbine).ToList();

        // Iterate through the thermal powerplants to allocate production.
        for (int i = 0; i < thermalPlants.Count; i++)
        {
            var currentPlant = thermalPlants[i];

            // If the remaining load is less than the plant's minimum production (Pmin), skip this plant.
            if (remainingLoad < currentPlant.Pmin)
            {
                response.Add(new ProductionPlanCommandResponse(currentPlant.Name, 0m));
                continue;
            }

            // Calculate the production based on the remaining load and plant's Pmin/Pmax constraints.
            decimal production = currentPlant.CalculateProduction(remainingLoad, 0);

            // If the production exceeds the remaining load, adjust the production to match the remaining load.
            if (production > remainingLoad)
            {
                production = remainingLoad;
            }

            // Adjust production considering the next plant's minimum production constraint (Pmin).
            if (i < thermalPlants.Count - 1 && remainingLoad > production)
            {
                var nextPlant = thermalPlants[i + 1];

                if (remainingLoad - production < nextPlant.Pmin)
                {
                    production = remainingLoad - nextPlant.Pmin;
                    if (production < currentPlant.Pmin)
                    {
                        production = currentPlant.Pmin;
                    }
                }
            }

            LoggingHelper.LogPowerplantEvaluation(currentPlant, production, 0);

            response.Add(new ProductionPlanCommandResponse(currentPlant.Name, production));

            remainingLoad -= production;
        }
    }
}