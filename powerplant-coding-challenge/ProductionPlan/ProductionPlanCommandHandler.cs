using MediatR;
using System.Globalization;

namespace powerplant_coding_challenge.ProductionPlan
{
    public class ProductionPlanCommandHandler : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
    {
        public Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
        {
            var request = command;
            var response = new List<ProductionPlanCommandResponse>();

            // 1. Calculate the production of wind turbines
            foreach (var windPlant in request.Powerplants.Where(powerplant => powerplant.Type == "windturbine"))
            {
                double production = windPlant.CalculateProduction(request.Load, request.Fuels.Wind);
                response.Add(new ProductionPlanCommandResponse(
                    windPlant.Name,
                    Math.Round(production, 1).ToString("F1", CultureInfo.InvariantCulture) // Convert to string with one decimal
                ));
                request.Load -= production;
            }

            // 2. Calculate the production cost for each remaining power plant
            var powerplants = request.Powerplants
                .Where(powerplant => powerplant.Type != "windturbine")
                .OrderBy(powerplant => powerplant.CalculateCostPerMWh(request.Fuels))
                .ToList();

            // 3. Allocate the remaining load
            double remainingLoad = request.Load;
            foreach (var powerplant in powerplants)
            {
                double production = powerplant.CalculateProduction(remainingLoad, request.Fuels.Wind);
                remainingLoad -= production;

                response.Add(new ProductionPlanCommandResponse(
                    powerplant.Name,
                    Math.Round(production, 1).ToString("F1", CultureInfo.InvariantCulture) // Convert to string with one decimal
                ));
            }

            return Task.FromResult(response);
        }
    }
}