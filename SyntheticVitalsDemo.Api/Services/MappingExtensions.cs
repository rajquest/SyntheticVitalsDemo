using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public static class MappingExtensions
{
    public static ClinicResponse ToResponse(this Clinic clinic) =>
        new(clinic.Id, clinic.Name, clinic.CreatedAtUtc, clinic.Patients.Count, clinic.Patients.Sum(patient => patient.VitalsSubmissions.Count));

    public static PatientResponse ToResponse(this Patient patient) =>
        new(
            patient.Id,
            patient.ClinicId,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Sex.ToString(),
            patient.Scenario.ToString(),
            patient.SystolicBp,
            patient.DiastolicBp,
            patient.BloodPressureDisplay,
            patient.Spo2,
            patient.HeartRate,
            patient.WeightLbs,
            patient.PaSystolic,
            patient.PaDiastolic,
            patient.PaMean,
            patient.PulmonaryPressureDisplay,
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
            vitals.PulmonaryPressureDisplay,
            vitals.Scenario.ToString(),
            vitals.TrendScenario.ToString(),
            vitals.Notes);
}
