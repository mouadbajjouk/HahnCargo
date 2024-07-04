using CargoSim.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CargoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SimController(SimService simService) : ControllerBase
{
    [HttpGet("")]
    public async Task Get()
    {
        await simService.Func(true);
    }
}
