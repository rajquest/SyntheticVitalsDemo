namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record DashboardSummaryResponse(
    int TotalClinics,
    int TotalPatients,
    int TotalVitalsSubmissions,
    int AbnormalVitalsCount,
    IReadOnlyList<RecentVitalsSubmissionResponse> RecentVitalsSubmissions);

public sealed record RecentVitalsSubmissionResponse(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string ClinicName,
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
    string Scenario);
