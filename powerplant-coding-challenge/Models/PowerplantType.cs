using System.Text.Json.Serialization;

namespace powerplant_coding_challenge.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PowerplantType
{
    gasfired,
    turbojet,
    windturbine
}