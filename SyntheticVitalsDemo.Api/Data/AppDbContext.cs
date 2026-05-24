using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalsSubmission> VitalsSubmissions => Set<VitalsSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(160);
            entity.HasMany(x => x.Patients).WithOne(x => x.Clinic).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Sex).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.Scenario).HasConversion<string>().HasMaxLength(48).IsRequired();
            entity.HasMany(x => x.VitalsSubmissions).WithOne(x => x.Patient).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VitalsSubmission>(entity =>
        {
            entity.Property(x => x.WeightLbs).HasPrecision(6, 1);
            entity.Property(x => x.Scenario).HasConversion<string>().HasMaxLength(48).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(400);
            entity.HasIndex(x => new { x.PatientId, x.SubmittedAtUtc });
        });
    }
}
