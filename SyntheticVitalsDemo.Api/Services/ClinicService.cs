using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class ClinicService(AppDbContext db)
{
    public async Task<IReadOnlyList<ClinicResponse>> GetAllAsync() =>
        await db.Clinics.Include(x => x.Patients).OrderBy(x => x.Name).Select(x =>
            new ClinicResponse(x.Id, x.Name, x.Location, x.CreatedAtUtc, x.Patients.Count)).ToArrayAsync();

    public async Task<ClinicResponse?> GetAsync(Guid id)
    {
        var clinic = await db.Clinics.Include(x => x.Patients).FirstOrDefaultAsync(x => x.Id == id);
        return clinic?.ToResponse();
    }

    public async Task<ClinicResponse> CreateAsync(CreateClinicRequest request)
    {
        var clinic = new Clinic { Name = request.Name.Trim(), Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim() };
        db.Clinics.Add(clinic);
        await db.SaveChangesAsync();
        return clinic.ToResponse();
    }

    public async Task<ClinicResponse?> UpdateAsync(Guid id, UpdateClinicRequest request)
    {
        var clinic = await db.Clinics.Include(x => x.Patients).FirstOrDefaultAsync(x => x.Id == id);
        if (clinic is null) return null;

        clinic.Name = request.Name.Trim();
        clinic.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        await db.SaveChangesAsync();
        return clinic.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var clinic = await db.Clinics.FindAsync(id);
        if (clinic is null) return false;

        db.Clinics.Remove(clinic);
        await db.SaveChangesAsync();
        return true;
    }
}
