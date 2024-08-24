using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Helpers;

namespace powerplant_coding_challenge.Services;

public class ProductionPlanService(ProductionPlanValidatorService validator)
{
    private readonly ProductionPlanValidatorService _validator = validator;

    public List<ProductionPlanCommandResponse> GenerateProductionPlan(ProductionPlanCommand command)
    {
        // Validation.
        if (command.Powerplants.Count == 0 || command.Load == 0)
        {
            return command.Powerplants.Select(p => new ProductionPlanCommandResponse(p.Name, 0m)).ToList();
        }
        _validator.ValidateTotalCapacity(command.Powerplants, command.Load);
        _validator.ValidateLoadAgainstPmin(command.Powerplants, command.Load);

        // Allocation.
        var response = AllocateProduction(command);

        return response;
    }

    private static List<ProductionPlanCommandResponse> AllocateProduction(ProductionPlanCommand command)
    {
        var response = new List<ProductionPlanCommandResponse>();

        // Tri des centrales
        var sortedPowerplants = command.Powerplants
            .OrderBy(powerplant => powerplant.CalculateCostPerMWh(command.Fuels))
            .ToList();

        decimal remainingLoad = command.Load;

        foreach (var plant in sortedPowerplants)
        {
            // Calcul de la production de chaque centrale.
            decimal production = plant.CalculateProduction(remainingLoad, command.Fuels.Wind);

            // Logging de l'évaluation de la centrale.
            LoggingHelper.LogPowerplantEvaluation(plant, production, command.Fuels.Wind);

            // Ajout de la production de la centrale à la réponse.
            response.Add(new ProductionPlanCommandResponse(plant.Name, production));

            // Mise à jour de la charge restante après allocation de la production de la centrale actuelle.
            remainingLoad -= production;
        }

        // Final Check.
        if (remainingLoad != 0)
        {
            LoggingHelper.LogRemainingLoadError(remainingLoad);
            throw new InvalidOperationException($"La charge restante n'est pas zéro après le calcul: {remainingLoad} MWh.");
        }

        return response;
    }
}