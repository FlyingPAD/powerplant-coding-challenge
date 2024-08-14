using FluentAssertions;
using Moq;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using Xunit;
using System.Globalization;

namespace powerplant_coding_challenge.Tests.Features;

public class ProductionPlanCommandHandlerIntegrationTests
{
    [Fact]
    public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload1()
    {
        // Arrange
        var costCalculator = new CostCalculatorService();
        var productionCalculator = new ProductionCalculatorService();

        var handler = new ProductionPlanCommandHandler(costCalculator, productionCalculator);

        var command = new ProductionPlanCommand
        {
            Load = 480m,
            Powerplants =
            [
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new Powerplant { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new Powerplant { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);

        var expectedValues = new[]
        {
            90.0m,   // windpark1
            21.6m,   // windpark2
            368.4m,  // gasfiredbig1
            0.0m,    // gasfiredbig2
            0.0m,    // gasfiredsomewhatsmaller
            0.0m     // tj1
        };

        for (int i = 0; i < result.Count; i++)
        {
            result[i].Power.Should().Be(expectedValues[i].ToString("0.0", CultureInfo.InvariantCulture));
        }
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
                "gasfired" => (f.Gas / p.Efficiency) + (0.3m * f.Co2),
                "turbojet" => f.Kerosine / p.Efficiency,
                "windturbine" => 0m,
                _ => decimal.MaxValue,
            };
        });

        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>())).Returns((Powerplant p, decimal load, decimal wind) =>
        {
            return p.Type.ToLower() switch
            {
                "windturbine" => p.Pmax * (wind / 100.0m),
                _ => Math.Min(p.Pmax, Math.Max(p.Pmin, load)),
            };
        });

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 480m,
            Powerplants =
            [
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new() { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new() { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new() { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new() { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new() { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 0m }
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
                "gasfired" => (f.Gas / p.Efficiency) + (0.3m * f.Co2),
                "turbojet" => f.Kerosine / p.Efficiency,
                "windturbine" => 0m,
                _ => decimal.MaxValue,
            };
        });

        mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>())).Returns((Powerplant p, decimal load, decimal wind) =>
        {
            return p.Type.ToLower() switch
            {
                "windturbine" => p.Pmax * (wind / 100.0m),
                _ => Math.Min(p.Pmax, Math.Max(p.Pmin, load)),
            };
        });

        var handler = new ProductionPlanCommandHandler(mockCostCalculator.Object, mockProductionCalculator.Object);

        var command = new ProductionPlanCommand
        {
            Load = 480m,
            Powerplants =
            [
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new() { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new() { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new() { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new() { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new() { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            ],
            Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 60m }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6);
        // Ajoutez ici les vérifications spécifiques pour le résultat attendu de ce payload
    }
}
