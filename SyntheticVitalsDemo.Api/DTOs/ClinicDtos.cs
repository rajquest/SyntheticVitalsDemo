namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record ClinicResponse(Guid Id, string Name, string? SiteId, DateTime CreatedAtUtc, int PatientCount, int SubmissionCount);
public sealed record CreateClinicRequest(string Name);
public sealed record UpdateClinicRequest(string Name);
