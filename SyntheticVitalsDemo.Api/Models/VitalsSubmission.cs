namespace SyntheticVitalsDemo.Api.Models;

public sealed class VitalsSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public int SystolicBp { get; set; }
    public int DiastolicBp { get; set; }
    public int Spo2 { get; set; }
    public int HeartRate { get; set; }
    public decimal WeightLbs { get; set; }
    public int PaSystolic { get; set; }
    public int PaDiastolic { get; set; }
    public int PaMean { get; set; }
    public PatientScenario Scenario { get; set; }
    public string? Notes { get; set; }
}
