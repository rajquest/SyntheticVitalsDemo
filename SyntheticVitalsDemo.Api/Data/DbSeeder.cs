using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Data;

public sealed class DbSeeder(AppDbContext db)
{
    private static readonly (string Name, string? SiteId)[] FallbackClinics =
    [
        ("North Demo Clinic", null),
        ("Central Synthetic Care", null),
        ("Lakeside Training Health", null)
    ];

    public async Task SeedAsync()
    {
        await db.Database.MigrateAsync();
        await SeedClinicsAsync();
        await SeedDevicesAsync();
    }

    private async Task SeedClinicsAsync()
    {
        if (await db.Clinics.AnyAsync()) return;

        var clinics = await LoadClinicsAsync();
        foreach (var (name, siteId) in clinics)
            db.Clinics.Add(new Clinic { Name = name, SiteId = siteId });

        await db.SaveChangesAsync();
    }

    private async Task SeedDevicesAsync()
    {
        if (await db.Devices.AnyAsync()) return;

        var devices = await LoadDevicesAsync();
        db.Devices.AddRange(devices);
        await db.SaveChangesAsync();
    }

    private static async Task<(string Name, string? SiteId)[]> LoadClinicsAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "clinics.csv");
        if (!File.Exists(path)) return FallbackClinics;

        var lines = await File.ReadAllLinesAsync(path);
        var clinics = lines
            .Skip(1)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(line =>
            {
                var comma = line.IndexOf(',');
                if (comma < 0) return (Name: line, SiteId: (string?)null);
                var name = line[..comma].Trim();
                var siteId = line[(comma + 1)..].Trim();
                return (Name: name, SiteId: string.IsNullOrWhiteSpace(siteId) ? null : (string?)siteId);
            })
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return clinics.Length > 0 ? clinics : FallbackClinics;
    }

    private static async Task<Device[]> LoadDevicesAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "devices.csv");
        if (!File.Exists(path)) return [];

        var lines = await File.ReadAllLinesAsync(path);
        return lines
            .Skip(1)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(line =>
            {
                var parts = line.Split(',');
                return new Device
                {
                    DeviceType = parts[0].Trim(),
                    DeviceId   = parts[1].Trim(),
                    ImeiNumber = parts.Length > 2 ? parts[2].Trim() : null
                };
            })
            .DistinctBy(x => new { x.DeviceType, x.DeviceId })
            .ToArray();
    }
}
