﻿using MediatR;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        var response = new List<ProductionPlanCommandResponse>();
        decimal remainingLoad = command.Load;
        decimal totalCost = 0m;

        // Vérification initiale si aucune centrale n'est fournie
        if (command.Powerplants.Count == 0)
        {
            return await Task.FromResult(response);
        }

        // Vérification initiale si la charge est zéro
        if (remainingLoad == 0)
        {
            foreach (var plant in command.Powerplants)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
            }
            return await Task.FromResult(response);
        }

        // Vérification si la charge demandée dépasse la capacité totale des centrales
        decimal totalCapacity = command.Powerplants.Sum(p => p.Pmax);
        if (remainingLoad > totalCapacity)
        {
            throw new InvalidOperationException("La charge demandée dépasse la capacité totale des centrales disponibles.");
        }

        // Vérification si la charge demandée est inférieure au Pmin total des centrales
        decimal totalMinCapacity = command.Powerplants.Sum(p => p.Pmin);
        if (remainingLoad < totalMinCapacity)
        {
            throw new InvalidOperationException("La charge demandée est inférieure au Pmin total des centrales disponibles.");
        }

        // 1. Traiter les éoliennes en premier
        foreach (var plant in command.Powerplants.Where(p => p.Type == PowerplantType.windturbine))
        {
            decimal production = plant.Pmax * (command.Fuels.Wind / 100);

            // Si la production éolienne dépasse la charge restante, ne pas produire
            if (production > remainingLoad)
            {
                production = 0;
            }
            else
            {
                remainingLoad -= production;
            }

            response.Add(new ProductionPlanCommandResponse(plant.Name, production));

            LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);
            LoggingHelper.LogProductionCost(production, 0);  // Pas de coût de carburant pour le vent
        }

        // 2. Trier les centrales restantes par coût marginal croissant
        var sortedPowerplants = command.Powerplants
            .Where(p => p.Type != PowerplantType.windturbine)
            .OrderBy(p => p.CalculateCostPerMWh(command.Fuels))
            .ToList();

        // 3. Allouer la production des centrales triées
        foreach (var plant in sortedPowerplants)
        {
            if (remainingLoad <= 0)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
                continue;
            }

            decimal production = Math.Min(plant.Pmax, remainingLoad);
            if (production >= plant.Pmin)
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, production));
                remainingLoad -= production;

                decimal cost = production * plant.CalculateCostPerMWh(command.Fuels);
                totalCost += cost;

                LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);
                LoggingHelper.LogProductionCost(production, cost);
            }
            else
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
                LoggingHelper.LogSkippedPowerplant(plant);
            }
        }

        // 4. Ajustement final pour couvrir exactement la charge
        if (remainingLoad > 0)
        {
            AdjustProductionToMatchLoad(response, sortedPowerplants, remainingLoad, command);
        }

        // 5. Ajouter les centrales non utilisées à la réponse avec production 0
        foreach (var plant in command.Powerplants)
        {
            if (!response.Any(r => r.Name == plant.Name))
            {
                response.Add(new ProductionPlanCommandResponse(plant.Name, 0m));
            }
        }

        // 6. Journaliser le résumé final et toute éventuelle divergence
        LoggingHelper.LogFinalSummary(command.Load, totalCost);

        return await Task.FromResult(response);
    }

    private static void AdjustProductionToMatchLoad(List<ProductionPlanCommandResponse> response, List<Powerplant> sortedPowerplants, decimal remainingLoad, ProductionPlanCommand command)
    {
        if (remainingLoad > 0)
        {
            foreach (var plant in sortedPowerplants)
            {
                var productionResponse = response.FirstOrDefault(r => r.Name == plant.Name);
                if (productionResponse != null)
                {
                    decimal currentProduction = productionResponse.Power;
                    decimal increase = Math.Min(remainingLoad, plant.Pmax - currentProduction);

                    if (increase > 0)
                    {
                        currentProduction += increase;
                        productionResponse.Power = currentProduction;
                        remainingLoad -= increase;

                        decimal cost = increase * plant.CalculateCostPerMWh(command.Fuels);
                        LoggingHelper.LogProductionCost(currentProduction, cost);
                    }

                    if (remainingLoad <= 0)
                        break;
                }
            }
        }
        else if (remainingLoad < 0)
        {
            foreach (var plant in sortedPowerplants.OrderByDescending(p => p.CalculateCostPerMWh(command.Fuels)))
            {
                var productionResponse = response.FirstOrDefault(r => r.Name == plant.Name);
                if (productionResponse != null)
                {
                    decimal currentProduction = productionResponse.Power;
                    decimal decrease = Math.Min(-remainingLoad, currentProduction - plant.Pmin);

                    if (decrease > 0)
                    {
                        currentProduction -= decrease;
                        productionResponse.Power = currentProduction;
                        remainingLoad += decrease;

                        decimal cost = decrease * plant.CalculateCostPerMWh(command.Fuels);
                        LoggingHelper.LogProductionCost(currentProduction, cost);
                    }

                    if (remainingLoad >= 0)
                        break;
                }
            }
        }

        if (remainingLoad != 0)
        {
            ForceFinalAdjustment(response, sortedPowerplants, remainingLoad, command.Fuels);
        }
    }

    private static void ForceFinalAdjustment(List<ProductionPlanCommandResponse> response, List<Powerplant> sortedPowerplants, decimal remainingLoad, Fuels fuels)
    {
        foreach (var plant in sortedPowerplants)
        {
            var productionResponse = response.First(r => r.Name == plant.Name);
            decimal currentProduction = productionResponse.Power;

            if (remainingLoad > 0 && currentProduction < plant.Pmax)
            {
                decimal adjustment = Math.Min(remainingLoad, plant.Pmax - currentProduction);
                productionResponse.Power = currentProduction + adjustment;
                remainingLoad -= adjustment;

                LoggingHelper.LogProductionCost(productionResponse.Power, adjustment * plant.CalculateCostPerMWh(fuels));
            }
            else if (remainingLoad < 0 && currentProduction > plant.Pmin)
            {
                decimal adjustment = Math.Min(-remainingLoad, currentProduction - plant.Pmin);
                productionResponse.Power = currentProduction - adjustment;
                remainingLoad += adjustment;

                LoggingHelper.LogProductionCost(productionResponse.Power, adjustment * plant.CalculateCostPerMWh(fuels));
            }

            if (remainingLoad == 0)
                break;
        }
    }
}
