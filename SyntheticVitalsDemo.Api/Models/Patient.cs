namespace SyntheticVitalsDemo.Api.Models;

public sealed class Patient
{
    public Guid PatientGuid { get; set; } = Guid.CreateVersion7();
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
    public int SeatedPaSystolic { get; set; }
    public int SeatedPaDiastolic { get; set; }
    public int SeatedPaMean { get; set; }
    public int SupinePaSystolic { get; set; }
    public int SupinePaDiastolic { get; set; }
    public int SupinePaMean { get; set; }
    public string BloodPressureDisplay => $"{SystolicBp} / {DiastolicBp}";
    public string SeatedPulmonaryPressureDisplay => $"{SeatedPaSystolic} / {SeatedPaDiastolic} ({SeatedPaMean})";
    public string SupinePulmonaryPressureDisplay => $"{SupinePaSystolic} / {SupinePaDiastolic} ({SupinePaMean})";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<VitalsSubmission> VitalsSubmissions { get; set; } = [];
}
