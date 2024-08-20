using FluentValidation;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandValidator : AbstractValidator<ProductionPlanCommand>
{
    public ProductionPlanCommandValidator()
    {
        // Validation for Load
        RuleFor(command => command.Load)
            .GreaterThan(0)
            .WithMessage("Load must be greater than 0.");

        // Validation for Powerplants
        RuleForEach(command => command.Powerplants)
            .ChildRules(plant =>
            {
                plant.RuleFor(powerplant => powerplant.Name)
                    .NotEmpty()
                    .WithMessage("Powerplant name must not be empty.");

                plant.RuleFor(powerplant => powerplant.Type)
                    .IsInEnum()
                    .WithMessage("Powerplant type must be a valid value from PowerplantType enum.");

                plant.RuleFor(powerplant => powerplant.Efficiency)
                    .InclusiveBetween(0.01m, 1m)
                    .WithMessage("Efficiency should be between 0.01 and 1");

                plant.RuleFor(powerplant => powerplant.Pmin)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Pmin must be >= 0.");

                plant.RuleFor(powerplant => powerplant.Pmax)
                    .GreaterThan(0)
                    .WithMessage("Pmax must be > 0.");

                plant.RuleFor(powerplant => powerplant.Pmax)
                    .GreaterThan(powerplant => powerplant.Pmin)
                    .WithMessage("Pmax must be > Pmin.");

                plant.When(powerplant => powerplant.Type == PowerplantType.windturbine, () =>
                {
                    plant.RuleFor(powerplant => powerplant.Efficiency)
                        .Equal(1)
                        .WithMessage("Efficiency for windturbine must be 1.");
                });

                plant.When(powerplant => powerplant.Type == PowerplantType.gasfired || powerplant.Type == PowerplantType.turbojet, () =>
                {
                    plant.RuleFor(powerplant => powerplant.Efficiency)
                        .GreaterThan(0.01m)
                        .WithMessage("Efficiency for gasfired or turbojet must be greater than 0.01.");
                });
            });

        // Validation for Fuels
        RuleFor(command => command.Fuels)
            .NotNull()
            .ChildRules(fuels =>
            {
                fuels.RuleFor(fuel => fuel.Wind)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Wind(%) should be between 0 and 100");

                fuels.RuleFor(fuel => fuel.Kerosine)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Kerosine cost cannot be negative");

                fuels.RuleFor(fuel => fuel.Gas)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Gas cost cannot be negative");

                fuels.RuleFor(fuel => fuel.Co2)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("CO2 cost cannot be negative");
            });
    }
}