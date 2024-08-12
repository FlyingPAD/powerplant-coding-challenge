using MediatR;
using Microsoft.AspNetCore.Mvc;
using powerplant_coding_challenge.ProductionPlan;

namespace PowerplantCodingChallenge.Controllers;

[ApiController]
[Route("productionplan")]
public class ProductionPlanController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductionPlanCommand command)
    => Ok(await _mediator.Send(command));
}