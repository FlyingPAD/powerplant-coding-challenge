using System.Text.Json.Serialization;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandResponse(string name, decimal power)
{
    public string Name { get; } = name;

    [JsonPropertyName("p")]
    public decimal Power { get; } = Math.Round(power, 1, MidpointRounding.ToEven);
}