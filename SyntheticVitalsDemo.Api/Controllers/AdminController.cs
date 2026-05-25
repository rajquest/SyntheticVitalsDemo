using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdminController(DemoDataResetService demoDataReset) : ControllerBase
{
    [HttpDelete("patient-data")]
    public async Task<ActionResult<ResetPatientDataResponse>> ResetPatientData() =>
        Ok(await demoDataReset.ResetPatientDataAsync());
}
