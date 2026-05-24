namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record ClinicResponse(Guid Id, string Name, string? Location, DateTime CreatedAtUtc, int PatientCount);
public sealed record CreateClinicRequest(string Name, string? Location);
public sealed record UpdateClinicRequest(string Name, string? Location);
