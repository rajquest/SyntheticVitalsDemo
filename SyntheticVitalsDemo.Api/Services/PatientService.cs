using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class PatientService(AppDbContext db)
{
    private static readonly string[] FirstNames = ["Avery", "Jordan", "Morgan", "Taylor", "Riley", "Casey", "Jamie", "Quinn", "Drew", "Reese"];
    private static readonly string[] LastNames = ["Demo", "Sample", "Training", "Synthetic", "Example", "Mock", "Sandbox", "Practice", "Fiction", "Placeholder"];

    public async Task<IReadOnlyList<PatientResponse>?> GetForClinicAsync(Guid clinicId)
    {
        if (!await db.Clinics.AnyAsync(x => x.Id == clinicId)) return null;

        return await db.Patients
            .Where(x => x.ClinicId == clinicId)
            .Include(x => x.VitalsSubmissions)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new PatientResponse(x.Id, x.ClinicId, x.FirstName, x.LastName, x.DateOfBirth, x.Sex.ToString(), x.Scenario.ToString(), x.CreatedAtUtc, x.VitalsSubmissions.Count))
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

    public async Task<IReadOnlyList<PatientResponse>?> GenerateAsync(Guid clinicId, GeneratePatientsRequest request)
    {
        if (!await db.Clinics.AnyAsync(x => x.Id == clinicId)) return null;

        var count = Math.Clamp(request.Count, 1, 100);
        var scenarios = ParseScenarios(request.Scenarios);
        var patients = Enumerable.Range(0, count).Select(i => new Patient
        {
            ClinicId = clinicId,
            FirstName = FirstNames[i % FirstNames.Length],
            LastName = $"{LastNames[i % LastNames.Length]}{i + 1}",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-35 - i % 45).AddDays(-i * 17)),
            Sex = i % 3 == 0 ? Sex.Female : i % 3 == 1 ? Sex.Male : Sex.Other,
            Scenario = scenarios[i % scenarios.Length]
        }).ToArray();

        db.Patients.AddRange(patients);
        await db.SaveChangesAsync();
        return patients.Select(x => x.ToResponse()).ToArray();
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

    private static PatientScenario[] ParseScenarios(string[]? values)
    {
        var parsed = values?
            .Where(x => Validation.TryParseScenario(x, out _))
            .Select(x =>
            {
                Validation.TryParseScenario(x, out var scenario);
                return scenario;
            })
            .Distinct()
            .ToArray();

        return parsed is { Length: > 0 } ? parsed : Enum.GetValues<PatientScenario>();
    }
}
