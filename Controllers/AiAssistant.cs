using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace booking_api.Controllers;

[ApiController]
[Route("[controller]")]
public class AiAssistant : ControllerBase
{
    private readonly IAiService _aiService;

    public AiAssistant(IAiService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Ask AI question
    /// </summary>
    /// <returns>AI response</returns>
    [HttpPost]
    public async Task<IActionResult> Get([FromBody] PromptDto promptDto)
    {
        return Ok(new
        {
            Response = await _aiService.AskGroqAsync(promptDto.Prompt)
        });
    }
}