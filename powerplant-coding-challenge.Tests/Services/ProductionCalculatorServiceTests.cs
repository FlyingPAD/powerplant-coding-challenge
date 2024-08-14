using FluentAssertions;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using Xunit;

namespace powerplant_coding_challenge.Tests.Services;

public class ProductionCalculatorServiceTests
{
    private readonly ProductionCalculatorService _productionCalculator = new();

    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_For_WindTurbine()
    {
        var powerplant = new Powerplant { Type = "windturbine", Pmax = 150 };
        var production = _productionCalculator.CalculateProduction(powerplant, 100, 60);

        production.Should().Be(150 * 0.6);
    }

    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_For_Other_Plants()
    {
        var powerplant = new Powerplant { Type = "gasfired", Pmin = 100, Pmax = 200 };
        var production = _productionCalculator.CalculateProduction(powerplant, 150, 0);

        production.Should().Be(150);
    }
}
