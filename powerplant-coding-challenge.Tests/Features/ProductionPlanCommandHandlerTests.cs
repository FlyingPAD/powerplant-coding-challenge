using FluentAssertions;
using Moq;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests.Features;

public class ProductionPlanCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Expected_ProductionPlan()
    {
        // Arrange
        var mockCostCalculator = new Mock<ICostCalculator>();
        var mockProductionCalculator = new Mock<IProductionCalculator>();

        mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>())).Returns(10);
        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<double>(), It.IsAny<double>())).Returns(100);

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 200,
            Powerplants = [
                new Powerplant { Name = "TestPlant", Type = "gasfired", Efficiency = 0.5, Pmin = 100, Pmax = 200 }
            ],
            Fuels = new Fuels { Gas = 13.4, Kerosine = 50.8, Co2 = 20, Wind = 60 }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("TestPlant");
        result[0].Power.Should().Be("100.0");
    }

    [Fact]
    public async Task Handle_Should_Distribute_Load_Among_Powerplants()
    {
        // Arrange
        var mockCostCalculator = new Mock<ICostCalculator>();
        var mockProductionCalculator = new Mock<IProductionCalculator>();

        mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>())).Returns(10);
        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<double>(), It.IsAny<double>())).Returns((Powerplant p, double load, double wind) => Math.Min(p.Pmax, load));

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 300,
            Powerplants = [
                new Powerplant { Name = "Plant1", Type = "gasfired", Efficiency = 0.5, Pmin = 100, Pmax = 200 },
                new Powerplant { Name = "Plant2", Type = "gasfired", Efficiency = 0.5, Pmin = 100, Pmax = 200 }
            ],
            Fuels = new Fuels { Gas = 13.4, Kerosine = 50.8, Co2 = 20, Wind = 60 }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Power.Should().Be("200.0");
        result[1].Power.Should().Be("100.0");
    }
}
