using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class VitalsService(AppDbContext db, IVitalsGenerationService generator)
{
    public async Task<IReadOnlyList<VitalsSubmissionResponse>?> GetForPatientAsync(Guid patientId)
    {
        if (!await db.Patients.AnyAsync(x => x.Id == patientId)) return null;

        return await db.VitalsSubmissions
            .Where(x => x.PatientId == patientId)
            .OrderBy(x => x.SubmittedAtUtc)
            .Select(x => new VitalsSubmissionResponse(x.Id, x.PatientId, x.SubmittedAtUtc, x.SystolicBp, x.DiastolicBp, x.Spo2, x.HeartRate, x.WeightLbs, x.PaSystolic, x.PaDiastolic, x.PaMean, x.Scenario.ToString(), x.Notes))
            .ToArrayAsync();
    }

    public async Task<VitalsSubmissionResponse?> GenerateOneAsync(Guid patientId, GenerateVitalsRequest request)
    {
        var patient = await db.Patients.FindAsync(patientId);
        if (patient is null) return null;

        var vitals = generator.Generate(patient, request.SubmittedAtUtc ?? DateTime.UtcNow);
        db.VitalsSubmissions.Add(vitals);
        await db.SaveChangesAsync();
        return vitals.ToResponse();
    }

    public async Task<IReadOnlyList<VitalsSubmissionResponse>?> GenerateSeriesAsync(Guid patientId, GenerateVitalsSeriesRequest request)
    {
        var patient = await db.Patients.FindAsync(patientId);
        if (patient is null) return null;
        if (request.Days is not (1 or 7 or 30 or 90)) throw new ArgumentOutOfRangeException(nameof(request.Days), "Days must be 1, 7, 30, or 90.");

        if (request.ReplaceExisting)
        {
            await db.VitalsSubmissions.Where(x => x.PatientId == patientId).ExecuteDeleteAsync();
        }

        var vitals = generator.GenerateSeries(patient, request.Days, request.EndDateUtc ?? DateTime.UtcNow);
        db.VitalsSubmissions.AddRange(vitals);
        await db.SaveChangesAsync();
        return vitals.Select(x => x.ToResponse()).ToArray();
    }

    public async Task<bool> DeleteForPatientAsync(Guid patientId)
    {
        if (!await db.Patients.AnyAsync(x => x.Id == patientId)) return false;

        await db.VitalsSubmissions.Where(x => x.PatientId == patientId).ExecuteDeleteAsync();
        return true;
    }
}
