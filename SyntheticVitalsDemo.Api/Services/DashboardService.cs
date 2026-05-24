using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class DashboardService(AppDbContext db)
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var recent = await db.VitalsSubmissions
            .Include(x => x.Patient)!.ThenInclude(x => x!.Clinic)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .Take(10)
            .Select(x => new RecentVitalsSubmissionResponse(
                x.Id,
                x.PatientId,
                x.Patient!.FirstName + " " + x.Patient.LastName,
                x.Patient.Clinic!.Name,
                x.SubmittedAtUtc,
                x.SystolicBp,
                x.DiastolicBp,
                x.Spo2,
                x.HeartRate,
                x.WeightLbs,
                x.PaSystolic,
                x.PaDiastolic,
                x.PaMean,
                x.Scenario.ToString()))
            .ToArrayAsync();

        var vitals = await db.VitalsSubmissions
            .Select(x => new { x.SystolicBp, x.DiastolicBp, x.Spo2, x.HeartRate, x.PaMean })
            .ToArrayAsync();

        return new DashboardSummaryResponse(
            await db.Clinics.CountAsync(),
            await db.Patients.CountAsync(),
            vitals.Length,
            vitals.Count(x => Validation.IsAbnormal(x.SystolicBp, x.DiastolicBp, x.Spo2, x.HeartRate, x.PaMean)),
            recent);
    }
}
