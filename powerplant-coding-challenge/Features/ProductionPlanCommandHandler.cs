using MediatR;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using Serilog;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    public Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        if (command.Powerplants.Count == 0 || command.Load == 0)
        {
            return Task.FromResult(command.Powerplants.Select(p => new ProductionPlanCommandResponse(p.Name, 0m)).ToList());
        }

        // Validate the production plan to ensure it can meet the required load.
        ProductionPlanValidator.ValidateTotalCapacity(command.Powerplants, command.Load);
        ProductionPlanValidator.ValidateLoadAgainstPmin(command.Powerplants, command.Load);

        var response = new List<ProductionPlanCommandResponse>();
        decimal totalCost = 0m;

        // Handle wind turbines first since their production is dependent on wind percentage.
        foreach (var plant in command.Powerplants.Where(p => p.Type == PowerplantType.windturbine))
        {
            decimal production = plant.CalculateProduction(command.Load, command.Fuels.Wind);
            response.Add(new ProductionPlanCommandResponse(plant.Name, production));

            LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);
            LoggingHelper.LogProductionCost(production, 0); // No cost for wind energy.
        }

        // Handle the remaining powerplants, ordered by their production cost.
        var sortedPowerplants = command.Powerplants
            .Where(p => p.Type != PowerplantType.windturbine)
            .OrderBy(p => p.CalculateCostPerMWh(command.Fuels))
            .ToList();

        // Logging the order and cost per MWh of each powerplant
        foreach (var plant in sortedPowerplants)
        {
            decimal costPerMWh = plant.CalculateCostPerMWh(command.Fuels);
            Log.Information("Powerplant: {PlantName}, Cost per MWh: {CostPerMWh}, Type: {Type}", plant.Name, costPerMWh, plant.Type);
        }

        decimal remainingLoad = command.Load - response.Sum(r => r.Power);

        foreach (var plant in sortedPowerplants)
        {
            decimal production = plant.CalculateProduction(remainingLoad, command.Fuels.Wind);
            if (production > 0)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, production));
                remainingLoad -= production;
                totalCost += production * plant.CalculateCostPerMWh(command.Fuels);

                LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);
                LoggingHelper.LogProductionCost(production, totalCost);
            }
            else
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
                LoggingHelper.LogSkippedPowerplant(plant);
            }
        }

        // Adjust the final production to match the exact required load.
        ProductionManager.EnsureTotalProductionMatchesLoad(response, command.Load);

        // Log the final production costs and any discrepancies.
        LoggingHelper.LogDiscrepancy(remainingLoad);
        LoggingHelper.LogFinalSummary(command.Load, totalCost);

        return Task.FromResult(response);
    }
}