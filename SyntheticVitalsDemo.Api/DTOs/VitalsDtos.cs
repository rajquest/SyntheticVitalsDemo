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
    int SeatedPaSystolic,
    int SeatedPaDiastolic,
    int SeatedPaMean,
    int SupinePaSystolic,
    int SupinePaDiastolic,
    int SupinePaMean,
    string SeatedPulmonaryPressureDisplay,
    string SupinePulmonaryPressureDisplay,
    string Scenario,
    string TrendScenario,
    string? Notes);

public sealed record GenerateVitalsRequest(DateTime? SubmittedAtUtc);
public sealed record GenerateVitalsSeriesRequest(int Days, DateTime? EndDateUtc, bool ReplaceExisting, string PulmonaryPressureScenario);
