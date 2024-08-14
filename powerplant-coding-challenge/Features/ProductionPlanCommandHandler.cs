using MediatR;
using powerplant_coding_challenge.Interfaces;
using Serilog;
using System.Globalization;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler(ICostCalculator costCalculator, IProductionCalculator productionCalculator) : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    private readonly ICostCalculator _costCalculator = costCalculator;
    private readonly IProductionCalculator _productionCalculator = productionCalculator;

    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        var request = command;
        var response = new List<ProductionPlanCommandResponse>();

        double remainingLoad = request.Load;
        double totalCost = 0;
        double totalProductionCalculated = 0;

        // Log initial request and load
        Log.Information("Requested Load: {Load} MW", request.Load);
        Log.Information("Production Breakdown:");

        // 1. Calculate the production of wind turbines
        foreach (var windPlant in request.Powerplants.Where(powerplant => powerplant.Type == "windturbine"))
        {
            double production = _productionCalculator.CalculateProduction(windPlant, remainingLoad, request.Fuels.Wind);
            if (production > remainingLoad) production = remainingLoad;

            double cost = _costCalculator.CalculateCostPerMWh(windPlant, request.Fuels) * production;
            totalCost += cost;
            remainingLoad -= production;

            // Accumulate the total production
            totalProductionCalculated += production;

            response.Add(new ProductionPlanCommandResponse(windPlant.Name, production.ToString("F1", CultureInfo.InvariantCulture)));

            Log.Information("- {PlantName}: {Production} MW @ {Wind}% wind (Cost: {Cost:F2} EUR)",
                windPlant.Name, production, request.Fuels.Wind, cost);
        }

        // 2. Calculate the production cost for each remaining power plant
        var powerplants = request.Powerplants
            .Where(powerplant => powerplant.Type != "windturbine")
            .OrderBy(powerplant => _costCalculator.CalculateCostPerMWh(powerplant, request.Fuels))
            .ToList();

        // 3. Allocate the remaining load
        foreach (var powerplant in powerplants)
        {
            if (remainingLoad <= 0)
            {
                response.Add(new ProductionPlanCommandResponse(powerplant.Name, "0.0"));
                Log.Information("- {PlantName}: 0 MW (Cost: 0 EUR)", powerplant.Name);
                continue;
            }

            double production = _productionCalculator.CalculateProduction(powerplant, remainingLoad, request.Fuels.Wind);
            if (production > remainingLoad) production = remainingLoad;

            double cost = _costCalculator.CalculateCostPerMWh(powerplant, request.Fuels) * production;
            totalCost += cost;
            remainingLoad -= production;

            // Accumulate the total production
            totalProductionCalculated += production;

            response.Add(new ProductionPlanCommandResponse(powerplant.Name, production.ToString("F1", CultureInfo.InvariantCulture)));

            Log.Information("- {PlantName}: {Production} MW (Cost: {Cost:F2} EUR)", powerplant.Name, production, cost);
        }

        // Log the total load processed and the total cost
        Log.Information("Load Processed: {TotalLoad:F1} MW\nTotal Cost: {TotalCost:F2} EUR",
            request.Load - remainingLoad, totalCost);

        // Log the total production calculated
        Log.Information("Total Production Calculated: {TotalProductionCalculated:F1} MW", totalProductionCalculated);

        // Check for discrepancies
        if (Math.Abs(totalProductionCalculated - request.Load) > 0.1)
        {
            double discrepancy = totalProductionCalculated - request.Load;
            Log.Warning("Discrepancy: {Discrepancy:F1} MW. Check the production allocation algorithm.", discrepancy);
        }

        return await Task.FromResult(response);
    }
}
