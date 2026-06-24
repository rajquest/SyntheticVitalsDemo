using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalsSubmission> VitalsSubmissions => Set<VitalsSubmission>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.SiteId).HasMaxLength(20);
            entity.HasMany(x => x.Patients).WithOne(x => x.Clinic).HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.PatientGuid);
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Sex).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.Scenario).HasConversion<string>().HasMaxLength(48).IsRequired();
            entity.Property(x => x.WeightLbs).HasPrecision(6, 1);
            entity.HasMany(x => x.VitalsSubmissions).WithOne(x => x.Patient).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VitalsSubmission>(entity =>
        {
            entity.Property(x => x.WeightLbs).HasPrecision(6, 1);
            entity.Property(x => x.Scenario).HasConversion<string>().HasMaxLength(48).IsRequired();
            entity.Property(x => x.TrendScenario).HasConversion<string>().HasMaxLength(64).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(400);
            entity.HasIndex(x => new { x.PatientId, x.SubmittedAtUtc });
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("device");
            entity.HasKey(x => new { x.DeviceType, x.DeviceId });
            entity.Property(x => x.DeviceType).HasColumnName("device_type").HasMaxLength(100).IsRequired();
            entity.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(150).IsRequired();
            entity.Property(x => x.ImeiNumber).HasColumnName("imei_number").HasMaxLength(50);
            entity.Property(x => x.BluetoothAddress).HasColumnName("bluetooth_address").HasMaxLength(50);
            entity.Property(x => x.DateTimeCreated).HasColumnName("date_time_created");
            entity.Property(x => x.DateTimeLastUpdated).HasColumnName("date_time_last_updated");
            entity.Property(x => x.DateTimeDeactivated).HasColumnName("date_time_deactivated");
            entity.Property(x => x.DateTimePatientAssigned).HasColumnName("date_time_patient_assigned");
            entity.Property(x => x.PatientGuid).HasColumnName("patient_guid");
            entity.HasIndex(x => x.ImeiNumber).HasDatabaseName("idx_imei");
            entity.HasIndex(x => x.PatientGuid).HasDatabaseName("idx_patient");
        });
    }
}
