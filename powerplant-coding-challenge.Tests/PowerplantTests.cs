using FluentAssertions;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests.Models;

public class PowerplantTests
{
    // Test to check production calculation for wind turbines
    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_For_WindTurbine()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.windturbine, Pmax = 150m };

        // Act
        var production = powerplant.CalculateProduction(100m, 60m);

        // Assert
        production.Should().Be(90m); // 150 * 0.6
    }

    // Test to check production calculation for gas-fired power plants when load is between Pmin and Pmax
    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_For_GasFired_When_Load_Between_Pmin_And_Pmax()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Pmin = 100m, Pmax = 200m };

        // Act
        var production = powerplant.CalculateProduction(150m, 0m);

        // Assert
        production.Should().Be(150m);
    }

    // Test to check production returns zero when load is less than Pmin
    [Fact]
    public void CalculateProduction_Should_Return_Zero_When_Load_Is_Less_Than_Pmin()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Pmin = 100m, Pmax = 200m };

        // Act
        var production = powerplant.CalculateProduction(50m, 0m);

        // Assert
        production.Should().Be(0m); // Should produce 0 because the load is less than Pmin
    }

    // Test to check production returns Pmax when load exceeds Pmax
    [Fact]
    public void CalculateProduction_Should_Return_Pmax_When_Load_Exceeds_Pmax()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Pmin = 100m, Pmax = 200m };

        // Act
        var production = powerplant.CalculateProduction(250m, 0m);

        // Assert
        production.Should().Be(200m); // Should produce Pmax
    }

    // Test to check cost calculation for gas-fired power plants
    [Fact]
    public void CalculateCostPerMWh_Should_Return_Correct_Cost_For_Gasfired()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Efficiency = 0.5m };
        var fuels = new Fuels { Gas = 13.4m, Co2 = 20m };

        // Act
        var cost = powerplant.CalculateCostPerMWh(fuels);

        // Assert
        cost.Should().Be(32.8m); // (13.4 / 0.5) + (0.3 * 20)
    }

    // Test to check cost calculation for turbojet power plants
    [Fact]
    public void CalculateCostPerMWh_Should_Return_Correct_Cost_For_Turbojet()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.turbojet, Efficiency = 0.3m };
        var fuels = new Fuels { Kerosine = 50.8m };

        // Act
        var cost = powerplant.CalculateCostPerMWh(fuels);

        // Assert
        cost.Should().BeApproximately(169.33m, 0.01m); // (50.8 / 0.3)
    }

    // Test to check cost calculation returns zero for wind turbines
    [Fact]
    public void CalculateCostPerMWh_Should_Return_Zero_For_WindTurbine()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.windturbine, Efficiency = 1.0m };
        var fuels = new Fuels();

        // Act
        var cost = powerplant.CalculateCostPerMWh(fuels);

        // Assert
        cost.Should().Be(0m);
    }

    // Additional test to check production when Pmax equals Pmin
    [Fact]
    public void CalculateProduction_Should_Return_Pmax_When_Pmax_Equals_Pmin()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Pmin = 150m, Pmax = 150m };

        // Act
        var production = powerplant.CalculateProduction(150m, 0m);

        // Assert
        production.Should().Be(150m); // Pmin == Pmax == production
    }

    // Additional test to check production for wind turbine with 0% wind
    [Fact]
    public void CalculateProduction_Should_Return_Zero_For_WindTurbine_With_Zero_Wind()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.windturbine, Pmax = 100m };

        // Act
        var production = powerplant.CalculateProduction(100m, 0m);

        // Assert
        production.Should().Be(0m); // No production without wind
    }

    // Additional test to check production for gas-fired plant when Pmin is zero
    [Fact]
    public void CalculateProduction_Should_Return_Correct_Production_When_Pmin_Is_Zero()
    {
        // Arrange
        var powerplant = new Powerplant { Type = PowerplantType.gasfired, Pmin = 0m, Pmax = 200m };

        // Act
        var production = powerplant.CalculateProduction(50m, 0m);

        // Assert
        production.Should().Be(50m); // Should produce exactly the load
    }

    // Additional test to check exception for unknown powerplant type in cost calculation
    [Fact]
    public void CalculateCostPerMWh_Should_Throw_Exception_For_Unknown_PowerplantType()
    {
        // Arrange
        var powerplant = new Powerplant { Type = (PowerplantType)999, Efficiency = 0.5m };
        var fuels = new Fuels { Gas = 13.4m, Co2 = 20m };

        // Act
        Action act = () => powerplant.CalculateCostPerMWh(fuels);

        // Assert
        act.Should().Throw<NotImplementedException>().WithMessage("Cost calculation for * is not implemented.");
    }
}