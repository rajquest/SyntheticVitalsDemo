using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public static class MappingExtensions
{
    public static ClinicResponse ToResponse(this Clinic clinic) =>
        new(clinic.Id, clinic.Name, clinic.Location, clinic.CreatedAtUtc, clinic.Patients.Count);

    public static PatientResponse ToResponse(this Patient patient) =>
        new(
            patient.Id,
            patient.ClinicId,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Sex.ToString(),
            patient.Scenario.ToString(),
            patient.CreatedAtUtc,
            patient.VitalsSubmissions.Count);

    public static VitalsSubmissionResponse ToResponse(this VitalsSubmission vitals) =>
        new(
            vitals.Id,
            vitals.PatientId,
            vitals.SubmittedAtUtc,
            vitals.SystolicBp,
            vitals.DiastolicBp,
            vitals.Spo2,
            vitals.HeartRate,
            vitals.WeightLbs,
            vitals.PaSystolic,
            vitals.PaDiastolic,
            vitals.PaMean,
            vitals.Scenario.ToString(),
            vitals.Notes);
}
