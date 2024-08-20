using MediatR;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        var response = new List<ProductionPlanCommandResponse>();
        decimal remainingLoad = command.Load;

        // 1. Sort the powerplants by marginal cost in ascending order
        var sortedPowerplants = command.Powerplants
            .OrderBy(p => p.CalculateCostPerMWh(command.Fuels))
            .ToList();

        // 2. Initial allocation based on marginal cost
        foreach (var plant in sortedPowerplants)
        {
            // Calculate the production for the current powerplant
            decimal production = plant.CalculateProduction(remainingLoad, command.Fuels.Wind);

            // Add the calculated production to the response
            response.Add(new ProductionPlanCommandResponse(plant.Name, production.ToString("F1")));

            // Subtract the production from the remaining load
            remainingLoad -= production;

            // Break the loop if the remaining load is already covered
            if (remainingLoad <= 0)
                break;
        }

        // 3. Final adjustments to ensure the load is exactly matched
        if (remainingLoad != 0)
        {
            AdjustProductionToMatchLoad(response, sortedPowerplants, remainingLoad, command);
        }

        return await Task.FromResult(response);
    }

    // Add `ProductionPlanCommand command` as a parameter
    private static void AdjustProductionToMatchLoad(List<ProductionPlanCommandResponse> response, List<Powerplant> sortedPowerplants, decimal remainingLoad, ProductionPlanCommand command)
    {
        if (remainingLoad > 0)
        {
            // Increase the production of the most flexible powerplants to meet the load
            foreach (var plant in sortedPowerplants)
            {
                var productionResponse = response.First(r => r.Name == plant.Name);
                decimal currentProduction = decimal.Parse(productionResponse.Power);
                decimal increase = Math.Min(remainingLoad, plant.Pmax - currentProduction);

                if (increase > 0)
                {
                    currentProduction += increase;
                    productionResponse.Power = currentProduction.ToString("F1");
                    remainingLoad -= increase;
                }

                if (remainingLoad <= 0)
                    break;
            }
        }
        else if (remainingLoad < 0)
        {
            // Reduce the production of the most expensive powerplants to match the exact load
            foreach (var plant in sortedPowerplants.OrderByDescending(p => p.CalculateCostPerMWh(command.Fuels)))
            {
                var productionResponse = response.First(r => r.Name == plant.Name);
                decimal currentProduction = decimal.Parse(productionResponse.Power);
                decimal decrease = Math.Min(-remainingLoad, currentProduction - plant.Pmin);

                if (decrease > 0)
                {
                    currentProduction -= decrease;
                    productionResponse.Power = currentProduction.ToString("F1");
                    remainingLoad += decrease;
                }

                if (remainingLoad >= 0)
                    break;
            }
        }

        // 4. If there's still a difference, force adjustment
        if (remainingLoad != 0)
        {
            ForceAdjustment(response, sortedPowerplants, remainingLoad);
        }
    }

    private static void ForceAdjustment(List<ProductionPlanCommandResponse> response, List<Powerplant> sortedPowerplants, decimal remainingLoad)
    {
        foreach (var plant in sortedPowerplants)
        {
            var productionResponse = response.First(r => r.Name == plant.Name);
            decimal currentProduction = decimal.Parse(productionResponse.Power);

            if (remainingLoad > 0 && currentProduction < plant.Pmax)
            {
                productionResponse.Power = (currentProduction + remainingLoad).ToString("F1");
                remainingLoad = 0;
            }
            else if (remainingLoad < 0 && currentProduction > plant.Pmin)
            {
                productionResponse.Power = (currentProduction + remainingLoad).ToString("F1");
                remainingLoad = 0;
            }

            if (remainingLoad == 0)
                break;
        }
    }
}
