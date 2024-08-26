using powerplant_coding_challenge.Models;
using Serilog;

namespace powerplant_coding_challenge.Helpers
{
    public static class LoggingHelper
    {
        public static async Task LogRequestAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            Log.Information("Request Body: {RequestBody}", requestBody);
        }

        public static async Task<string> LogResponseAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            Log.Information("Response Body: {ResponseBody}", responseBodyText);
            return responseBodyText;
        }

        public static void LogSortedPowerplantsByCost(IEnumerable<Powerplant> powerplants, Fuels fuels)
        {
            Log.Information("Listing powerplants sorted by cost:");

            foreach (var powerplant in powerplants)
            {
                var costPerMWh = powerplant.CalculateCostPerMWh(fuels);
                Log.Information(
                    "  - {PlantName}: Cost per MWh = {CostPerMWh:F2} EUR/MWh",
                    powerplant.Name, costPerMWh
                );
            }
        }

        public static void LogPowerplantEvaluation(Powerplant powerplant, decimal production, decimal windPercentage)
        {
            if (powerplant.Type == PowerplantTypeEnumeration.windturbine)
            {
                Log.Information(
                    " => Evaluating {PlantName}:\n" +
                    "  - Type: {Type} \n" +
                    "  - Pmin: {Pmin} MW \n" +
                    "  - Pmax: {Pmax} MW \n" +
                    "  - Efficiency: {Efficiency:F2} \n" +
                    "  - Producing: {Production} MWh @ {Wind}% wind",
                    powerplant.Name, powerplant.Type, powerplant.Pmin, powerplant.Pmax, powerplant.Efficiency,
                    production, windPercentage
                );
            }
            else
            {
                Log.Information(
                    " => Evaluating {PlantName}:\n" +
                    "  - Type: {Type} \n" +
                    "  - Pmin: {Pmin} MW \n" +
                    "  - Pmax: {Pmax} MW \n" +
                    "  - Efficiency: {Efficiency:F2} \n" +
                    "  - Producing: {Production} MWh",
                    powerplant.Name, powerplant.Type, powerplant.Pmin, powerplant.Pmax, powerplant.Efficiency,
                    production
                );
            }
        }

        public static void LogFinalSummary(decimal totalProduction, decimal totalCost)
        {
            Log.Information(" -> Load Processed: {TotalLoad:F1} MWh | Total Cost: {TotalCost:F2} EUR/h", totalProduction, totalCost);
        }

        public static void LogRemainingLoadError(decimal remainingLoad)
        {
            Log.Error("The remaining load after allocation is not zero: {RemainingLoad} MWh. An exception will be thrown.", remainingLoad);
        }
    }
}