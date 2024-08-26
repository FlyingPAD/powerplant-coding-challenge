using powerplant_coding_challenge.Models;
using Serilog;

namespace powerplant_coding_challenge.Helpers;

public static class LoggingHelper
{
    public static async Task LogRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        Log.Information($"Request Body: {requestBody}");
    }

    public static async Task<string> LogResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Log.Information($"Response Body: {responseBodyText}");
        return responseBodyText;
    }

    public static void LogSortedPowerplantsByCost(IEnumerable<Powerplant> powerplants, Fuels fuels)
    {
        Log.Information("Listing powerplants sorted by cost:");

        foreach (var powerplant in powerplants)
        {
            var costPerMWh = powerplant.CalculateCostPerMWh(fuels);
            Log.Information($"  - {powerplant.Name}: Cost per MWh = {costPerMWh:F2} EUR/MWh");
        }
    }

    public static void LogPowerplantEvaluation(Powerplant powerplant, decimal production, decimal windPercentage)
    {
        if (powerplant.Type == PowerplantTypeEnumeration.WindTurbine)
        {
            Log.Information($@"
                    => Evaluating {powerplant.Name}:
                    - Type: {powerplant.Type}
                    - Pmin: {powerplant.Pmin} MW
                    - Pmax: {powerplant.Pmax} MW
                    - Efficiency: {powerplant.Efficiency:F2}
                    - Producing: {production} MWh @ {windPercentage}% wind");
        }
        else
        {
            Log.Information($@"
                    => Evaluating {powerplant.Name}:
                    - Type: {powerplant.Type}
                    - Pmin: {powerplant.Pmin} MW
                    - Pmax: {powerplant.Pmax} MW
                    - Efficiency: {powerplant.Efficiency:F2}
                    - Producing: {production} MWh");
        }
    }

    public static void LogSkippedWindPlant(Powerplant plant, decimal remainingLoad, decimal windProduction, bool isWindBeneficial)
    {
        Log.Information($@"
                Skipping Wind Plant {plant.Name}:
                - Remaining Load = {remainingLoad} MWh
                - Potential Wind Production = {windProduction} MWh
                - Is Wind Beneficial: {isWindBeneficial}");
    }

    public static void LogThermalAllocation(Powerplant plant, decimal production, decimal remainingLoad)
    {
        Log.Information($@"
                Thermal Allocation: {plant.Name} allocated {production} MWh
                - Remaining Load after = {remainingLoad} MWh");
    }
}