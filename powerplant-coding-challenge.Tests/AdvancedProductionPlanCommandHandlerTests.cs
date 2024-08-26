using FluentAssertions;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;
using powerplant_coding_challenge.Services;
using FluentValidation;
using Xunit;

namespace powerplant_coding_challenge.Tests
{
    public class AdvancedProductionPlanCommandHandlerTests
    {
        private readonly ProductionPlanCommandHandler _handler;
        private readonly Fuels _baseFuels;

        public AdvancedProductionPlanCommandHandlerTests()
        {
            var productionPlanValidator = new ProductionPlanValidatorService();
            var productionManager = new ProductionPlanService(productionPlanValidator);
            var commandValidator = new ProductionPlanCommandValidator();

            _handler = new ProductionPlanCommandHandler(productionManager, commandValidator);

            _baseFuels = new Fuels { Co2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Load_Is_Too_High()
        {
            // Test to ensure that an exception is thrown when the requested load exceeds the total available capacity.
            var command = new ProductionPlanCommand
            {
                Load = 500m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 50m, Pmax = 100m },
                    new Powerplant { Name = "Gas2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 50m, Pmax = 100m }
                },
                Fuels = _baseFuels
            };

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("The requested load exceeds the total capacity of the available power plants.");
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Load_Is_Too_Low()
        {
            // Test to ensure that an exception is thrown when the requested load is below the minimum threshold (Pmin) of non-wind powerplants.
            var command = new ProductionPlanCommand
            {
                Load = 20m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 50m, Pmax = 100m },
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 50m }
                },
                Fuels = _baseFuels
            };

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("The requested load is below the minimum production capacity (Pmin) of all non-wind power plants.");
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Wind_Enough()
        {
            // Test to check if wind power is sufficient to meet the load without using other powerplants.
            var command = new ProductionPlanCommand
            {
                Load = 25m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 50m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Wind1").Power.Should().Be(25.0m);
            result.First(x => x.Name == "Gas1").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Wind_Not_Enough()
        {
            // Test to verify that when wind power is insufficient, additional powerplants are used to meet the load.
            var command = new ProductionPlanCommand
            {
                Load = 50m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 50m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Wind1").Power.Should().Be(25.0m);
            result.First(x => x.Name == "Gas1").Power.Should().Be(25.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Wind_Too_Much()
        {
            // Test to verify that wind power production is adjusted when it exceeds the required load.
            var command = new ProductionPlanCommand
            {
                Load = 20m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 50m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Wind1").Power.Should().Be(0.0m);
            result.First(x => x.Name == "Gas1").Power.Should().Be(20.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Gas_Efficiency()
        {
            // Test to ensure that the most efficient gas plant is used first to meet the load.
            var command = new ProductionPlanCommand
            {
                Load = 20m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.6m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas3", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.8m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas4", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.3m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas5", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.45m, Pmin = 10m, Pmax = 100m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Gas3").Power.Should().Be(20.0m);
            result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum().Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_When_All_Gas_Plants_Are_Needed()
        {
            // Test to verify that all gas plants are used when the load requires it.
            var command = new ProductionPlanCommand
            {
                Load = 490m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.6m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas3", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.8m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas4", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.3m, Pmin = 10m, Pmax = 100m },
                    new Powerplant { Name = "Gas5", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.45m, Pmin = 10m, Pmax = 100m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Gas1").Power.Should().Be(100.0m);
            result.First(x => x.Name == "Gas2").Power.Should().Be(100.0m);
            result.First(x => x.Name == "Gas3").Power.Should().Be(100.0m);
            result.First(x => x.Name == "Gas4").Power.Should().Be(90.0m);
            result.First(x => x.Name == "Gas5").Power.Should().Be(100.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Gas_Pmin_Constraint()
        {
            // Test to ensure that the gas plants respect the Pmin constraint during production.
            var command = new ProductionPlanCommand
            {
                Load = 125m,
                Powerplants =
                {
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 50m },
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 110m, Pmax = 200m },
                    new Powerplant { Name = "Gas2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.8m, Pmin = 80m, Pmax = 150m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Gas2").Power.Should().Be(100.0m);
            result.First(x => x.Name == "Gas1").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Kerosine_Plant()
        {
            // Test to ensure that the kerosine (TurboJet) plant is utilized correctly.
            var command = new ProductionPlanCommand
            {
                Load = 100m,
                Powerplants =
                {
                    new Powerplant { Name = "Wind1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.5m, Pmin = 100m, Pmax = 200m },
                    new Powerplant { Name = "Kerosine1", Type = PowerplantTypeEnumeration.TurboJet, Efficiency = 0.5m, Pmin = 0m, Pmax = 200m }
                },
                Fuels = _baseFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.First(x => x.Name == "Gas1").Power.Should().Be(0.0m);
            result.First(x => x.Name == "Kerosine1").Power.Should().Be(25.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_When_CO2_Is_Considered()
        {
            // Test to ensure that CO2 costs are correctly factored into the decision-making process.
            var command = new ProductionPlanCommand
            {
                Load = 150m,
                Powerplants =
                {
                    new Powerplant { Name = "Gas1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.3m, Pmin = 100m, Pmax = 200m },
                    new Powerplant { Name = "Kerosine1", Type = PowerplantTypeEnumeration.TurboJet, Efficiency = 1m, Pmin = 0m, Pmax = 200m }
                },
                Fuels = _baseFuels
            };

            var resultNoCO2 = await _handler.Handle(command, CancellationToken.None);

            resultNoCO2.First(x => x.Name == "Gas1").Power.Should().Be(150.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Tricky_Test1()
        {
            // Test with tricky inputs to ensure the correct powerplants are used in complex scenarios.
            var trickyFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 20m, Wind = 100m };
            var command = new ProductionPlanCommand
            {
                Load = 60m,
                Powerplants =
                {
                    new Powerplant { Name = "windpark1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 20m },
                    new Powerplant { Name = "GasFired", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.9m, Pmin = 50m, Pmax = 100m },
                    new Powerplant { Name = "GasFiredinefficient", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.1m, Pmin = 0m, Pmax = 100m }
                },
                Fuels = trickyFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Select(x => x.Power).Sum().Should().Be(60m);
            result.First(x => x.Name == "windpark1").Power.Should().Be(0.0m);
            result.First(x => x.Name == "GasFired").Power.Should().Be(60.0m);
            result.First(x => x.Name == "GasFiredinefficient").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_Tricky_Test2()
        {
            // Another tricky test to verify that powerplants are chosen correctly when multiple constraints are in play.
            var trickyFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 20m, Wind = 100m };
            var command = new ProductionPlanCommand
            {
                Load = 80m,
                Powerplants =
                {
                    new Powerplant { Name = "windpark1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 60m },
                    new Powerplant { Name = "GasFired", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.9m, Pmin = 50m, Pmax = 100m },
                    new Powerplant { Name = "GasFiredinefficient", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.1m, Pmin = 0m, Pmax = 200m }
                },
                Fuels = trickyFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Select(x => x.Power).Sum().Should().Be(80m);
            result.First(x => x.Name == "windpark1").Power.Should().Be(0.0m);
            result.First(x => x.Name == "GasFired").Power.Should().Be(80.0m);
            result.First(x => x.Name == "GasFiredinefficient").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload1_NoCO2()
        {
            // Test with a sample payload to ensure the handler returns the expected production plan without CO2 costs.
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
                Powerplants =
                {
                    new Powerplant { Name = "GasFiredbig1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredbig2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredsomewhatsmaller", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new Powerplant { Name = "tj1", Type = PowerplantTypeEnumeration.TurboJet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new Powerplant { Name = "windpark1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new Powerplant { Name = "windpark2", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 36m }
                },
                Fuels = exampleFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNullOrEmpty();

            var windPark1 = result.FirstOrDefault(x => x.Name == "windpark1");
            windPark1.Should().NotBeNull();
            windPark1!.Power.Should().Be(90.0m);

            var windPark2 = result.FirstOrDefault(x => x.Name == "windpark2");
            windPark2.Should().NotBeNull();
            windPark2!.Power.Should().Be(21.6m);

            var totalProduction = result.Select(x => x.Power).Sum();
            totalProduction.Should().Be(480m);

            result.First(x => x.Name == "GasFiredbig1").Power.Should().Be(368.4m);
            result.First(x => x.Name == "GasFiredbig2").Power.Should().Be(0.0m);
            result.First(x => x.Name == "GasFiredsomewhatsmaller").Power.Should().Be(0.0m);
            result.First(x => x.Name == "tj1").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload2_NoCO2()
        {
            // Another test with different wind values to ensure the production plan is correct.
            var exampleFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 0m };
            var command = new ProductionPlanCommand
            {
                Load = 480m,
                Powerplants =
                {
                    new Powerplant { Name = "GasFiredbig1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredbig2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredsomewhatsmaller", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new Powerplant { Name = "tj1", Type = PowerplantTypeEnumeration.TurboJet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new Powerplant { Name = "windpark1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new Powerplant { Name = "windpark2", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 36m }
                },
                Fuels = exampleFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Select(x => x.Power).Sum().Should().Be(480m);
            result.First(x => x.Name == "windpark1").Power.Should().Be(0.0m);
            result.First(x => x.Name == "windpark2").Power.Should().Be(0.0m);
            result.First(x => x.Name == "GasFiredbig1").Power.Should().Be(380.0m);
            result.First(x => x.Name == "GasFiredbig2").Power.Should().Be(100.0m);
            result.First(x => x.Name == "GasFiredsomewhatsmaller").Power.Should().Be(0.0m);
            result.First(x => x.Name == "tj1").Power.Should().Be(0.0m);
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_Results_For_ExamplePayload3_NoCO2()
        {
            // Test with maximum load to verify the full operation of powerplants.
            var exampleFuels = new Fuels { Co2 = 0, Kerosine = 50.8m, Gas = 13.4m, Wind = 60m };
            var command = new ProductionPlanCommand
            {
                Load = 910m,
                Powerplants =
                {
                    new Powerplant { Name = "GasFiredbig1", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredbig2", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "GasFiredsomewhatsmaller", Type = PowerplantTypeEnumeration.GasFired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new Powerplant { Name = "tj1", Type = PowerplantTypeEnumeration.TurboJet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new Powerplant { Name = "windpark1", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new Powerplant { Name = "windpark2", Type = PowerplantTypeEnumeration.WindTurbine, Efficiency = 1m, Pmin = 0m, Pmax = 36m }
                },
                Fuels = exampleFuels
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Select(x => x.Power).Sum().Should().Be(910m);
            result.First(x => x.Name == "windpark1").Power.Should().Be(90.0m);
            result.First(x => x.Name == "windpark2").Power.Should().Be(21.6m);
            result.First(x => x.Name == "GasFiredbig1").Power.Should().Be(460.0m);
            result.First(x => x.Name == "GasFiredbig2").Power.Should().Be(338.4m);
            result.First(x => x.Name == "GasFiredsomewhatsmaller").Power.Should().Be(0.0m);
            result.First(x => x.Name == "tj1").Power.Should().Be(0.0m);
        }
    }
}