using FluentValidation.TestHelper;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests.Features;

public class ProductionPlanCommandValidatorTests
{
    private readonly ProductionPlanCommandValidator _validator = new();

    [Fact]
    public void Validator_Should_Have_Error_When_Load_Is_Negative()
    {
        var command = new ProductionPlanCommand { Load = -10m };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Load).WithErrorMessage("Load must be greater than 0.");
    }

    [Fact]
    public void Validator_Should_Pass_When_Data_Is_Valid()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants = [
                new Powerplant { Name = "Plant1", Type = "gasfired", Efficiency = 0.5m, Pmin = 50m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
