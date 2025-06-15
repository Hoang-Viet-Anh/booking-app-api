using booking_api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace booking_api.Controllers;

[ApiController]
[Route("[controller]")]
public class Coworkings : ControllerBase
{
    private readonly ICoworkingsService _coworkingsService;

    public Coworkings(ICoworkingsService coworkingsService)
    {
        _coworkingsService = coworkingsService;
    }

    /// <summary>
    /// Gets all coworking
    /// </summary>
    /// <returns>Array of Coworking</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _coworkingsService.GetAllCoworkingsAsync());
    }
}