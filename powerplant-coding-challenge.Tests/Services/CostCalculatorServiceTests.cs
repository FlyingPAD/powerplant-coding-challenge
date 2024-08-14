using FluentAssertions;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using Xunit;

namespace powerplant_coding_challenge.Tests.Services;

public class CostCalculatorServiceTests
{
    private readonly CostCalculatorService _costCalculator = new();

    [Fact]
    public void CalculateCostPerMWh_Should_Return_Correct_Cost_For_Gasfired()
    {
        // Arrange
        var powerplant = new Powerplant { Type = "gasfired", Efficiency = 0.5 };
        var fuels = new Fuels { Gas = 13.4, Co2 = 20 };

        // Act
        var cost = _costCalculator.CalculateCostPerMWh(powerplant, fuels);

        // Assert
        cost.Should().Be(32.8);
    }

    [Fact]
    public void CalculateCostPerMWh_Should_Return_Zero_For_WindTurbine()
    {
        var powerplant = new Powerplant { Type = "windturbine", Efficiency = 1.0 };
        var fuels = new Fuels();

        var cost = _costCalculator.CalculateCostPerMWh(powerplant, fuels);

        cost.Should().Be(0);
    }
}