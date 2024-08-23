using powerplant_coding_challenge.Models;
using Serilog;

namespace powerplant_coding_challenge.Helpers;

public static class LoggingHelper
{
    public static void LogPowerplantEvaluation(Powerplant powerplant, decimal production, decimal windPercentage)
    {
        Log.Information(
            " => Evaluating {PlantName} :\n" +
            "  - Type : {Type} \n" +
            "  - Pmin : {Pmin} MW \n" +
            "  - Pmax : {Pmax} MW \n" +
            "  - Efficiency : {Efficiency:F2} \n" +
            "  - Producing : {Production} MWh @ {Wind}% wind",
            powerplant.Name, powerplant.Type, powerplant.Pmin, powerplant.Pmax, powerplant.Efficiency,
            production, windPercentage
        );
    }

    public static void LogSkippedPowerplant(Powerplant powerplant)
    {
        Log.Information(
            " => Skipped {PlantName} : Remaining load is 0.0 MWh.",
            powerplant.Name
        );
    }

    public static void LogProductionCost(decimal production, decimal cost)
    {
        Log.Information(
            " => Producing : {Production} MWh ({Cost:F2} EUR)\n" +
            "  - Cost Breakdown :\n" +
            "      Total Cost : {Cost:F2} EUR",
            production, cost
        );
    }

    public static void LogFinalSummary(decimal totalProduction, decimal totalCost)
    {
        Log.Information(" -> Load Processed : {TotalLoad:F1} MWh | Total Cost : {TotalCost:F2} EUR/h", totalProduction, totalCost);
    }

    public static void LogDiscrepancy(decimal discrepancy)
    {
        if (discrepancy > 0.1m)
        {
            Log.Warning("Discrepancy : {Discrepancy:F1} MWh. Production exceeds the requested load.", discrepancy);
        }
        else if (discrepancy < -0.1m)
        {
            Log.Warning("Discrepancy : {Discrepancy:F1} MWh. Production is insufficient to meet the requested load.", discrepancy);
        }
        else
        {
            Log.Information("Discrepancy : {Discrepancy:F1} MWh. The production meets the requested load perfectly.", discrepancy);
        }
    }

    public static void LogRemainingLoadError(decimal remainingLoad)
    {
        Log.Error("La charge restante après allocation n'est pas zéro : {RemainingLoad} MWh. Une exception va être lancée.", remainingLoad);
    }
}
