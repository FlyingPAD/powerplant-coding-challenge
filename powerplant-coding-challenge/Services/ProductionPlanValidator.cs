using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services
{
    public class ProductionPlanValidator
    {
        public static void ValidateTotalCapacity(List<Powerplant> powerplants, decimal load)
        {
            decimal totalCapacity = powerplants.Sum(p => p.Pmax);
            if (load > totalCapacity)
            {
                throw new InvalidOperationException("La charge demandée dépasse la capacité totale des centrales disponibles.");
            }
        }

        public static void ValidateLoadAgainstPmin(List<Powerplant> powerplants, decimal load)
        {
            bool isLoadBelowAllPmin = powerplants
                .Where(p => p.Type != PowerplantType.windturbine)
                .All(p => load < p.Pmin);

            if (isLoadBelowAllPmin)
            {
                throw new InvalidOperationException("La charge demandée est inférieure au Pmin de chaque centrale non éolienne.");
            }
        }
    }
}