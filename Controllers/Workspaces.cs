using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace booking_api.Controllers;

[ApiController]
[Route("[controller]")]
public class Workspaces : ControllerBase
{
    private readonly IWorkspacesService _workspacesService;

    public Workspaces(IWorkspacesService workspacesService)
    {
        _workspacesService = workspacesService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _workspacesService.GetAllWorkspacesAsync());
    }
}