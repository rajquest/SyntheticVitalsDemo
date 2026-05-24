using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(DashboardService dashboard) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary() => Ok(await dashboard.GetSummaryAsync());
}
