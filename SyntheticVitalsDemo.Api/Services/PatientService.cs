using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class PatientService(AppDbContext db, SyntheticPatientGeneratorService generator, IVitalsGenerationService vitalsGenerator)
{
    public async Task<IReadOnlyList<PatientResponse>> GetAllAsync() =>
        await db.Patients
            .Include(x => x.VitalsSubmissions)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new PatientResponse(
                x.Id,
                x.ClinicId,
                x.FirstName,
                x.LastName,
                x.DateOfBirth,
                x.Sex.ToString(),
                x.Scenario.ToString(),
                x.SystolicBp,
                x.DiastolicBp,
                x.SystolicBp + " / " + x.DiastolicBp,
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
                x.CreatedAtUtc,
                x.VitalsSubmissions.Count))
            .ToArrayAsync();

    public async Task<IReadOnlyList<PatientResponse>?> GetForClinicAsync(Guid clinicId)
    {
        if (!await db.Clinics.AnyAsync(x => x.Id == clinicId)) return null;

        return await db.Patients
            .Where(x => x.ClinicId == clinicId)
            .Include(x => x.VitalsSubmissions)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new PatientResponse(
                x.Id,
                x.ClinicId,
                x.FirstName,
                x.LastName,
                x.DateOfBirth,
                x.Sex.ToString(),
                x.Scenario.ToString(),
                x.SystolicBp,
                x.DiastolicBp,
                x.SystolicBp + " / " + x.DiastolicBp,
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
                x.CreatedAtUtc,
                x.VitalsSubmissions.Count))
            .ToArrayAsync();
    }

    public async Task<PatientResponse?> GetAsync(Guid id)
    {
        var patient = await db.Patients.Include(x => x.VitalsSubmissions).FirstOrDefaultAsync(x => x.Id == id);
        return patient?.ToResponse();
    }

    public async Task<PatientResponse?> CreateAsync(Guid clinicId, CreatePatientRequest request)
    {
        if (!await db.Clinics.AnyAsync(x => x.Id == clinicId)) return null;
        Validation.TryParseSex(request.Sex, out var sex);
        Validation.TryParseScenario(request.Scenario, out var scenario);

        var patient = new Patient
        {
            ClinicId = clinicId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Sex = sex,
            Scenario = scenario
        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        return patient.ToResponse();
    }

    public async Task<GeneratePatientsResponse?> GenerateAsync(Guid clinicId, GeneratePatientsRequest request)
    {
        if (!await db.Clinics.AnyAsync(x => x.Id == clinicId)) return null;
        var patientScenario = ResolvePatientScenario(request);
        Validation.TryParsePulmonaryPressureTrendScenario(request.PulmonaryPressureTrendScenario, out var trendScenario);
        var trendDays = request.TrendDays;

        var patients = generator.Generate(clinicId, request, patientScenario);
        var vitalsSubmissions = patients
            .SelectMany(patient =>
            {
                var series = vitalsGenerator.GenerateSeries(patient, trendDays, DateTime.UtcNow, trendScenario);
                var latest = series[series.Count - 1];
                patient.SystolicBp = latest.SystolicBp;
                patient.DiastolicBp = latest.DiastolicBp;
                patient.Spo2 = latest.Spo2;
                patient.HeartRate = latest.HeartRate;
                patient.WeightLbs = latest.WeightLbs;
                patient.SeatedPaSystolic = latest.SeatedPaSystolic;
                patient.SeatedPaDiastolic = latest.SeatedPaDiastolic;
                patient.SeatedPaMean = latest.SeatedPaMean;
                patient.SupinePaSystolic = latest.SupinePaSystolic;
                patient.SupinePaDiastolic = latest.SupinePaDiastolic;
                patient.SupinePaMean = latest.SupinePaMean;
                return series;
            })
            .ToArray();

        db.Patients.AddRange(patients);
        db.VitalsSubmissions.AddRange(vitalsSubmissions);
        await db.SaveChangesAsync();
        var updatedPatientCount = await db.Patients.CountAsync(x => x.ClinicId == clinicId);

        return new GeneratePatientsResponse(
            clinicId,
            patients.Count,
            updatedPatientCount,
            patients.Select(x => x.ToResponse()).ToArray());
    }

    private static PatientScenario ResolvePatientScenario(GeneratePatientsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Scenario) &&
            Validation.TryParseScenario(request.Scenario, out var parsedScenario))
        {
            return parsedScenario;
        }

        if (Validation.TryParseScenario(request.PulmonaryPressureScenario, out var parsed) &&
            Validation.PulmonaryPressureScenarios.Contains(parsed))
        {
            return parsed;
        }

        return PatientScenario.NormalPaPressure;
    }

    public async Task<PatientResponse?> UpdateAsync(Guid id, UpdatePatientRequest request)
    {
        var patient = await db.Patients.Include(x => x.VitalsSubmissions).FirstOrDefaultAsync(x => x.Id == id);
        if (patient is null) return null;

        Validation.TryParseSex(request.Sex, out var sex);
        Validation.TryParseScenario(request.Scenario, out var scenario);
        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Sex = sex;
        patient.Scenario = scenario;
        await db.SaveChangesAsync();
        return patient.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var patient = await db.Patients.FindAsync(id);
        if (patient is null) return false;

        db.Patients.Remove(patient);
        await db.SaveChangesAsync();
        return true;
    }

}
