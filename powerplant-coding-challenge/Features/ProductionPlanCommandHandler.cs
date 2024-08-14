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

        decimal remainingLoad = request.Load;
        decimal totalCost = 0m;
        decimal totalProductionCalculated = 0m;

        Log.Information("Requested Load : {Load} MWh (Energy required for the next hour) \n", request.Load);

        // 1. Calcul de la production des éoliennes
        foreach (var windPlant in request.Powerplants.Where(powerplant => powerplant.Type == "windturbine"))
        {
            decimal production = _productionCalculator.CalculateProduction(windPlant, remainingLoad, request.Fuels.Wind);
            if (production > remainingLoad) production = remainingLoad;

            totalProductionCalculated += production;
            remainingLoad -= production;

            response.Add(new ProductionPlanCommandResponse(windPlant.Name, production.ToString("F1", CultureInfo.InvariantCulture)));

            Log.Information(
                " => Evaluating {PlantName} :\n" +
                "  - Type : {Type} \n" +
                "  - Pmin : {Pmin} MW \n" +
                "  - Pmax : {Pmax} MW \n" +
                "  - Efficiency : {Efficiency:F2} \n" +
                "  - Producing : {Production} MWh @ {Wind}% wind (0 EUR) \n",
                windPlant.Name, windPlant.Type, windPlant.Pmin, windPlant.Pmax, windPlant.Efficiency,
                production, request.Fuels.Wind
            );
        }

        // 2. Calcul de la production et du coût pour chaque centrale restante
        var powerplants = request.Powerplants
            .Where(powerplant => powerplant.Type != "windturbine")
            .OrderBy(powerplant => _costCalculator.CalculateCostPerMWh(powerplant, request.Fuels))
            .ToList();

        foreach (var powerplant in powerplants)
        {
            string logMessage = string.Format(
                " => Evaluating {0} :\n" +
                "  - Type : {1} \n" +
                "  - Pmin : {2} MW \n" +
                "  - Pmax : {3} MW \n" +
                "  - Efficiency : {4:F2}",
                powerplant.Name, powerplant.Type, powerplant.Pmin, powerplant.Pmax, powerplant.Efficiency
            );

            if (remainingLoad <= 0)
            {
                logMessage += "\n  -> Skipped : Remaining load is 0.0 MWh.";
                Log.Information(logMessage + "\n");
                response.Add(new ProductionPlanCommandResponse(powerplant.Name, "0.0"));
                continue;
            }

            decimal production = _productionCalculator.CalculateProduction(powerplant, remainingLoad, request.Fuels.Wind);
            if (production > remainingLoad) production = remainingLoad;

            decimal fuelCostPerMWh = request.Fuels.Gas / powerplant.Efficiency;
            decimal co2CostPerMWh = 0.3m * request.Fuels.Co2;
            decimal totalCostPerMWh = fuelCostPerMWh + co2CostPerMWh;
            decimal costBeforeRounding = totalCostPerMWh * production;

            decimal cost = Math.Round(costBeforeRounding, 2);
            totalCost += cost;

            remainingLoad -= production;
            totalProductionCalculated += production;

            logMessage += string.Format(
                "\n  - Producing : {0} MWh ({1:F2} EUR)\n" +
                "  - Cost Breakdown :\n" +
                "      Fuel Cost : {2:F2} EUR/MWh \n" +
                "      CO2 Cost : {3:F2} EUR/MWh \n" +
                "      Total Cost per MWh : {4:F2} EUR",
                production, cost, fuelCostPerMWh, co2CostPerMWh, totalCostPerMWh
            );

            Log.Information(logMessage + "\n");

            response.Add(new ProductionPlanCommandResponse(powerplant.Name, production.ToString("F1", CultureInfo.InvariantCulture)));
        }

        // Arrondir le coût total après avoir additionné tous les coûts
        totalCost = Math.Round(totalCost, 2);

        Log.Information(" -> Load Processed : {TotalLoad:F1} MWh | Total Cost : {TotalCost:F2} EUR/h", totalProductionCalculated, totalCost);

        decimal discrepancy = totalProductionCalculated - request.Load;
        if (discrepancy > 0.1m)
        {
            Log.Warning("Discrepancy : {Discrepancy:F1} MWh. Production exceeds the requested load.", discrepancy);
        }
        else if (discrepancy < -0.1m)
        {
            Log.Warning("Discrepancy : {Discrepancy:F1} MWh. Production is insufficient to meet the requested load.", discrepancy);
        }
        else
        {
            Log.Information("Discrepancy : {Discrepancy:F1} MWh. The production meets the requested load perfectly.", discrepancy);
        }

        return await Task.FromResult(response);
    }
}
