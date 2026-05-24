using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/export")]
public sealed class ExportController(CsvExportService export) : ControllerBase
{
    [HttpGet("vitals")]
    public async Task<FileContentResult> ExportVitals()
    {
        var csv = await export.ExportVitalsAsync();
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "synthetic-vitals.csv");
    }
}
