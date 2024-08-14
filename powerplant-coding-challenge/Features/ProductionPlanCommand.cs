using MediatR;
using powerplant_coding_challenge.Models;

namespace powerplant_coding_challenge.Features;

public class ProductionPlanCommand : IRequest<List<ProductionPlanCommandResponse>>
{
    public double Load { get; set; }
    public List<Powerplant> Powerplants { get; set; } = [];
    public Fuels Fuels { get; set; } = new();
}