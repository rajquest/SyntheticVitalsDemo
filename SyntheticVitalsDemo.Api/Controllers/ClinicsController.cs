using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/clinics")]
public sealed class ClinicsController(ClinicService clinics) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClinicResponse>>> GetAll() => Ok(await clinics.GetAllAsync());

    [HttpGet("{clinicId:guid}")]
    public async Task<ActionResult<ClinicResponse>> Get(Guid clinicId)
    {
        var clinic = await clinics.GetAsync(clinicId);
        return clinic is null ? NotFound() : Ok(clinic);
    }

    [HttpPost]
    public async Task<ActionResult<ClinicResponse>> Create(CreateClinicRequest request)
    {
        var error = Validation.ValidateClinic(request.Name);
        if (error is not null) return ValidationProblem(error);

        var clinic = await clinics.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { clinicId = clinic.Id }, clinic);
    }

    [HttpPut("{clinicId:guid}")]
    public async Task<ActionResult<ClinicResponse>> Update(Guid clinicId, UpdateClinicRequest request)
    {
        var error = Validation.ValidateClinic(request.Name);
        if (error is not null) return ValidationProblem(error);

        var clinic = await clinics.UpdateAsync(clinicId, request);
        return clinic is null ? NotFound() : Ok(clinic);
    }

    [HttpDelete("{clinicId:guid}")]
    public async Task<IActionResult> Delete(Guid clinicId) =>
        await clinics.DeleteAsync(clinicId) ? NoContent() : NotFound();
}
