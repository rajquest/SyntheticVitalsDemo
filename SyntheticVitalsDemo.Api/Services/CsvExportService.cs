using System.Text;
using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class CsvExportService(AppDbContext db)
{
    public async Task<string> ExportVitalsAsync()
    {
        var rows = await db.VitalsSubmissions
            .Include(x => x.Patient)!.ThenInclude(x => x!.Clinic)
            .OrderBy(x => x.SubmittedAtUtc)
            .Select(x => new
            {
                Clinic = x.Patient!.Clinic!.Name,
                Patient = x.Patient.FirstName + " " + x.Patient.LastName,
                x.Patient.Scenario,
                x.SubmittedAtUtc,
                x.SystolicBp,
                x.DiastolicBp,
                x.Spo2,
                x.HeartRate,
                x.WeightLbs,
                x.SeatedPaSystolic,
                x.SeatedPaDiastolic,
                x.SeatedPaMean,
                x.SupinePaSystolic,
                x.SupinePaDiastolic,
                x.SupinePaMean,
                x.Notes
            })
            .ToArrayAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Clinic,Patient,Scenario,SubmittedAtUtc,SystolicBp,DiastolicBp,Spo2,HeartRate,WeightLbs,SeatedPaSystolic,SeatedPaDiastolic,SeatedPaMean,SupinePaSystolic,SupinePaDiastolic,SupinePaMean,Notes");
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",", [
                Escape(row.Clinic),
                Escape(row.Patient),
                row.Scenario.ToString(),
                row.SubmittedAtUtc.ToString("O"),
                row.SystolicBp.ToString(),
                row.DiastolicBp.ToString(),
                row.Spo2.ToString(),
                row.HeartRate.ToString(),
                row.WeightLbs.ToString("0.0"),
                row.SeatedPaSystolic.ToString(),
                row.SeatedPaDiastolic.ToString(),
                row.SeatedPaMean.ToString(),
                row.SupinePaSystolic.ToString(),
                row.SupinePaDiastolic.ToString(),
                row.SupinePaMean.ToString(),
                Escape(row.Notes ?? string.Empty)
            ]));
        }

        return csv.ToString();
    }

    private static string Escape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
