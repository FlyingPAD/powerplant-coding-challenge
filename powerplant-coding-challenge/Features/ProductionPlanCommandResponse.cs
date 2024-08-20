using System.Globalization;
using System.Text.Json.Serialization;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommandResponse(string name, decimal power)
{
    public string Name { get; set; } = name;

    [JsonPropertyName("p")]
    public string Power { get; set; } = power.ToString("F1", CultureInfo.InvariantCulture);
}