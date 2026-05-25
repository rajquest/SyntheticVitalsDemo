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
    int PaSystolic,
    int PaDiastolic,
    int PaMean,
    string PulmonaryPressureDisplay,
    string Scenario);
