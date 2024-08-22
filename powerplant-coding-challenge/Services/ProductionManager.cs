using powerplant_coding_challenge.Features;

namespace powerplant_coding_challenge.Services
{
    public class ProductionManager
    {
        public static void AdjustProductionToMatchLoad(List<ProductionPlanCommandResponse> responseList, decimal remainingLoad)
        {
            if (remainingLoad == 0) return;

            foreach (var response in responseList.OrderByDescending(response => response.Power))
            {
                if (remainingLoad == 0) break;

                decimal adjustment = Math.Min(Math.Abs(remainingLoad), response.Power);

                if (remainingLoad > 0)
                {
                    response.Power += adjustment;
                    remainingLoad -= adjustment;
                }
                else
                {
                    response.Power -= adjustment;
                    remainingLoad += adjustment;
                }
            }
        }
    }
}