using LoggingService.Models;
using LoggingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoggingService.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "Admin")]
public class LogsController : ControllerBase
{
    private const int MaxWindow = 10_000;

    private readonly IApiLogStore _store;

    public LogsController(IApiLogStore store) => _store = store;

    [HttpGet]
    public async Task<ActionResult<LogsPage>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken ct = default)
    {
        size = Math.Clamp(size, 1, 200);
        if (page < 1) page = 1;

        if (page * size > MaxWindow)
            return BadRequest("Only the latest 10,000 logs can be browsed.");

        return Ok(await _store.GetLogsAsync(page, size, ct));
    }
}