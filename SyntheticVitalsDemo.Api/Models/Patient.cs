namespace SyntheticVitalsDemo.Api.Models;

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Sex Sex { get; set; }
    public PatientScenario Scenario { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<VitalsSubmission> VitalsSubmissions { get; set; } = [];
}
