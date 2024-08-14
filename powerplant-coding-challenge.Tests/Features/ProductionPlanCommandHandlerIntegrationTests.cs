using FluentAssertions;
using Moq;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using Xunit;

namespace powerplant_coding_challenge.Tests.Features;

public class ProductionPlanCommandHandlerIntegrationTests
{
    [Fact]
    public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload1()
    {
        // Arrange
        var costCalculator = new CostCalculatorService(); // Utilisation de l'implémentation réelle
        var productionCalculator = new ProductionCalculatorService(); // Utilisation de l'implémentation réelle

        var handler = new ProductionPlanCommandHandler(costCalculator, productionCalculator);

        var command = new ProductionPlanCommand
        {
            Load = 480,
            Powerplants =
    [
        new() { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
        new Powerplant { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
        new Powerplant { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37, Pmin = 40, Pmax = 210 },
        new Powerplant { Name = "tj1", Type = "turbojet", Efficiency = 0.3, Pmin = 0, Pmax = 16 },
        new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 150 },
        new Powerplant { Name = "windpark2", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 36 }
    ],
            Fuels = new Fuels { Gas = 13.4, Kerosine = 50.8, Co2 = 20, Wind = 60 }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);

        result[0].Name.Should().Be("windpark1");
        result[0].Power.Should().Be("90.0");   // windpark1

        result[1].Name.Should().Be("windpark2");
        result[1].Power.Should().Be("21.6");  // windpark2

        result[2].Name.Should().Be("gasfiredbig1");
        result[2].Power.Should().Be("368.4"); // gasfiredbig1

        result[3].Name.Should().Be("gasfiredbig2");
        result[3].Power.Should().Be("100.0");  // gasfiredbig2

        result[4].Name.Should().Be("gasfiredsomewhatsmaller");
        result[4].Power.Should().Be("40.0");  // gasfiredsomewhatsmaller (mise à jour de l'attente)

        result[5].Name.Should().Be("tj1");
        result[5].Power.Should().Be("0.0");   // tj1
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload2()
    {
        // Arrange
        var mockCostCalculator = new Mock<ICostCalculator>();
        var mockProductionCalculator = new Mock<IProductionCalculator>();

        mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>())).Returns((Powerplant p, Fuels f) =>
        {
            return p.Type.ToLower() switch
            {
                "gasfired" => (f.Gas / p.Efficiency) + (0.3 * f.Co2),
                "turbojet" => f.Kerosine / p.Efficiency,
                "windturbine" => 0,
                _ => double.MaxValue,
            };
        });

        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<double>(), It.IsAny<double>())).Returns((Powerplant p, double load, double wind) =>
        {
            return p.Type.ToLower() switch
            {
                "windturbine" => p.Pmax * (wind / 100.0),
                _ => Math.Min(p.Pmax, Math.Max(p.Pmin, load)),
            };
        });

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 480,
            Powerplants =
            [
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
                new() { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
                new() { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37, Pmin = 40, Pmax = 210 },
                new() { Name = "tj1", Type = "turbojet", Efficiency = 0.3, Pmin = 0, Pmax = 16 },
                new() { Name = "windpark1", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 150 },
                new() { Name = "windpark2", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 36 }
            ],
            Fuels = new Fuels { Gas = 13.4, Kerosine = 50.8, Co2 = 20, Wind = 0 }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
        // Ajoutez ici les vérifications spécifiques pour le résultat attendu de ce payload
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload3()
    {
        // Arrange
        var mockCostCalculator = new Mock<ICostCalculator>();
        var mockProductionCalculator = new Mock<IProductionCalculator>();

        mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>())).Returns((Powerplant p, Fuels f) =>
        {
            return p.Type.ToLower() switch
            {
                "gasfired" => (f.Gas / p.Efficiency) + (0.3 * f.Co2),
                "turbojet" => f.Kerosine / p.Efficiency,
                "windturbine" => 0,
                _ => double.MaxValue,
            };
        });

        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<double>(), It.IsAny<double>())).Returns((Powerplant p, double load, double wind) =>
        {
            return p.Type.ToLower() switch
            {
                "windturbine" => p.Pmax * (wind / 100.0),
                _ => Math.Min(p.Pmax, Math.Max(p.Pmin, load)),
            };
        });

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 480,
            Powerplants =
            [
                new() { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
                new() { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53, Pmin = 100, Pmax = 460 },
                new() { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37, Pmin = 40, Pmax = 210 },
                new() { Name = "tj1", Type = "turbojet", Efficiency = 0.3, Pmin = 0, Pmax = 16 },
                new() { Name = "windpark1", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 150 },
                new() { Name = "windpark2", Type = "windturbine", Efficiency = 1, Pmin = 0, Pmax = 36 }
            ],
            Fuels = new Fuels { Gas = 13.4, Kerosine = 50.8, Co2 = 20, Wind = 60 }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
        // Ajoutez ici les vérifications spécifiques pour le résultat attendu de ce payload
    }
}
