namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record PatientResponse(
    Guid Id,
    Guid ClinicId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Sex,
    string Scenario,
    DateTime CreatedAtUtc,
    int VitalsSubmissionCount);

public sealed record CreatePatientRequest(string FirstName, string LastName, DateOnly DateOfBirth, string Sex, string Scenario);
public sealed record UpdatePatientRequest(string FirstName, string LastName, DateOnly DateOfBirth, string Sex, string Scenario);
public sealed record GeneratePatientsRequest(int Count, string[]? Scenarios);
