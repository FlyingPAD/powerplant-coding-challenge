namespace powerplant_coding_challenge.Models
{
    public class Powerplant
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Efficiency { get; set; }
        public double Pmin { get; set; }
        public double Pmax { get; set; }

        /// <summary>
        /// Calculates the cost per MWh based on the type of fuel and the efficiency of the powerplant.
        /// </summary>
        /// <param name="fuels">An object containing the prices of different fuels.</param>
        /// <returns>The calculated cost per MWh.</returns>
        public double CalculateCostPerMWh(Fuels fuels)
        {
            return Type.ToLower() switch
            {
                // Cost for gas-fired powerplants includes fuel cost and CO2 emission cost.
                "gasfired" => (fuels.Gas / Efficiency) + (0.3 * fuels.Co2),
                // Cost for turbojet powerplants is based only on kerosine consumption.
                "turbojet" => fuels.Kerosine / Efficiency,
                // Wind turbines have no fuel cost, so their cost per MWh is effectively zero.
                "windturbine" => 0,
                // Return a very high value for unsupported powerplant types.
                _ => double.MaxValue,
            };
        }

        /// <summary>
        /// Calculates the power output based on the load and wind percentage.
        /// </summary>
        /// <param name="load">The required load to be met by the powerplant.</param>
        /// <param name="windPercentage">The efficiency of wind turbines as a percentage of Pmax.</param>
        /// <returns>The calculated power production.</returns>
        public double CalculateProduction(double load, double windPercentage)
        {
            if (Type == "windturbine")
            {
                // Wind turbines produce power proportional to the wind percentage.
                return Pmax * (windPercentage / 100.0);
            }

            // Other powerplants produce power constrained by their Pmin and Pmax values.
            return Math.Min(Pmax, Math.Max(Pmin, load));
        }
    }
}