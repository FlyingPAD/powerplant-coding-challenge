using System.Text.Json.Serialization;

namespace powerplant_coding_challenge.ProductionPlan;

public class ProductionPlanCommandResponse(string name, string power)
{
    public string Name { get; set; } = name;

    [JsonPropertyName("p")]
    public string Power { get; set; } = power;
}
