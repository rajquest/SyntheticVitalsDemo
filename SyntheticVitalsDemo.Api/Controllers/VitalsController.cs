using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/patients/{patientId:guid}")]
public sealed class VitalsController(VitalsService vitals) : ControllerBase
{
    [HttpGet("vitals")]
    public async Task<ActionResult<IReadOnlyList<VitalsSubmissionResponse>>> GetForPatient(Guid patientId)
    {
        var results = await vitals.GetForPatientAsync(patientId);
        return results is null ? NotFound() : Ok(results);
    }

    [HttpPost("generate-vitals")]
    public async Task<ActionResult<VitalsSubmissionResponse>> GenerateOne(Guid patientId, GenerateVitalsRequest request)
    {
        var generated = await vitals.GenerateOneAsync(patientId, request);
        return generated is null ? NotFound() : Ok(generated);
    }

    [HttpPost("generate-vitals-series")]
    public async Task<ActionResult<IReadOnlyList<VitalsSubmissionResponse>>> GenerateSeries(Guid patientId, GenerateVitalsSeriesRequest request)
    {
        if (request.Days is < 7 or > 30) return ValidationProblem("Days must be between 7 and 30.");
        if (!Validation.TryParsePulmonaryPressureTrendScenario(request.PulmonaryPressureScenario, out _))
        {
            return ValidationProblem("Pulmonary artery pressure trend is required.");
        }

        var generated = await vitals.GenerateSeriesAsync(patientId, request);
        return generated is null ? NotFound() : Ok(generated);
    }

    [HttpDelete("vitals")]
    public async Task<IActionResult> Delete(Guid patientId) =>
        await vitals.DeleteForPatientAsync(patientId) ? NoContent() : NotFound();
}
