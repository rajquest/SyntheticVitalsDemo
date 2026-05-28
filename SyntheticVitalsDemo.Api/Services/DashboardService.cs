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
                x.SeatedPaSystolic,
                x.SeatedPaDiastolic,
                x.SeatedPaMean,
                x.SupinePaSystolic,
                x.SupinePaDiastolic,
                x.SupinePaMean,
                x.SeatedPaSystolic + " / " + x.SeatedPaDiastolic + " (" + x.SeatedPaMean + ")",
                x.SupinePaSystolic + " / " + x.SupinePaDiastolic + " (" + x.SupinePaMean + ")",
                x.Scenario.ToString()))
            .ToArrayAsync();

        var totalSubmissions = await db.VitalsSubmissions.CountAsync();
        var outOfRangeSubmissions = await db.VitalsSubmissions.CountAsync(x =>
            x.SeatedPaSystolic < PulmonaryPressureGeneratorService.ReferenceSystolicMin || x.SeatedPaSystolic > PulmonaryPressureGeneratorService.ReferenceSystolicMax ||
            x.SeatedPaDiastolic < PulmonaryPressureGeneratorService.ReferenceDiastolicMin || x.SeatedPaDiastolic > PulmonaryPressureGeneratorService.ReferenceDiastolicMax ||
            x.SeatedPaMean < PulmonaryPressureGeneratorService.ReferenceMeanMin || x.SeatedPaMean > PulmonaryPressureGeneratorService.ReferenceMeanMax ||
            x.SupinePaSystolic < PulmonaryPressureGeneratorService.ReferenceSystolicMin || x.SupinePaSystolic > PulmonaryPressureGeneratorService.ReferenceSystolicMax ||
            x.SupinePaDiastolic < PulmonaryPressureGeneratorService.ReferenceDiastolicMin || x.SupinePaDiastolic > PulmonaryPressureGeneratorService.ReferenceDiastolicMax ||
            x.SupinePaMean < PulmonaryPressureGeneratorService.ReferenceMeanMin || x.SupinePaMean > PulmonaryPressureGeneratorService.ReferenceMeanMax);

        return new DashboardSummaryResponse(
            await db.Clinics.CountAsync(),
            await db.Patients.CountAsync(),
            totalSubmissions,
            outOfRangeSubmissions,
            recent);
    }
}
