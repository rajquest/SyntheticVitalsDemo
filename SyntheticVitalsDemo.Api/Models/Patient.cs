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
    public int SystolicBp { get; set; }
    public int DiastolicBp { get; set; }
    public int Spo2 { get; set; }
    public int HeartRate { get; set; }
    public decimal WeightLbs { get; set; }
    public int PaSystolic { get; set; }
    public int PaDiastolic { get; set; }
    public int PaMean { get; set; }
    public string BloodPressureDisplay => $"{SystolicBp} / {DiastolicBp}";
    public string PulmonaryPressureDisplay => $"{PaSystolic} / {PaDiastolic} ({PaMean})";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<VitalsSubmission> VitalsSubmissions { get; set; } = [];
}
