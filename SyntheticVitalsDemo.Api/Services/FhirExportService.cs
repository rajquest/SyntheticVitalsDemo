using System.Globalization;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using DomainPatient = SyntheticVitalsDemo.Api.Models.Patient;
using DomainVitals = SyntheticVitalsDemo.Api.Models.VitalsSubmission;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class FhirExportService(AppDbContext db)
{
    private static readonly FhirJsonSerializer Serializer = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private const string LoincSystem    = "http://loinc.org";
    private const string UcumSystem     = "http://unitsofmeasure.org";
    private const string ObsCatSystem   = "http://terminology.hl7.org/CodeSystem/observation-category";

    public async Task<string?> ExportVitalsSubmissionAsync(Guid id)
    {
        var vitals = await db.VitalsSubmissions
            .Include(x => x.Patient)!.ThenInclude(x => x!.Clinic)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (vitals is null) return null;
        var raw = Serializer.SerializeToString(BuildBundle(vitals));
        using var doc = JsonDocument.Parse(raw);
        return JsonSerializer.Serialize(doc.RootElement, JsonOptions);
    }

    private static Bundle BuildBundle(DomainVitals vitals)
    {
        var patient   = vitals.Patient;
        var patientId = (patient?.PatientGuid ?? vitals.PatientId).ToString();
        var effective = new DateTimeOffset(vitals.SubmittedAtUtc, TimeSpan.Zero);

        var bundle = new Bundle
        {
            Id               = vitals.Id.ToString(),
            Type             = Bundle.BundleType.Collection,
            TimestampElement = new Instant(effective)
        };

        bundle.Entry.Add(ToEntry($"urn:synthetic-vitals:patient:{patientId}", BuildPatient(patient, patientId)));

        bundle.Entry.Add(ObsEntry(vitals.Id, patientId, "systolic-bp",       VitalLoincCodes.SystolicBp,           VitalLoincDisplays.SystolicBp,           vitals.SystolicBp,   UcumUnits.MmHg,      effective));
        bundle.Entry.Add(ObsEntry(vitals.Id, patientId, "diastolic-bp",      VitalLoincCodes.DiastolicBp,          VitalLoincDisplays.DiastolicBp,          vitals.DiastolicBp,  UcumUnits.MmHg,      effective));
        bundle.Entry.Add(ObsEntry(vitals.Id, patientId, "spo2",              VitalLoincCodes.Spo2,                 VitalLoincDisplays.Spo2,                 vitals.Spo2,         UcumUnits.Percent,   effective));
        bundle.Entry.Add(ObsEntry(vitals.Id, patientId, "heart-rate",        VitalLoincCodes.HeartRate,            VitalLoincDisplays.HeartRate,            vitals.HeartRate,    UcumUnits.PerMinute, effective));
        bundle.Entry.Add(ObsEntry(vitals.Id, patientId, "body-weight",       VitalLoincCodes.BodyWeight,           VitalLoincDisplays.BodyWeight,           (int)vitals.WeightLbs, UcumUnits.Pounds,  effective));

        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "seated-pa-systolic",  VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SeatedPaSystolic,  BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "seated-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SeatedPaDiastolic, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "seated-pa-mean",      VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SeatedPaMean,      BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "supine-pa-systolic",  VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SupinePaSystolic,  BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));
        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "supine-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SupinePaDiastolic, BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));
        bundle.Entry.Add(PaObsEntry(vitals.Id, patientId, "supine-pa-mean",      VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SupinePaMean,      BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));

        return bundle;
    }

    private static Patient BuildPatient(DomainPatient? patient, string patientId) =>
        new()
        {
            Id   = patientId,
            Name = [new HumanName
            {
                Family = patient?.LastName ?? string.Empty,
                Given  = [patient?.FirstName ?? string.Empty]
            }],
            BirthDateElement = patient is not null
                ? new Date(patient.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                : null
        };

    private static Bundle.EntryComponent ObsEntry(
        Guid submissionId, string patientId, string suffix,
        string loincCode, string display,
        int value, string ucumCode, DateTimeOffset effective) =>
        ToEntry(
            $"urn:synthetic-vitals:observation:{submissionId}:{suffix}",
            new Observation
            {
                Id       = $"{submissionId}-{suffix}",
                Status   = ObservationStatus.Final,
                Category = VitalSignsCategory(),
                Code     = LoincConcept(loincCode, display),
                Subject  = new ResourceReference($"Patient/{patientId}"),
                Effective = new FhirDateTime(effective),
                Value    = UcumQuantity(value, ucumCode)
            });

    private static Bundle.EntryComponent PaObsEntry(
        Guid submissionId, string patientId, string suffix,
        string loincCode, string display,
        int value, string positionCode, string positionDisplay, DateTimeOffset effective) =>
        ToEntry(
            $"urn:synthetic-vitals:observation:{submissionId}:{suffix}",
            new Observation
            {
                Id        = $"{submissionId}-{suffix}",
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept(loincCode, display),
                Subject   = new ResourceReference($"Patient/{patientId}"),
                Effective = new FhirDateTime(effective),
                Value     = UcumQuantity(value, UcumUnits.MmHg),
                Component =
                [
                    new Observation.ComponentComponent
                    {
                        Code  = LoincConcept(VitalLoincCodes.BodyPosition, VitalLoincDisplays.BodyPosition),
                        Value = LoincConcept(positionCode, positionDisplay)
                    }
                ]
            });

    private static Bundle.EntryComponent ToEntry(string fullUrl, Resource resource) =>
        new() { FullUrl = fullUrl, Resource = resource };

    private static List<CodeableConcept> VitalSignsCategory() =>
    [
        new CodeableConcept
        {
            Coding = [new Coding { System = ObsCatSystem, Code = "vital-signs", Display = "Vital Signs" }]
        }
    ];

    private static CodeableConcept LoincConcept(string code, string display) =>
        new()
        {
            Coding = [new Coding { System = LoincSystem, Code = code, Display = display }],
            Text   = display
        };

    private static Quantity UcumQuantity(decimal value, string ucumCode) =>
        new() { Value = value, System = UcumSystem, Code = ucumCode };
}
