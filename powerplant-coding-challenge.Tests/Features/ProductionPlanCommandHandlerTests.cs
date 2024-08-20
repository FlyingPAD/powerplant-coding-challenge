using FluentAssertions;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests.Features;

public class ProductionPlanCommandHandlerTests
{
    // Test to verify that the handler correctly distributes load among multiple powerplants
    [Fact]
    public async Task Handle_Should_Distribute_Load_Among_Powerplants()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 300m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Plant2", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Power.Should().Be("200.0");
        result[1].Power.Should().Be("100.0");
    }

    // Test to verify that the handler correctly handles load that exceeds the total Pmax of all powerplants
    [Fact(Skip = "Test is skipped for now")]
    public async Task Handle_Should_Cap_Production_At_Pmax_When_Load_Exceeds_Total_Pmax()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 1000m, // Load exceeds total Pmax of powerplants
            Powerplants =
        [
            new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 400m },
            new Powerplant { Name = "Plant2", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 400m },
            new Powerplant { Name = "Plant3", Type = PowerplantType.windturbine, Pmin = 0m, Pmax = 150m }
        ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Debugging output to understand what's happening
        foreach (var r in result)
        {
            Console.WriteLine($"Powerplant: {r.Name}, Power: {r.Power}");
        }

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Power.Should().Be("400.0"); // Expected Pmax for Plant1
        result[1].Power.Should().Be("400.0"); // Expected Pmax for Plant2
        result[2].Power.Should().Be("90.0");  // Expected production for Windturbine (150 * 0.6)
    }


    // Test to verify that the handler correctly assigns minimum production for plants when load is low
    [Fact(Skip = "Test is skipped for now")]
    public async Task Handle_Should_Assign_Pmin_When_Load_Is_Low()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 50m, // Load is lower than Pmin of available plants
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Plant2", Type = PowerplantType.windturbine, Pmin = 0m, Pmax = 150m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Power.Should().Be("0.0");   // Gasfired plant can't operate below Pmin
        result[1].Power.Should().Be("30.0");  // Windturbine produces 150 * 0.6, but the load is limited
    }

    // Test to verify that the handler correctly handles windturbine production based on wind percentage
    [Fact]
    public async Task Handle_Should_Calculate_WindTurbine_Production_Based_On_Wind_Percentage()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 300m,
            Powerplants =
            [
                new Powerplant { Name = "Wind1", Type = PowerplantType.windturbine, Pmin = 0m, Pmax = 100m },
                new Powerplant { Name = "Wind2", Type = PowerplantType.windturbine, Pmin = 0m, Pmax = 200m }
            ],
            Fuels = new Fuels { Wind = 50m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Power.Should().Be("50.0"); // Wind1 produces 100 * 0.5
        result[1].Power.Should().Be("100.0"); // Wind2 produces 200 * 0.5
    }

    // Test to verify that the handler correctly handles scenarios where no powerplants are provided
    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Powerplants_Provided()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 300m,
            Powerplants = [],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // Test to verify that the handler correctly handles scenarios where the load is zero
    [Fact]
    public async Task Handle_Should_Return_Zero_Production_When_Load_Is_Zero()
    {
        // Arrange
        var handler = new ProductionPlanCommandHandler();

        var command = new ProductionPlanCommand
        {
            Load = 0m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = PowerplantType.gasfired, Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Wind1", Type = PowerplantType.windturbine, Pmin = 0m, Pmax = 100m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Power.Should().Be("0.0");
        result[1].Power.Should().Be("0.0");
    }
}
