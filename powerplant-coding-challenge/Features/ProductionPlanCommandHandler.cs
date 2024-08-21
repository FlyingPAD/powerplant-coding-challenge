using MediatR;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    public Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        if (command.Powerplants.Count == 0 || command.Load == 0)
        {
            return Task.FromResult(command.Powerplants.Select(p => new ProductionPlanCommandResponse(p.Name, 0m)).ToList());
        }

        decimal totalCapacity = command.Powerplants.Sum(p => p.Pmax);
        if (command.Load > totalCapacity)
        {
            throw new InvalidOperationException("La charge demandée dépasse la capacité totale des centrales disponibles.");
        }

        bool isLoadBelowAllPmin = command.Powerplants
            .Where(p => p.Type != PowerplantType.windturbine)
            .All(p => command.Load < p.Pmin);

        if (isLoadBelowAllPmin)
        {
            throw new InvalidOperationException("La charge demandée est inférieure au Pmin total des centrales disponibles.");
        }

        var response = new List<ProductionPlanCommandResponse>();
        decimal remainingLoad = command.Load;
        decimal totalCost = 0m;

        // Gestion des éoliennes
        foreach (var plant in command.Powerplants.Where(p => p.Type == PowerplantType.windturbine))
        {
            decimal production = plant.CalculateProduction(remainingLoad, command.Fuels.Wind);
            remainingLoad -= production;
            response.Add(new ProductionPlanCommandResponse(plant.Name, production));

            LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);
            LoggingHelper.LogProductionCost(production, 0); // Pas de coût pour l'éolien
        }

        // Gestion des autres centrales (triées par coût de production)
        var sortedPowerplants = command.Powerplants
            .Where(p => p.Type != PowerplantType.windturbine)
            .OrderBy(p => p.CalculateCostPerMWh(command.Fuels))
            .ToList();

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

        // Ajustement final
        AdjustProductionToMatchLoad(response, remainingLoad);

        LoggingHelper.LogFinalSummary(command.Load, totalCost);

        return Task.FromResult(response);
    }

    private static void AdjustProductionToMatchLoad(List<ProductionPlanCommandResponse> response, decimal remainingLoad)
    {
        if (remainingLoad == 0) return;

        foreach (var productionResponse in response.OrderByDescending(r => r.Power))
        {
            if (remainingLoad == 0) break;

            decimal adjustment = Math.Min(Math.Abs(remainingLoad), productionResponse.Power);

            if (remainingLoad > 0)
            {
                productionResponse.Power += adjustment;
                remainingLoad -= adjustment;
            }
            else
            {
                productionResponse.Power -= adjustment;
                remainingLoad += adjustment;
            }

            LoggingHelper.LogProductionCost(productionResponse.Power, 0); // Coût à ajuster si nécessaire
        }

        LoggingHelper.LogDiscrepancy(remainingLoad);
    }
}
