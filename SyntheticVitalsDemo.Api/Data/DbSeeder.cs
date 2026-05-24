using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Models;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Data;

public sealed class DbSeeder(AppDbContext db, IVitalsGenerationService generator)
{
    private static readonly string[] ClinicNames = ["North Demo Clinic", "Central Synthetic Care", "Lakeside Training Health"];
    private static readonly string[] Locations = ["Chicago, IL", "Madison, WI", "Grand Rapids, MI"];
    private static readonly PatientScenario[] ScenarioMix = Enum.GetValues<PatientScenario>();

    public async Task SeedAsync()
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Clinics.AnyAsync()) return;

        for (var clinicIndex = 0; clinicIndex < 3; clinicIndex++)
        {
            var clinic = new Clinic
            {
                Name = ClinicNames[clinicIndex],
                Location = Locations[clinicIndex]
            };

            for (var patientIndex = 0; patientIndex < 10; patientIndex++)
            {
                var scenario = ScenarioMix[(clinicIndex * 10 + patientIndex) % ScenarioMix.Length];
                var patient = new Patient
                {
                    FirstName = $"Demo{clinicIndex + 1}{patientIndex + 1}",
                    LastName = "Patient",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-40 - patientIndex).AddDays(-clinicIndex * 23)),
                    Sex = patientIndex % 3 == 0 ? Sex.Female : patientIndex % 3 == 1 ? Sex.Male : Sex.Other,
                    Scenario = scenario
                };

                if (patientIndex < 6)
                {
                    foreach (var vitals in generator.GenerateSeries(patient, 30, DateTime.UtcNow.Date.AddHours(8)))
                    {
                        patient.VitalsSubmissions.Add(vitals);
                    }
                }

                clinic.Patients.Add(patient);
            }

            db.Clinics.Add(clinic);
        }

        await db.SaveChangesAsync();
    }
}
