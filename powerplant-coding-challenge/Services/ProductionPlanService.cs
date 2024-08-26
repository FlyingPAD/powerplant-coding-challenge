using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services;

public class ProductionPlanService(ProductionPlanValidatorService validator)
{
    private readonly ProductionPlanValidatorService _validator = validator;

    public List<ProductionPlanCommandResponse> GenerateProductionPlan(ProductionPlanCommand command)
    {
        // Validation.
        if (command.Powerplants.Count == 0 || command.Load == 0)
        {
            return command.Powerplants.Select(powerplant => new ProductionPlanCommandResponse(powerplant.Name, 0m)).ToList();
        }
        _validator.ValidateTotalCapacity(command.Powerplants, command.Load);
        _validator.ValidateLoadAgainstPmin(command.Powerplants, command.Load);

        // Allocation.
        var response = AllocateProduction(command);

        return response;
    }

    private static List<ProductionPlanCommandResponse> AllocateProduction(ProductionPlanCommand command)
    {
        // Sort powerplants by cost.
        var sortedByCostPowerplants = command.Powerplants
            .OrderBy(powerplant => powerplant.CalculateCostPerMWh(command.Fuels))
            .ToList();
        LoggingHelper.LogSortedPowerplantsByCost(sortedByCostPowerplants, command.Fuels);

        // Generate best scenario.
        var bestScenario = GenerateBestScenario(sortedByCostPowerplants, command);

        return bestScenario;
    }

    public static List<ProductionPlanCommandResponse> GenerateBestScenario(List<Powerplant> sortedPowerplants, ProductionPlanCommand command)
    {
        var scenarios = new List<List<Powerplant>>
        {
            sortedPowerplants
        };

        for (int plantIndex = 0; plantIndex < sortedPowerplants.Count; plantIndex++)
        {
            var scenario = sortedPowerplants.Where((_, currentIndex) => currentIndex != plantIndex).ToList();
            scenarios.Add(scenario);
        }

        List<ProductionPlanCommandResponse> bestScenario = [];
        var lowestCost = decimal.MaxValue;

        foreach (var scenario in scenarios)
        {
            var remainingLoad = command.Load;
            var response = new List<ProductionPlanCommandResponse>();

            AllocateWindPower(scenario, ref remainingLoad, response, command.Fuels.Wind);
            AllocateThermalPower(scenario, ref remainingLoad, response);

            var totalCost = CalculateScenarioCost(response, scenario, command.Fuels);

            if (remainingLoad == 0 && totalCost < lowestCost)
            {
                lowestCost = totalCost;
                bestScenario = response;
            }
        }
        bestScenario.AddRange(sortedPowerplants.Where(powerplant => !bestScenario.Select(powerplant => powerplant.Name).Contains(powerplant.Name)).Select(powerplant => new ProductionPlanCommandResponse(powerplant.Name,0.0m)));
        return bestScenario;
    }

    private static decimal CalculateScenarioCost(List<ProductionPlanCommandResponse> response, List<Powerplant> scenario, Fuels fuels)
    {
        decimal totalCost = 0;

        foreach (var allocation in response)
        {
            var powerplant = scenario.First(powerplant => powerplant.Name == allocation.Name);
            totalCost += allocation.Power * powerplant.CalculateCostPerMWh(fuels);
        }

        return totalCost;
    }

    private static void AllocateWindPower(List<Powerplant> powerplants, ref decimal remainingLoad, List<ProductionPlanCommandResponse> response, decimal windPercentage)
    {
        var windPlants = powerplants.Where(powerplant => powerplant.Type == PowerplantTypeEnumeration.WindTurbine).ToList();
        var thermalPlants = powerplants.Where(powerplant => powerplant.Type != PowerplantTypeEnumeration.WindTurbine).ToList();

        foreach (var plant in windPlants)
        {
            var windProduction = plant.CalculateProduction(remainingLoad, windPercentage);
            var potentialRemainingLoad = remainingLoad - windProduction;

            var isWindBeneficial = potentialRemainingLoad == 0 || SimulateThermalAllocation(thermalPlants, potentialRemainingLoad);

            if (isWindBeneficial && windProduction <= remainingLoad)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, windProduction));
                remainingLoad -= windProduction;
            }
            else
            {
                LoggingHelper.LogSkippedWindPlant(plant, remainingLoad, windProduction, isWindBeneficial);
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
            }
        }
    }

    private static bool SimulateThermalAllocation(List<Powerplant> thermalPlants, decimal remainingLoad)
    {
        if (thermalPlants.All(powerplant => remainingLoad < powerplant.Pmin))
        {
            return false;
        }

        foreach (var plant in thermalPlants)
        {
            var production = Math.Min(plant.Pmax, remainingLoad);
            remainingLoad -= production;
            LoggingHelper.LogThermalAllocation(plant, production, remainingLoad);
        }

        return remainingLoad <= 0;
    }

    private static void AllocateThermalPower(List<Powerplant> powerplants, ref decimal remainingLoad, List<ProductionPlanCommandResponse> response)
    {
        var thermalPlants = powerplants.Where(powerplant => powerplant.Type != PowerplantTypeEnumeration.WindTurbine).ToList();

        foreach (var currentPlant in thermalPlants)
        {
            if (remainingLoad < currentPlant.Pmin)
            {
                response.Add(new ProductionPlanCommandResponse(currentPlant.Name, 0m));
                continue;
            }

            var production = currentPlant.CalculateProduction(remainingLoad, 0);

            if (production > remainingLoad)
            {
                production = remainingLoad;
            }

            AdjustProductionForNextPlant(thermalPlants, currentPlant, ref production, remainingLoad);

            LoggingHelper.LogPowerplantEvaluation(currentPlant, production, 0);

            response.Add(new ProductionPlanCommandResponse(currentPlant.Name, production));
            remainingLoad -= production;
        }
    }

    private static void AdjustProductionForNextPlant(List<Powerplant> thermalPlants, Powerplant currentPlant, ref decimal production, decimal remainingLoad)
    {
        var nextPlantIndex = thermalPlants.IndexOf(currentPlant) + 1;

        if (nextPlantIndex >= thermalPlants.Count || remainingLoad <= production) return;

        var nextPlant = thermalPlants[nextPlantIndex];

        if (remainingLoad - production < nextPlant.Pmin)
        {
            production = remainingLoad - nextPlant.Pmin;

            if (production < currentPlant.Pmin)
            {
                production = currentPlant.Pmin;
            }
        }
    }
}