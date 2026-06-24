using System.Globalization;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using DomainPatient = SyntheticVitalsDemo.Api.Models.Patient;
using DomainVitals = SyntheticVitalsDemo.Api.Models.VitalsSubmission;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class RhythmFhirExportService(AppDbContext db)
{
    private static readonly FhirJsonSerializer Serializer = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private const string LoincSystem    = "http://loinc.org";
    private const string UcumSystem     = "http://unitsofmeasure.org";
    private const string ObsCatSystem   = "http://terminology.hl7.org/CodeSystem/observation-category";
    private const string OrgIdSystem    = "https://api.rhythm360.io/fhir/clinic-code";
    private const string DeviceTypeName = "Cordella PA Sensor";

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
        var patient    = vitals.Patient;
        var clinicName = patient?.Clinic?.Name ?? "Unknown Clinic";
        var clinicCode = ToClinicCode(clinicName);
        var effective  = new DateTimeOffset(vitals.SubmittedAtUtc, TimeSpan.Zero);

        // TODO: Replace with real Cordella PA Sensor device serial number before production.
        var deviceSerial = $"CRD-{Random.Shared.Next(100000000, 999999999)}";

        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        bundle.Entry.Add(OrgEntry(clinicName, clinicCode));
        bundle.Entry.Add(PatientEntry(patient));
        bundle.Entry.Add(DeviceEntry(deviceSerial));
        bundle.Entry.Add(BpPanelEntry(vitals, effective));
        bundle.Entry.Add(ObsEntry("obs-spo2",        VitalLoincCodes.Spo2,                 VitalLoincDisplays.Spo2,                 vitals.Spo2,         UcumUnits.Percent,   UcumUnits.Percent,   effective));
        bundle.Entry.Add(ObsEntry("obs-heart-rate",  VitalLoincCodes.HeartRate,            VitalLoincDisplays.HeartRate,            vitals.HeartRate,    UcumUnits.PerMinute, UcumUnits.PerMinute, effective));
        bundle.Entry.Add(ObsEntry("obs-body-weight", VitalLoincCodes.BodyWeight,           VitalLoincDisplays.BodyWeight,           (int)vitals.WeightLbs, UcumUnits.Pounds,  UcumUnits.Pounds,   effective));
        bundle.Entry.Add(PaEntry("obs-seated-pa-systolic",  VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SeatedPaSystolic,  BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaEntry("obs-seated-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SeatedPaDiastolic, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaEntry("obs-seated-pa-mean",      VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SeatedPaMean,      BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective));
        bundle.Entry.Add(PaEntry("obs-supine-pa-systolic",  VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SupinePaSystolic,  BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));
        bundle.Entry.Add(PaEntry("obs-supine-pa-diastolic", VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SupinePaDiastolic, BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));
        bundle.Entry.Add(PaEntry("obs-supine-pa-mean",      VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SupinePaMean,      BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective));

        return bundle;
    }

    private static Bundle.EntryComponent OrgEntry(string name, string code) =>
        PostEntry("urn:uuid:org-1", "Organization",
            new Organization
            {
                Identifier = [new Identifier { System = OrgIdSystem, Value = code }],
                Name       = name
            });

    private static Bundle.EntryComponent PatientEntry(DomainPatient? patient) =>
        PostEntry("urn:uuid:pat-1", "Patient",
            new Patient
            {
                Name = [new HumanName
                {
                    Family = patient?.LastName ?? string.Empty,
                    Given  = [patient?.FirstName ?? string.Empty]
                }],
                BirthDateElement = patient is not null
                    ? new Date(patient.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                    : null
            });

    private static Bundle.EntryComponent DeviceEntry(string serialNumber) =>
        PostEntry("urn:uuid:dev-1", "Device",
            new Device
            {
                SerialNumber = serialNumber,
                Type         = new CodeableConcept { Text = DeviceTypeName }
            });

    private static Bundle.EntryComponent BpPanelEntry(DomainVitals vitals, DateTimeOffset effective) =>
        PostEntry("urn:uuid:obs-bp", "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept("85354-9", "Blood pressure panel"),
                Subject   = new ResourceReference("urn:uuid:pat-1"),
                Device    = new ResourceReference("urn:uuid:dev-1"),
                Performer = [new ResourceReference("urn:uuid:org-1")],
                Effective = new FhirDateTime(effective),
                Component =
                [
                    new Observation.ComponentComponent
                    {
                        Code  = new CodeableConcept { Coding = [new Coding { System = LoincSystem, Code = VitalLoincCodes.SystolicBp }] },
                        Value = MmHgQuantity(vitals.SystolicBp)
                    },
                    new Observation.ComponentComponent
                    {
                        Code  = new CodeableConcept { Coding = [new Coding { System = LoincSystem, Code = VitalLoincCodes.DiastolicBp }] },
                        Value = MmHgQuantity(vitals.DiastolicBp)
                    }
                ]
            });

    private static Bundle.EntryComponent ObsEntry(
        string obsId, string loincCode, string display,
        int value, string unitDisplay, string ucumCode, DateTimeOffset effective) =>
        PostEntry($"urn:uuid:{obsId}", "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept(loincCode, display),
                Subject   = new ResourceReference("urn:uuid:pat-1"),
                Device    = new ResourceReference("urn:uuid:dev-1"),
                Performer = [new ResourceReference("urn:uuid:org-1")],
                Effective = new FhirDateTime(effective),
                Value     = new Quantity { Value = value, Unit = unitDisplay, System = UcumSystem, Code = ucumCode }
            });

    private static Bundle.EntryComponent PaEntry(
        string obsId, string loincCode, string display,
        int value, string positionCode, string positionDisplay, DateTimeOffset effective) =>
        PostEntry($"urn:uuid:{obsId}", "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept(loincCode, display),
                Subject   = new ResourceReference("urn:uuid:pat-1"),
                Device    = new ResourceReference("urn:uuid:dev-1"),
                Performer = [new ResourceReference("urn:uuid:org-1")],
                Effective = new FhirDateTime(effective),
                Value     = MmHgQuantity(value),
                Component =
                [
                    new Observation.ComponentComponent
                    {
                        Code  = LoincConcept(VitalLoincCodes.BodyPosition, VitalLoincDisplays.BodyPosition),
                        Value = LoincConcept(positionCode, positionDisplay)
                    }
                ]
            });

    private static Bundle.EntryComponent PostEntry(string fullUrl, string resourceType, Resource resource) =>
        new()
        {
            FullUrl  = fullUrl,
            Resource = resource,
            Request  = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.POST, Url = resourceType }
        };

    private static List<CodeableConcept> VitalSignsCategory() =>
    [
        new CodeableConcept
        {
            Coding = [new Coding { System = ObsCatSystem, Code = "vital-signs" }]
        }
    ];

    private static CodeableConcept LoincConcept(string code, string display) =>
        new()
        {
            Coding = [new Coding { System = LoincSystem, Code = code, Display = display }],
            Text   = display
        };

    private static Quantity MmHgQuantity(decimal value) =>
        new() { Value = value, Unit = "mmHg", System = UcumSystem, Code = UcumUnits.MmHg };

    private static string ToClinicCode(string clinicName) =>
        clinicName.ToLowerInvariant().Replace(' ', '-').Replace("'", string.Empty);
}
