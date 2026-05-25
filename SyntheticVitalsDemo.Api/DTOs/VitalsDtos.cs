namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record VitalsSubmissionResponse(
    Guid Id,
    Guid PatientId,
    DateTime SubmittedAtUtc,
    int SystolicBp,
    int DiastolicBp,
    int Spo2,
    int HeartRate,
    decimal WeightLbs,
    int PaSystolic,
    int PaDiastolic,
    int PaMean,
    string PulmonaryPressureDisplay,
    string Scenario,
    string TrendScenario,
    string? Notes);

public sealed record GenerateVitalsRequest(DateTime? SubmittedAtUtc);
public sealed record GenerateVitalsSeriesRequest(int Days, DateTime? EndDateUtc, bool ReplaceExisting, string PulmonaryPressureScenario);
