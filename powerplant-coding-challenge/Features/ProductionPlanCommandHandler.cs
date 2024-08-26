using FluentValidation;
using MediatR;
using powerplant_coding_challenge.Helpers;
using powerplant_coding_challenge.Services;
using Serilog;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandHandler(ProductionPlanService productionManager, IValidator<ProductionPlanCommand> validator) : IRequestHandler<ProductionPlanCommand, List<ProductionPlanCommandResponse>>
{
    private readonly ProductionPlanService _productionManager = productionManager;
    private readonly IValidator<ProductionPlanCommand> _validator = validator;

    public async Task<List<ProductionPlanCommandResponse>> Handle(ProductionPlanCommand command, CancellationToken cancellationToken)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            Log.Warning("Validation failed for production plan command: {Errors}", validationResult.Errors);
            throw new ValidationException(validationResult.Errors);
        }

        Log.Information("Processing production plan for load: {Load} MWh with {PowerplantsCount} powerplants.", command.Load, command.Powerplants.Count);

        // Generate Production Plan
        var response = _productionManager.GenerateProductionPlan(command);

        // Calculate and log total production and cost
        var totalProduction = response.Sum(r => r.Power);
        var totalCost = response.Sum(r => r.Power * command.Fuels.Gas);
        LoggingHelper.LogFinalSummary(totalProduction, totalCost);

        return response;
    }
}