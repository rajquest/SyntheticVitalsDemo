using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
public sealed class PatientsController(PatientService patients) : ControllerBase
{
    [HttpGet("api/clinics/{clinicId:guid}/patients")]
    public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetForClinic(Guid clinicId)
    {
        var results = await patients.GetForClinicAsync(clinicId);
        return results is null ? NotFound() : Ok(results);
    }

    [HttpGet("api/patients/{patientId:guid}")]
    public async Task<ActionResult<PatientResponse>> Get(Guid patientId)
    {
        var patient = await patients.GetAsync(patientId);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost("api/clinics/{clinicId:guid}/patients")]
    public async Task<ActionResult<PatientResponse>> Create(Guid clinicId, CreatePatientRequest request)
    {
        var error = Validation.ValidatePatient(request.FirstName, request.LastName, request.DateOfBirth, request.Sex, request.Scenario);
        if (error is not null) return ValidationProblem(error);

        var patient = await patients.CreateAsync(clinicId, request);
        return patient is null ? NotFound() : CreatedAtAction(nameof(Get), new { patientId = patient.Id }, patient);
    }

    [HttpPost("api/clinics/{clinicId:guid}/patients/generate")]
    public async Task<ActionResult<GeneratePatientsResponse>> Generate(Guid clinicId, GeneratePatientsRequest request)
    {
        if (clinicId == Guid.Empty) return ValidationProblem("Clinic is required.");
        if (request.Count is not (5 or 10 or 25 or 50 or 100)) return ValidationProblem("Count must be 5, 10, 25, 50, or 100.");
        if (request.MalePercentage is < 0 or > 100) return ValidationProblem("Male percentage must be between 0 and 100.");
        if (!Validation.TryParsePulmonaryPressureTrendScenario(request.PulmonaryPressureScenario, out _))
        {
            return ValidationProblem($"Pulmonary artery pressure trend must be one of: {string.Join(", ", Enum.GetNames<PulmonaryPressureTrendScenario>())}.");
        }
        if (request.TrendDays is < 7 or > 30) return ValidationProblem("Trend days must be between 7 and 30.");

        var generated = await patients.GenerateAsync(clinicId, request);
        return generated is null ? NotFound() : Ok(generated);
    }

    [HttpPut("api/patients/{patientId:guid}")]
    public async Task<ActionResult<PatientResponse>> Update(Guid patientId, UpdatePatientRequest request)
    {
        var error = Validation.ValidatePatient(request.FirstName, request.LastName, request.DateOfBirth, request.Sex, request.Scenario);
        if (error is not null) return ValidationProblem(error);

        var patient = await patients.UpdateAsync(patientId, request);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpDelete("api/patients/{patientId:guid}")]
    public async Task<IActionResult> Delete(Guid patientId) =>
        await patients.DeleteAsync(patientId) ? NoContent() : NotFound();
}
