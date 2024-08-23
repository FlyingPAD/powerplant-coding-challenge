using MediatR;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Services;
using Serilog;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler(ProductionManager productionManager) : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    private readonly ProductionManager _productionManager = productionManager;

    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        Log.Information("Processing production plan for load: {Load} MWh with {PowerplantsCount} powerplants.", command.Load, command.Powerplants.Count);

        var response = _productionManager.GenerateProductionPlan(command);

        foreach (var plantResponse in response)
        {
            Log.Information("Powerplant {Name} will produce {Power} MWh.", plantResponse.Name, plantResponse.Power);
        }

        var totalProduction = response.Sum(r => r.Power);
        LoggingHelper.LogFinalSummary(totalProduction, response.Sum(r => r.Power * command.Fuels.Gas)); // Vous pouvez ajuster le coût ici selon le besoin

        await Task.CompletedTask;

        return response;
    }
}