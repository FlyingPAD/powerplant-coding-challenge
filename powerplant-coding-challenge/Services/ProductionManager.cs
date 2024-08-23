using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Services
{
    public class ProductionManager
    {
        public void EnsureTotalProductionMatchesLoad(List<ProductionPlanCommandResponse> responseList, decimal totalLoad, List<Powerplant> powerplants)
        {
            decimal totalProduction = responseList.Sum(r => r.Power);

            if (totalProduction > totalLoad)
            {
                foreach (var response in responseList.OrderByDescending(r => r.Power))
                {
                    decimal overProduction = totalProduction - totalLoad;
                    if (overProduction <= 0) break;

                    decimal adjustment = Math.Min(overProduction, response.Power);

                    response.Power -= adjustment;
                    totalProduction -= adjustment;
                }
            }
            else if (totalProduction < totalLoad)
            {
                foreach (var response in responseList.OrderBy(r => r.Power))
                {
                    decimal underProduction = totalLoad - totalProduction;
                    if (underProduction <= 0) break;

                    var correspondingPowerplant = powerplants.FirstOrDefault(p => p.Name == response.Name);
                    if (correspondingPowerplant != null)
                    {
                        decimal adjustment = Math.Min(underProduction, correspondingPowerplant.Pmax - response.Power);

                        response.Power += adjustment;
                        totalProduction += adjustment;
                    }
                }
            }

            if (totalProduction != totalLoad)
            {
                decimal discrepancy = totalLoad - totalProduction;
                if (discrepancy > 0)
                {
                    var adjustableResponses = responseList.Where(r => r.Power > 0 && r.Power < powerplants.First(p => p.Name == r.Name).Pmax).OrderBy(r => r.Power).ToList();
                    foreach (var response in adjustableResponses)
                    {
                        if (totalProduction >= totalLoad) break;

                        var correspondingPowerplant = powerplants.FirstOrDefault(p => p.Name == response.Name);
                        if (correspondingPowerplant != null)
                        {
                            decimal adjustment = Math.Min(discrepancy, correspondingPowerplant.Pmax - response.Power);
                            response.Power += adjustment;
                            totalProduction += adjustment;
                            discrepancy -= adjustment;
                        }
                    }
                }
                else if (discrepancy < 0)
                {
                    var adjustableResponses = responseList.Where(r => r.Power > 0).OrderByDescending(r => r.Power).ToList();
                    foreach (var response in adjustableResponses)
                    {
                        if (totalProduction <= totalLoad) break;

                        decimal adjustment = Math.Min(Math.Abs(discrepancy), response.Power);
                        response.Power -= adjustment;
                        totalProduction -= adjustment;
                        discrepancy += adjustment;
                    }
                }
            }
        }
    }
}