using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class DashboardService(AppDbContext db)
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var recent = await db.VitalsSubmissions
            .Include(x => x.Patient)
            .ThenInclude(x => x!.Clinic)
            .OrderByDescending(x => x.SubmittedAtUtc)
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
                x.PaSystolic + " / " + x.PaDiastolic + " (" + x.PaMean + ")",
                x.Scenario.ToString()))
            .ToArrayAsync();

        var totalSubmissions = await db.VitalsSubmissions.CountAsync();
        var outOfRangeSubmissions = await db.VitalsSubmissions.CountAsync(x =>
            x.PaSystolic < 15 || x.PaSystolic > 30 ||
            x.PaDiastolic < 4 || x.PaDiastolic > 12 ||
            x.PaMean < 9 || x.PaMean > 20);

        return new DashboardSummaryResponse(
            await db.Clinics.CountAsync(),
            await db.Patients.CountAsync(),
            totalSubmissions,
            outOfRangeSubmissions,
            recent);
    }
}
