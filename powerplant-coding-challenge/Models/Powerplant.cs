namespace powerplant_coding_challenge.Models
{
    public class Powerplant
    {
        public string Name { get; set; } = string.Empty;
        public PowerplantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Pmin { get; set; }
        public decimal Pmax { get; set; }

        public decimal CalculateProduction(decimal load, decimal windPercentage)
        {
            decimal production;

            if (Type == PowerplantType.windturbine)
            {
                // Calculate wind turbine production based on wind percentage
                decimal windFactor = windPercentage / 100.0m;
                production = Pmax * windFactor;

                // Adjust if the remaining load is less than the calculated production
                if (production > load)
                {
                    production = load;
                }
            }
            else
            {
                // For thermal plants, ensure production is within Pmin and Pmax
                if (load < Pmin)
                {
                    production = 0; // Skip if remaining load is less than Pmin
                }
                else
                {
                    production = Math.Min(Pmax, load); // Produce as much as needed but no more than Pmax
                }
            }

            return production;
        }

        public decimal CalculateCostPerMWh(Fuels fuels)
        {
            return Type switch
            {
                PowerplantType.gasfired => (fuels.Gas / Efficiency) + (0.3m * fuels.Co2),
                PowerplantType.turbojet => fuels.Kerosine / Efficiency,
                PowerplantType.windturbine => 0m,
                _ => throw new NotImplementedException($"Cost calculation for {Type} is not implemented.")
            };
        }
    }
}
