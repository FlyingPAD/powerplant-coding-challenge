using FluentAssertions;
using Moq;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Models;
using Serilog;
using Xunit;

namespace powerplant_coding_challenge.Tests.Features;

public class AdvancedProductionPlanCommandHandlerTests
{
    private readonly Mock<ICostCalculator> _mockCostCalculator;
    private readonly Mock<IProductionCalculator> _mockProductionCalculator;
    private readonly ProductionPlanCommandHandler _handler;
    private readonly ProductionPlanCommandHandler _handlerCo2Enabled;
    private readonly Fuels _baseFuels;

    public AdvancedProductionPlanCommandHandlerTests()
    {
        _mockCostCalculator = new Mock<ICostCalculator>();
        _mockProductionCalculator = new Mock<IProductionCalculator>();
        _handler = new ProductionPlanCommandHandler(_mockCostCalculator.Object, _mockProductionCalculator.Object);
        _handlerCo2Enabled = new ProductionPlanCommandHandler(_mockCostCalculator.Object, _mockProductionCalculator.Object);
        _baseFuels = new Fuels { Co2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Load_Is_Too_High()
    {
        var command = new ProductionPlanCommand
        {
            Load = 500m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 50m, Pmax = 100m },
                new Powerplant { Name = "Gas2", Type = "gasfired", Efficiency = 0.5m, Pmin = 50m, Pmax = 100m }
            },
            Fuels = _baseFuels
        };

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Load_Is_Too_Low()
    {
        var command = new ProductionPlanCommand
        {
            Load = 20m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 50m, Pmax = 100m },
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 50m }
            },
            Fuels = _baseFuels
        };

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Wind_Enough()
    {
        var command = new ProductionPlanCommand
        {
            Load = 25m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 50m }
            },
            Fuels = _baseFuels
        };

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => plant.Type == "windturbine" ? wind * plant.Pmax / 100 : Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Wind1").Power.Should().Be("25.0");
        result.First(x => x.Name == "Gas1").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Wind_Not_Enough()
    {
        var command = new ProductionPlanCommand
        {
            Load = 50m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 50m }
            },
            Fuels = _baseFuels
        };

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => plant.Type == "windturbine" ? wind * plant.Pmax / 100 : Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Wind1").Power.Should().Be("25.0");
        result.First(x => x.Name == "Gas1").Power.Should().Be("25.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Wind_Too_Much()
    {
        var command = new ProductionPlanCommand
        {
            Load = 20m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 50m }
            },
            Fuels = _baseFuels
        };

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => plant.Type == "windturbine" ? wind * plant.Pmax / 100 : Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Wind1").Power.Should().Be("0.0");
        result.First(x => x.Name == "Gas1").Power.Should().Be("20.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Gas_Efficiency()
    {
        var command = new ProductionPlanCommand
        {
            Load = 20m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas2", Type = "gasfired", Efficiency = 0.6m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas3", Type = "gasfired", Efficiency = 0.8m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas4", Type = "gasfired", Efficiency = 0.3m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas5", Type = "gasfired", Efficiency = 0.45m, Pmin = 10m, Pmax = 100m }
            },
            Fuels = _baseFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Gas3").Power.Should().Be("20.0");
        result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum(x => decimal.Parse(x)).Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_When_All_Gas_Plants_Are_Needed()
    {
        var command = new ProductionPlanCommand
        {
            Load = 490m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas2", Type = "gasfired", Efficiency = 0.6m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas3", Type = "gasfired", Efficiency = 0.8m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas4", Type = "gasfired", Efficiency = 0.3m, Pmin = 10m, Pmax = 100m },
                new Powerplant { Name = "Gas5", Type = "gasfired", Efficiency = 0.45m, Pmin = 10m, Pmax = 100m }
            },
            Fuels = _baseFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Gas1").Power.Should().Be("100.0");
        result.First(x => x.Name == "Gas2").Power.Should().Be("100.0");
        result.First(x => x.Name == "Gas3").Power.Should().Be("100.0");
        result.First(x => x.Name == "Gas4").Power.Should().Be("90.0");
        result.First(x => x.Name == "Gas5").Power.Should().Be("100.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Gas_Pmin_Constraint()
    {
        var command = new ProductionPlanCommand
        {
            Load = 125m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 50m },
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 110m, Pmax = 200m },
                new Powerplant { Name = "Gas2", Type = "gasfired", Efficiency = 0.8m, Pmin = 80m, Pmax = 150m }
            },
            Fuels = _baseFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Gas2").Power.Should().Be("100.0");
        result.First(x => x.Name == "Gas1").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Kerosine_Plant()
    {
        var command = new ProductionPlanCommand
        {
            Load = 100m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Wind1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Kerosine1", Type = "turbojet", Efficiency = 0.5m, Pmin = 0m, Pmax = 200m }
            },
            Fuels = _baseFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "turbojet" ? fuels.Kerosine / plant.Efficiency : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => Math.Min(load, plant.Pmax));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.First(x => x.Name == "Gas1").Power.Should().Be("0.0");
        result.First(x => x.Name == "Kerosine1").Power.Should().Be("25.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_When_CO2_Is_Considered()
    {
        var command = new ProductionPlanCommand
        {
            Load = 150m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "Gas1", Type = "gasfired", Efficiency = 0.3m, Pmin = 100m, Pmax = 200m },
                new Powerplant { Name = "Kerosine1", Type = "turbojet", Efficiency = 1m, Pmin = 0m, Pmax = 200m }
            },
            Fuels = _baseFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "turbojet" ? fuels.Kerosine / plant.Efficiency : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) => Math.Min(load, plant.Pmax));

        var resultNoCO2 = await _handler.Handle(command, CancellationToken.None);
        var resultCO2 = await _handlerCo2Enabled.Handle(command, CancellationToken.None);

        resultNoCO2.First(x => x.Name == "Gas1").Power.Should().Be("150.0");
        resultCO2.First(x => x.Name == "Kerosine1").Power.Should().Be("150.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Tricky_Test1()
    {
        var trickyFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 20m, Wind = 100m };
        var command = new ProductionPlanCommand
        {
            Load = 60m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 20m },
                new Powerplant { Name = "gasfired", Type = "gasfired", Efficiency = 0.9m, Pmin = 50m, Pmax = 100m },
                new Powerplant { Name = "gasfiredinefficient", Type = "gasfired", Efficiency = 0.1m, Pmin = 0m, Pmax = 100m }
            },
            Fuels = trickyFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "windturbine" ? 0m : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) =>
                                 {
                                     if (plant.Type == "windturbine")
                                         return plant.Pmax * wind / 100;
                                     return Math.Min(load, plant.Pmax);
                                 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Select(x => x.Power).Sum(x => decimal.Parse(x)).Should().Be(60m);
        result.First(x => x.Name == "windpark1").Power.Should().Be("0.0");
        result.First(x => x.Name == "gasfired").Power.Should().Be("60.0");
        result.First(x => x.Name == "gasfiredinefficient").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_Tricky_Test2()
    {
        var trickyFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 20m, Wind = 100m };
        var command = new ProductionPlanCommand
        {
            Load = 80m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 60m },
                new Powerplant { Name = "gasfired", Type = "gasfired", Efficiency = 0.9m, Pmin = 50m, Pmax = 100m },
                new Powerplant { Name = "gasfiredinefficient", Type = "gasfired", Efficiency = 0.1m, Pmin = 0m, Pmax = 200m }
            },
            Fuels = trickyFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "windturbine" ? 0m : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) =>
                                 {
                                     if (plant.Type == "windturbine")
                                         return plant.Pmax * wind / 100;
                                     return Math.Min(load, plant.Pmax);
                                 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Select(x => x.Power).Sum(x => decimal.Parse(x)).Should().Be(80m);
        result.First(x => x.Name == "windpark1").Power.Should().Be("0.0");
        result.First(x => x.Name == "gasfired").Power.Should().Be("80.0");
        result.First(x => x.Name == "gasfiredinefficient").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload1_NoCO2()
    {
        // Arrange
        var exampleFuels = new Fuels
        {
            Co2 = 0,
            Kerosine = 50.8m,
            Gas = 13.4m,
            Wind = 60m
        };

        var command = new ProductionPlanCommand
        {
            Load = 480m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new Powerplant { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new Powerplant { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            },
            Fuels = exampleFuels
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Vérification si la liste n'est pas vide
        result.Should().NotBeNullOrEmpty();

        // Vérification si les centrales éoliennes produisent correctement
        var windPark1 = result.FirstOrDefault(x => x.Name == "windpark1");
        windPark1.Should().NotBeNull();
        windPark1.Power.Should().Be("90.0");

        var windPark2 = result.FirstOrDefault(x => x.Name == "windpark2");
        windPark2.Should().NotBeNull();
        windPark2.Power.Should().Be("21.6");

        // Vérification si la production totale est correcte
        var totalProduction = result.Select(x => decimal.Parse(x.Power)).Sum();
        totalProduction.Should().Be(480m); // Vérification de la production totale

        // Vérification des valeurs individuelles
        result.First(x => x.Name == "gasfiredbig1").Power.Should().Be("368.4");
        result.First(x => x.Name == "gasfiredbig2").Power.Should().Be("0.0");
        result.First(x => x.Name == "gasfiredsomewhatsmaller").Power.Should().Be("0.0");
        result.First(x => x.Name == "tj1").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload2_NoCO2()
    {
        var exampleFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 0m };
        var command = new ProductionPlanCommand
        {
            Load = 480m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new Powerplant { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new Powerplant { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            },
            Fuels = exampleFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "windturbine" ? 0m : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) =>
                                 {
                                     if (plant.Type == "windturbine")
                                         return plant.Pmax * wind / 100;
                                     return Math.Min(load, plant.Pmax);
                                 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Select(x => x.Power).Sum(x => decimal.Parse(x)).Should().Be(480m);
        result.First(x => x.Name == "windpark1").Power.Should().Be("0.0");
        result.First(x => x.Name == "windpark2").Power.Should().Be("0.0");
        result.First(x => x.Name == "gasfiredbig1").Power.Should().Be("380.0");
        result.First(x => x.Name == "gasfiredbig2").Power.Should().Be("100.0");
        result.First(x => x.Name == "gasfiredsomewhatsmaller").Power.Should().Be("0.0");
        result.First(x => x.Name == "tj1").Power.Should().Be("0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload3_NoCO2()
    {
        var exampleFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 60m };
        var command = new ProductionPlanCommand
        {
            Load = 910m,
            Powerplants = new List<Powerplant>
            {
                new Powerplant { Name = "gasfiredbig1", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredbig2", Type = "gasfired", Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                new Powerplant { Name = "gasfiredsomewhatsmaller", Type = "gasfired", Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                new Powerplant { Name = "tj1", Type = "turbojet", Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                new Powerplant { Name = "windpark1", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                new Powerplant { Name = "windpark2", Type = "windturbine", Efficiency = 1m, Pmin = 0m, Pmax = 36m }
            },
            Fuels = exampleFuels
        };

        _mockCostCalculator.Setup(x => x.CalculateCostPerMWh(It.IsAny<Powerplant>(), It.IsAny<Fuels>()))
                           .Returns<Powerplant, Fuels>((plant, fuels) => plant.Type == "windturbine" ? 0m : fuels.Gas / plant.Efficiency + 0.3m * fuels.Co2);

        _mockProductionCalculator.Setup(x => x.CalculateProduction(It.IsAny<Powerplant>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                                 .Returns<Powerplant, decimal, decimal>((plant, load, wind) =>
                                 {
                                     if (plant.Type == "windturbine")
                                         return plant.Pmax * wind / 100;
                                     return Math.Min(load, plant.Pmax);
                                 });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Select(x => x.Power).Sum(x => decimal.Parse(x)).Should().Be(910m);
        result.First(x => x.Name == "windpark1").Power.Should().Be("90.0");
        result.First(x => x.Name == "windpark2").Power.Should().Be("21.6");
        result.First(x => x.Name == "gasfiredbig1").Power.Should().Be("460.0");
        result.First(x => x.Name == "gasfiredbig2").Power.Should().Be("338.4");
        result.First(x => x.Name == "gasfiredsomewhatsmaller").Power.Should().Be("0.0");
        result.First(x => x.Name == "tj1").Power.Should().Be("0.0");
    }
}
