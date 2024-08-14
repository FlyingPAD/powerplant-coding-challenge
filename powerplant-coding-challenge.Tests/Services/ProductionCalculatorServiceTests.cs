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
        var powerplant = new Powerplant { Type = "windturbine", Pmax = 150m };
        var production = _productionCalculator.CalculateProduction(powerplant, 100m, 60m);

        production.Should().Be(150m * 0.6m);
    }

    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_For_Other_Plants()
    {
        var powerplant = new Powerplant { Type = "gasfired", Pmin = 100m, Pmax = 200m };
        var production = _productionCalculator.CalculateProduction(powerplant, 150m, 0m);

        production.Should().Be(150m);
    }
}
