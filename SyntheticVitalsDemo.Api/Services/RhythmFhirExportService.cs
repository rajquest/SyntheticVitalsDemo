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
    private const string DeviceTypeName = "Cordella Patient Gateway";
    private const string UnassignedDeviceSerial = "UNASSIGNED";

    // Reflects the real order a vitals session is taken: BP panel, then
    // SpO2/HR, then weight, then seated PA, then a reposition pause before
    // supine PA. Readings within the same measurement event (BP panel;
    // seated PA systolic/diastolic/mean; supine PA systolic/diastolic/mean)
    // share one effectiveDateTime since they come from a single reading.
    private const int BpPanelOffsetSeconds    = 0;
    private const int Spo2OffsetSeconds       = 15;
    private const int HeartRateOffsetSeconds  = 16;
    private const int WeightOffsetSeconds     = 30;
    private const int SeatedPaOffsetSeconds   = 90;
    private const int SupinePaOffsetSeconds   = 180;

    public async Task<string?> ExportVitalsSubmissionAsync(Guid id)
    {
        var vitals = await db.VitalsSubmissions
            .Include(x => x.Patient)!.ThenInclude(x => x!.Clinic)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (vitals is null) return null;

        var device = vitals.Patient is not null
            ? await db.Devices.FirstOrDefaultAsync(x => x.PatientGuid == vitals.Patient.PatientGuid
                                                     && x.DateTimeDeactivated == null)
            : null;

        var raw = Serializer.SerializeToString(BuildBundle(vitals, device));
        using var doc = JsonDocument.Parse(raw);
        return JsonSerializer.Serialize(doc.RootElement, JsonOptions);
    }

    private static Bundle BuildBundle(DomainVitals vitals, Models.Device? device)
    {
        var patient    = vitals.Patient;
        var clinicName = patient?.Clinic?.Name ?? "Unknown Clinic";
        var clinicCode = ToClinicCode(clinicName);
        var effective  = new DateTimeOffset(vitals.SubmittedAtUtc, TimeSpan.Zero);
        var deviceSerial = device?.DeviceId ?? UnassignedDeviceSerial;

        var orgUrn = NewUrn();
        var patUrn = NewUrn();
        var devUrn = NewUrn();

        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        bundle.Entry.Add(OrgEntry(orgUrn, clinicName, clinicCode));
        bundle.Entry.Add(PatientEntry(patUrn, patient));
        bundle.Entry.Add(DeviceEntry(devUrn, deviceSerial));
        bundle.Entry.Add(BpPanelEntry(vitals, effective.AddSeconds(BpPanelOffsetSeconds), patUrn, devUrn, orgUrn));
        bundle.Entry.Add(ObsEntry(VitalLoincCodes.Spo2,      VitalLoincDisplays.Spo2,      vitals.Spo2,         UcumUnits.Percent,   UcumUnits.Percent,   effective.AddSeconds(Spo2OffsetSeconds),      patUrn, devUrn, orgUrn));
        bundle.Entry.Add(ObsEntry(VitalLoincCodes.HeartRate, VitalLoincDisplays.HeartRate, vitals.HeartRate,    UcumUnits.PerMinute, UcumUnits.PerMinute, effective.AddSeconds(HeartRateOffsetSeconds), patUrn, devUrn, orgUrn));
        bundle.Entry.Add(ObsEntry(VitalLoincCodes.BodyWeight, VitalLoincDisplays.BodyWeight, (int)vitals.WeightLbs, UcumUnits.Pounds, UcumUnits.Pounds,   effective.AddSeconds(WeightOffsetSeconds),    patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SeatedPaSystolic,  BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective.AddSeconds(SeatedPaOffsetSeconds),  patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SeatedPaDiastolic, BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective.AddSeconds(SeatedPaOffsetSeconds), patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SeatedPaMean,      BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay, effective.AddSeconds(SeatedPaOffsetSeconds),      patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArterySystolic,  VitalLoincDisplays.PulmonaryArterySystolic,  vitals.SupinePaSystolic,  BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective.AddSeconds(SupinePaOffsetSeconds),  patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic, vitals.SupinePaDiastolic, BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective.AddSeconds(SupinePaOffsetSeconds), patUrn, devUrn, orgUrn));
        bundle.Entry.Add(PaEntry(VitalLoincCodes.PulmonaryArteryMean,      VitalLoincDisplays.PulmonaryArteryMean,      vitals.SupinePaMean,      BodyPositionCodes.SupineCode,  BodyPositionCodes.SupineDisplay,  effective.AddSeconds(SupinePaOffsetSeconds),      patUrn, devUrn, orgUrn));

        return bundle;
    }

    private static string NewUrn() => $"urn:uuid:{Guid.NewGuid()}";

    private static Bundle.EntryComponent OrgEntry(string urn, string name, string code) =>
        PostEntry(urn, "Organization",
            new Organization
            {
                Identifier = [new Identifier { System = OrgIdSystem, Value = code }],
                Name       = name
            });

    private static Bundle.EntryComponent PatientEntry(string urn, DomainPatient? patient) =>
        PostEntry(urn, "Patient",
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

    private static Bundle.EntryComponent DeviceEntry(string urn, string serialNumber) =>
        PostEntry(urn, "Device",
            new Device
            {
                SerialNumber = serialNumber,
                Type         = new CodeableConcept { Text = DeviceTypeName }
            });

    private static Bundle.EntryComponent BpPanelEntry(DomainVitals vitals, DateTimeOffset effective, string patUrn, string devUrn, string orgUrn) =>
        PostEntry(NewUrn(), "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept("85354-9", "Blood pressure panel"),
                Subject   = new ResourceReference(patUrn),
                Device    = new ResourceReference(devUrn),
                Performer = [new ResourceReference(orgUrn)],
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
        string loincCode, string display,
        int value, string unitDisplay, string ucumCode, DateTimeOffset effective,
        string patUrn, string devUrn, string orgUrn) =>
        PostEntry(NewUrn(), "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept(loincCode, display),
                Subject   = new ResourceReference(patUrn),
                Device    = new ResourceReference(devUrn),
                Performer = [new ResourceReference(orgUrn)],
                Effective = new FhirDateTime(effective),
                Value     = new Quantity { Value = value, Unit = unitDisplay, System = UcumSystem, Code = ucumCode }
            });

    private static Bundle.EntryComponent PaEntry(
        string loincCode, string display,
        int value, string positionCode, string positionDisplay, DateTimeOffset effective,
        string patUrn, string devUrn, string orgUrn) =>
        PostEntry(NewUrn(), "Observation",
            new Observation
            {
                Status    = ObservationStatus.Final,
                Category  = VitalSignsCategory(),
                Code      = LoincConcept(loincCode, display),
                Subject   = new ResourceReference(patUrn),
                Device    = new ResourceReference(devUrn),
                Performer = [new ResourceReference(orgUrn)],
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
