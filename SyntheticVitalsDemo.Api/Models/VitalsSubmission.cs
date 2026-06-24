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
    public int SeatedPaSystolic { get; set; }
    public int SeatedPaDiastolic { get; set; }
    public int SeatedPaMean { get; set; }
    public int SupinePaSystolic { get; set; }
    public int SupinePaDiastolic { get; set; }
    public int SupinePaMean { get; set; }
    public string SeatedPulmonaryPressureDisplay => $"{SeatedPaSystolic} / {SeatedPaDiastolic} ({SeatedPaMean})";
    public string SupinePulmonaryPressureDisplay => $"{SupinePaSystolic} / {SupinePaDiastolic} ({SupinePaMean})";
    public PatientScenario Scenario { get; set; }
    public VitalsTrendScenario TrendScenario { get; set; } = VitalsTrendScenario.NormalStable;
    public string? Notes { get; set; }
}
