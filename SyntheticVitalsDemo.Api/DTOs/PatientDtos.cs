namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record PatientResponse(
    Guid PatientGuid,
    Guid ClinicId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Sex,
    string Scenario,
    int SystolicBp,
    int DiastolicBp,
    string BloodPressureDisplay,
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
    DateTime CreatedAtUtc,
    int VitalsSubmissionCount);

public sealed record CreatePatientRequest(string FirstName, string LastName, DateOnly DateOfBirth, string Sex, string Scenario);
public sealed record UpdatePatientRequest(string FirstName, string LastName, DateOnly DateOfBirth, string Sex, string Scenario);
public sealed record GeneratePatientsRequest(
    int Count,
    int MalePercentage,
    string VitalsTrendScenario,
    int TrendDays,
    string? Scenario = null);
public sealed record GeneratePatientsResponse(
    Guid ClinicId,
    int GeneratedCount,
    int UpdatedPatientCount,
    IReadOnlyList<PatientResponse> Patients);
