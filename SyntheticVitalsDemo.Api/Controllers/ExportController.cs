using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/export")]
public sealed class ExportController(CsvExportService export, Hl7ExportService hl7Export, FhirExportService fhirExport, RhythmFhirExportService rhythmFhirExport) : ControllerBase
{
    [HttpGet("vitals")]
    public async Task<FileContentResult> ExportVitals()
    {
        var csv = await export.ExportVitalsAsync();
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "synthetic-vitals.csv");
    }

    [HttpGet("vitals/{submissionId:guid}/hl7")]
    public async Task<IActionResult> ExportVitalsSubmissionHl7(Guid submissionId)
    {
        var hl7 = await hl7Export.ExportVitalsSubmissionAsync(submissionId);
        return hl7 is null
            ? NotFound()
            : File(System.Text.Encoding.ASCII.GetBytes(hl7), "application/hl7-v2", $"synthetic-vitals-{submissionId:N}.hl7");
    }

    [HttpGet("vitals/{submissionId:guid}/fhir")]
    public async Task<IActionResult> ExportVitalsSubmissionFhir(Guid submissionId)
    {
        var fhir = await fhirExport.ExportVitalsSubmissionAsync(submissionId);
        return fhir is null
            ? NotFound()
            : File(System.Text.Encoding.UTF8.GetBytes(fhir), "application/fhir+json", $"synthetic-vitals-{submissionId:N}.json");
    }

    [HttpGet("vitals/{submissionId:guid}/fhir-rhythm")]
    public async Task<IActionResult> ExportVitalsSubmissionFhirRhythm(Guid submissionId)
    {
        var fhir = await rhythmFhirExport.ExportVitalsSubmissionAsync(submissionId);
        return fhir is null
            ? NotFound()
            : File(System.Text.Encoding.UTF8.GetBytes(fhir), "application/fhir+json", $"rhythm-vitals-{submissionId:N}.json");
    }
}
