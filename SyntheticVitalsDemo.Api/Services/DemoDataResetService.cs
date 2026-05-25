using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class DemoDataResetService(AppDbContext db)
{
    public async Task<ResetPatientDataResponse> ResetPatientDataAsync()
    {
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            var deletedVitalsSubmissions = await db.VitalsSubmissions.ExecuteDeleteAsync();
            var deletedPatients = await db.Patients.ExecuteDeleteAsync();

            await transaction.CommitAsync();

            return new ResetPatientDataResponse(deletedVitalsSubmissions, deletedPatients);
        });
    }
}
