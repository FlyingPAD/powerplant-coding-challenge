using FluentValidation.TestHelper;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests;

public class ProductionPlanCommandValidatorTests
{
    private readonly ProductionPlanCommandValidator _validator = new();

    // Test to ensure validation error occurs when Load is negative
    [Fact]
    public void Validator_Should_Have_Error_When_Load_Is_Negative()
    {
        var command = new ProductionPlanCommand { Load = -10m };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Load).WithErrorMessage("Load must be greater than 0.");
    }

    // Test to ensure validation passes with valid data
    [Fact]
    public void Validator_Should_Pass_When_Data_Is_Valid()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 50m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Test to ensure validation error occurs when a powerplant has invalid efficiency
    [Fact]
    public void Validator_Should_Have_Error_When_Efficiency_Is_Out_Of_Range()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 1.5m, Pmin = 50m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Powerplants[0].Efficiency")
            .WithErrorMessage("Efficiency should be between 0.01 and 1");
    }

    // Test to ensure validation error occurs when Pmax is less than Pmin
    [Fact]
    public void Validator_Should_Have_Error_When_Pmax_Is_Less_Than_Pmin()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 200m, Pmax = 100m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Powerplants[0].Pmax")
            .WithErrorMessage("Pmax must be > Pmin.");
    }

    // Test to ensure validation error occurs when fuel values are negative
    [Fact]
    public void Validator_Should_Have_Error_When_Fuel_Values_Are_Negative()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 50m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = -13.4m, Kerosine = -50.8m, Co2 = -20m, Wind = 60m }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Fuels.Gas")
            .WithErrorMessage("Gas cost cannot be negative");
        result.ShouldHaveValidationErrorFor("Fuels.Kerosine")
            .WithErrorMessage("Kerosine cost cannot be negative");
        result.ShouldHaveValidationErrorFor("Fuels.Co2")
            .WithErrorMessage("CO2 cost cannot be negative");
    }
}
