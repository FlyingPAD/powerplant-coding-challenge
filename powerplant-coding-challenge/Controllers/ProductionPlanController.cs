using Microsoft.AspNetCore.Mvc;

namespace PowerplantCodingChallenge.Controllers;

[ApiController]
[Route("productionplan")]
public class ProductionPlanController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("coucou");
    }
}
