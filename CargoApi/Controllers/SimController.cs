using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CargoApi.Controllers;

[Route("api/simulation")]
[ApiController]
public class SimController(ISimService simService, IStateService stateService) : ControllerBase
{
    [HttpGet("graph")]
    public IActionResult GetGraph()
    {
        return Ok(simService.GetGraph());
    }

    [HttpGet("cargo")]
    public async Task<IActionResult> GetCargo()
    {
        return Ok(await simService.GetCargo());
    }

    [HttpGet("orders/create")]
    public async Task CreateOrders()
    {
        await simService.CreateOrders();
    }

    [HttpGet("move")]
    public async Task Move()
    {
        if (stateService.IsFirstTime)
        {
            await simService.Move(stateService.IsFirstTime);

            stateService.IsFirstTime = false;

            return;
        }

        await simService.Move(stateService.IsFirstTime);
    }

    [HttpGet("start")]
    public async Task Start()
    {
        await simService.Start();
    }
}
