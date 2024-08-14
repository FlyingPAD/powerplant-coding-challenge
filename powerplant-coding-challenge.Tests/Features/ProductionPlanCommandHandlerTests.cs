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
    public async Task Handle_Should_Distribute_Load_Among_Powerplants()
    {
        // Arrange
        var mockCostCalculator = new Mock<ICostCalculator>();
        var mockProductionCalculator = new Mock<IProductionCalculator>();

        mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>())).Returns(10m);
        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>())).Returns((Powerplant p, decimal load, decimal wind) => Math.Min(p.Pmax, load));

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 300m,
            Powerplants =
            [
                new Powerplant { Name = "Plant1", Type = "gasfired", Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Plant2", Type = "gasfired", Efficiency = 0.5m, Pmin = 100m, Pmax = 200m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var expectedPowerPlant1 = "200.0";
        var expectedPowerPlant2 = "100.0";

        // Vérification des valeurs retournées
        result[0].Power.Should().Be(expectedPowerPlant1);
        result[1].Power.Should().Be(expectedPowerPlant2);

        // Vérification que CalculateProduction a été appelé avec les bons paramètres
        mockProductionCalculator.Verify(x => x.CalculateProduction(
            It.Is<Powerplant>(p => p.Name == "Plant1"),
            It.Is<decimal>(load => load == 300m), // Load initialement assigné à la première centrale
            It.Is<decimal>(wind => wind == 60m)), Times.Once);

        mockProductionCalculator.Verify(x => x.CalculateProduction(
            It.Is<Powerplant>(p => p.Name == "Plant2"),
            It.Is<decimal>(load => load == 100m), // La charge restante assignée à la deuxième centrale
            It.Is<decimal>(wind => wind == 60m)), Times.Once);
    }
}
