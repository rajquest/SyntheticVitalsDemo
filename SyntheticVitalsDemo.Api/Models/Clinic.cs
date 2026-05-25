namespace SyntheticVitalsDemo.Api.Models;

public sealed class Clinic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<Patient> Patients { get; set; } = [];
}
