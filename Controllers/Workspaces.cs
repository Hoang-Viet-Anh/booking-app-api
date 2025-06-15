using booking_api.Interfaces;
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

    /// <summary>
    /// Gets all workspaces
    /// </summary>
    /// <returns>Array of Workspace</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _workspacesService.GetAllWorkspacesAsync());
    }
}