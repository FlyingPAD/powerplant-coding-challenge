using FluentAssertions;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;
using Xunit;

namespace powerplant_coding_challenge.Tests
{
    public class ProductionPlanCommandHandlerIntegrationTests
    {
        [Fact]
        public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload1()
        {
            // Arrange
            var handler = new ProductionPlanCommandHandler();

            var command = new ProductionPlanCommand
            {
                Load = 480m,
                Powerplants =
                [
                    new() { Name = "gasfiredbig1", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new() { Name = "gasfiredbig2", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new() { Name = "gasfiredsomewhatsmaller", Type = PowerplantType.gasfired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new() { Name = "tj1", Type = PowerplantType.turbojet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new() { Name = "windpark1", Type = PowerplantType.windturbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new() { Name = "windpark2", Type = PowerplantType.windturbine, Efficiency = 1m, Pmin = 0m, Pmax = 36m }
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
                new { Name = "windpark1", Power = 90.0m },
                new { Name = "windpark2", Power = 21.6m },
                new { Name = "gasfiredbig1", Power = 368.4m },
                new { Name = "gasfiredbig2", Power = 0.0m },
                new { Name = "gasfiredsomewhatsmaller", Power = 0.0m },
                new { Name = "tj1", Power = 0.0m }
            };

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Name.Should().Be(expectedValues[i].Name);
                result[i].Power.Should().Be(expectedValues[i].Power);
            }
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload2()
        {
            // Arrange
            var handler = new ProductionPlanCommandHandler();

            var command = new ProductionPlanCommand
            {
                Load = 480m,
                Powerplants =
                [
                    new Powerplant { Name = "gasfiredbig1", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "gasfiredbig2", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new Powerplant { Name = "gasfiredsomewhatsmaller", Type = PowerplantType.gasfired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new Powerplant { Name = "tj1", Type = PowerplantType.turbojet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new Powerplant { Name = "windpark1", Type = PowerplantType.windturbine, Efficiency = 1.00m, Pmin = 0m, Pmax = 150m },
                    new Powerplant { Name = "windpark2", Type = PowerplantType.windturbine, Efficiency = 1.00m, Pmin = 0m, Pmax = 36m }
                ],
                Fuels = new Fuels { Gas = 13.4m, Kerosine = 50.8m, Co2 = 20m, Wind = 0m }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);

            var expectedValues = new[]
            {
                new { Name = "windpark1", Power = 0.0m },
                new { Name = "windpark2", Power = 0.0m },
                new { Name = "gasfiredbig1", Power = 460.0m },
                new { Name = "gasfiredbig2", Power = 4.0m },
                new { Name = "gasfiredsomewhatsmaller", Power = 0.0m },
                new { Name = "tj1", Power = 16.0m }
            };

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Name.Should().Be(expectedValues[i].Name);
                result[i].Power.Should().Be(expectedValues[i].Power);
            }
        }

        [Fact]
        public async Task Handle_Should_Return_Expected_ProductionPlan_For_Payload3()
        {
            // Arrange
            var handler = new ProductionPlanCommandHandler();

            var command = new ProductionPlanCommand
            {
                Load = 910m,
                Powerplants =
                [
                    new() { Name = "gasfiredbig1", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new() { Name = "gasfiredbig2", Type = PowerplantType.gasfired, Efficiency = 0.53m, Pmin = 100m, Pmax = 460m },
                    new() { Name = "gasfiredsomewhatsmaller", Type = PowerplantType.gasfired, Efficiency = 0.37m, Pmin = 40m, Pmax = 210m },
                    new() { Name = "tj1", Type = PowerplantType.turbojet, Efficiency = 0.3m, Pmin = 0m, Pmax = 16m },
                    new() { Name = "windpark1", Type = PowerplantType.windturbine, Efficiency = 1m, Pmin = 0m, Pmax = 150m },
                    new() { Name = "windpark2", Type = PowerplantType.windturbine, Efficiency = 1m, Pmin = 0m, Pmax = 36m }
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
                new { Name = "windpark1", Power = 90.0m },
                new { Name = "windpark2", Power = 21.6m },
                new { Name = "gasfiredbig1", Power = 460.0m },
                new { Name = "gasfiredbig2", Power = 338.4m },
                new { Name = "gasfiredsomewhatsmaller", Power = 0.0m },
                new { Name = "tj1", Power = 0.0m }
            };

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Name.Should().Be(expectedValues[i].Name);
                result[i].Power.Should().Be(expectedValues[i].Power);
            }
        }
    }
}
