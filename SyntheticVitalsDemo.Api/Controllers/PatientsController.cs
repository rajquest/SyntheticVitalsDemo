using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
public sealed class PatientsController(PatientService patients) : ControllerBase
{
    [HttpGet("api/patients")]
    public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetAll() =>
        Ok(await patients.GetAllAsync());

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
        return patient is null ? NotFound() : CreatedAtAction(nameof(Get), new { patientId = patient.PatientGuid }, patient);
    }

    [HttpPost("api/clinics/{clinicId:guid}/patients/generate")]
    public async Task<ActionResult<GeneratePatientsResponse>> Generate(Guid clinicId, GeneratePatientsRequest request)
    {
        if (clinicId == Guid.Empty) return ValidationProblem("Clinic is required.");
        if (request.Count is not (1 or 5 or 10 or 25 or 50 or 100)) return ValidationProblem("Count must be 1, 5, 10, 25, 50, or 100.");
        if (request.MalePercentage is < 0 or > 100) return ValidationProblem("Male percentage must be between 0 and 100.");
        if (!Validation.TryParseVitalsTrendScenario(request.VitalsTrendScenario, out _))
        {
            return ValidationProblem($"Vitals trend scenario must be one of: {string.Join(", ", Enum.GetNames<VitalsTrendScenario>())}.");
        }
        if (request.TrendDays is not (1 or 7 or 14 or 30 or 60 or 180 or 365)) return ValidationProblem("Trend readings must be 1, 7, 14, 30, 60, 180, or 365.");

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
