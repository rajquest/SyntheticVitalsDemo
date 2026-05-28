using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class FhirExportService(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<string?> ExportVitalsSubmissionAsync(Guid id)
    {
        var vitals = await db.VitalsSubmissions
            .Include(x => x.Patient)!.ThenInclude(x => x!.Clinic)
            .FirstOrDefaultAsync(x => x.Id == id);

        return vitals is null ? null : JsonSerializer.Serialize(BuildBundle(vitals), JsonOptions);
    }

    private static object BuildBundle(VitalsSubmission vitals)
    {
        var patient = vitals.Patient;
        var patientId = patient?.Id ?? vitals.PatientId;
        var effectiveDateTime = vitals.SubmittedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

        return new
        {
            resourceType = "Bundle",
            id = vitals.Id.ToString(),
            type = "collection",
            timestamp = effectiveDateTime,
            entry = new object[]
            {
                new { fullUrl = $"urn:synthetic-vitals:patient:{patientId}", resource = BuildPatient(patient, patientId) },
                BuildObservationEntry(vitals.Id, patientId, "systolic-bp", VitalLoincCodes.SystolicBp, VitalLoincDisplays.SystolicBp, vitals.SystolicBp, UcumUnits.MmHg, effectiveDateTime),
                BuildObservationEntry(vitals.Id, patientId, "diastolic-bp", VitalLoincCodes.DiastolicBp, VitalLoincDisplays.DiastolicBp, vitals.DiastolicBp, UcumUnits.MmHg, effectiveDateTime),
                BuildObservationEntry(vitals.Id, patientId, "spo2", VitalLoincCodes.Spo2, VitalLoincDisplays.Spo2, vitals.Spo2, UcumUnits.Percent, effectiveDateTime),
                BuildObservationEntry(vitals.Id, patientId, "heart-rate", VitalLoincCodes.HeartRate, VitalLoincDisplays.HeartRate, vitals.HeartRate, UcumUnits.PerMinute, effectiveDateTime),
                BuildObservationEntry(vitals.Id, patientId, "body-weight", VitalLoincCodes.BodyWeight, VitalLoincDisplays.BodyWeight, vitals.WeightLbs, UcumUnits.Pounds, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "seated-pa-systolic", VitalLoincCodes.PulmonaryArterySystolic, VitalLoincDisplays.PulmonaryArterySystolic, vitals.SeatedPaSystolic, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "seated-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SeatedPaDiastolic, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "seated-pa-mean", VitalLoincCodes.PulmonaryArteryMean, VitalLoincDisplays.PulmonaryArteryMean, vitals.SeatedPaMean, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "supine-pa-systolic", VitalLoincCodes.PulmonaryArterySystolic, VitalLoincDisplays.PulmonaryArterySystolic, vitals.SupinePaSystolic, BodyPositionCodes.SupineCode, BodyPositionCodes.SupineDisplay, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "supine-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SupinePaDiastolic, BodyPositionCodes.SupineCode, BodyPositionCodes.SupineDisplay, effectiveDateTime),
                BuildPulmonaryObservationEntry(vitals.Id, patientId, "supine-pa-mean", VitalLoincCodes.PulmonaryArteryMean, VitalLoincDisplays.PulmonaryArteryMean, vitals.SupinePaMean, BodyPositionCodes.SupineCode, BodyPositionCodes.SupineDisplay, effectiveDateTime)
            }
        };
    }

    private static object BuildPatient(Patient? patient, Guid patientId) =>
        new
        {
            resourceType = "Patient",
            id = patientId.ToString(),
            name = new[]
            {
                new
                {
                    family = patient?.LastName ?? string.Empty,
                    given = new[] { patient?.FirstName ?? string.Empty }
                }
            },
            birthDate = patient?.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

    private static object BuildObservationEntry(Guid submissionId, Guid patientId, string suffix, string loincCode, string display, decimal value, string unit, string effectiveDateTime) =>
        new
        {
            fullUrl = $"urn:synthetic-vitals:observation:{submissionId}:{suffix}",
            resource = BuildObservation(submissionId, patientId, suffix, loincCode, display, value, unit, effectiveDateTime)
        };

    private static object BuildPulmonaryObservationEntry(Guid submissionId, Guid patientId, string suffix, string loincCode, string display, int value, string positionCode, string positionDisplay, string effectiveDateTime) =>
        new
        {
            fullUrl = $"urn:synthetic-vitals:observation:{submissionId}:{suffix}",
            resource = BuildObservation(
                submissionId,
                patientId,
                suffix,
                loincCode,
                display,
                value,
                UcumUnits.MmHg,
                effectiveDateTime,
                positionCode,
                positionDisplay)
        };

    private static object BuildObservation(
        Guid submissionId,
        Guid patientId,
        string suffix,
        string loincCode,
        string display,
        decimal value,
        string unit,
        string effectiveDateTime,
        string? positionCode = null,
        string? positionDisplay = null)
    {
        var observation = new Dictionary<string, object?>
        {
            ["resourceType"] = "Observation",
            ["id"] = $"{submissionId}-{suffix}",
            ["status"] = "final",
            ["category"] = new[]
            {
                new
                {
                    coding = new[]
                    {
                        new
                        {
                            system = "http://terminology.hl7.org/CodeSystem/observation-category",
                            code = "vital-signs",
                            display = "Vital Signs"
                        }
                    }
                }
            },
            ["code"] = Coding(loincCode, display),
            ["subject"] = new { reference = $"Patient/{patientId}" },
            ["effectiveDateTime"] = effectiveDateTime,
            ["valueQuantity"] = new
            {
                value,
                unit,
                system = "http://unitsofmeasure.org",
                code = unit
            }
        };

        if (positionCode is not null && positionDisplay is not null)
        {
            observation["component"] = new[]
            {
                new
                {
                    code = Coding(VitalLoincCodes.BodyPosition, VitalLoincDisplays.BodyPosition),
                    valueCodeableConcept = Coding(positionCode, positionDisplay)
                }
            };
        }

        return observation;
    }

    private static object Coding(string code, string display) =>
        new
        {
            coding = new[]
            {
                new
                {
                    system = code.StartsWith("LA", StringComparison.OrdinalIgnoreCase)
                        ? "http://loinc.org"
                        : "http://loinc.org",
                    code,
                    display
                }
            },
            text = display
        };
}
