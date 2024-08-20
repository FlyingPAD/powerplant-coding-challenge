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

    // Test to verify that the handler throws an exception when the load exceeds the total Pmax of all powerplants
    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Load_Exceeds_Total_Pmax()
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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await handler.Handle(command, CancellationToken.None);
        });
    }

    // Test to verify that the handler correctly assigns minimum production for plants when load is low
    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Load_Is_Below_Pmin()
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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
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
