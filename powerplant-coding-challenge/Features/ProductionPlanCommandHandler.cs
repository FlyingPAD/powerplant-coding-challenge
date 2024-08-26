using MediatR;
using powerplant_coding_challenge.Services;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler(ProductionPlanService productionManager) : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    private readonly ProductionPlanService _productionManager = productionManager;

    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        return _productionManager.GenerateProductionPlan(command);
    }
}