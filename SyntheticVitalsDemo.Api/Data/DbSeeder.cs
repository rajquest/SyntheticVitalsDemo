using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Data;

public sealed class DbSeeder(AppDbContext db)
{
    private static readonly string[] FallbackClinicNames = ["North Demo Clinic", "Central Synthetic Care", "Lakeside Training Health"];

    public async Task SeedAsync()
    {
        await db.Database.MigrateAsync();
        if (await db.Clinics.AnyAsync()) return;

        var clinicNames = await LoadClinicNamesAsync();

        for (var clinicIndex = 0; clinicIndex < clinicNames.Length; clinicIndex++)
        {
            var clinic = new Clinic
            {
                Name = clinicNames[clinicIndex]
            };

            db.Clinics.Add(clinic);
        }

        await db.SaveChangesAsync();
    }

    private static async Task<string[]> LoadClinicNamesAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SampleData", "clinics.csv");
        if (!File.Exists(path)) return FallbackClinicNames;

        var lines = await File.ReadAllLinesAsync(path);
        var names = lines
            .Skip(1)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return names.Length > 0 ? names : FallbackClinicNames;
    }
}
