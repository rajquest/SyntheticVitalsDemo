using HL7.Dotnetcore;
using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class Hl7ExportService(AppDbContext db)
{
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

        return BuildMessage(vitals, device?.DeviceId ?? string.Empty);
    }

    private static string BuildMessage(VitalsSubmission vitals, string deviceSerial)
    {
        var patient = vitals.Patient;
        var clinic = patient?.Clinic;
        var clinicName = clinic?.Name ?? "SyntheticVitalsDemo";
        var receivingFacility = string.IsNullOrWhiteSpace(clinic?.SiteId)
            ? clinicName
            : $"{clinicName}^{clinic.SiteId}";
        var submittedAt = vitals.SubmittedAtUtc.ToUniversalTime().ToString("yyyyMMddHHmmss");
        var messageControlId = vitals.Id.ToString("N");

        var message = new Message();
        message.AddSegmentMSH(
            "SyntheticVitalsDemo",
            "Edwards Lifesciences",
            "PaceMate",
            receivingFacility,
            string.Empty,
            "ORU^R01",
            messageControlId,
            "P",
            "2.5.1");
        message.SetValue("MSH.7", submittedAt);

        message.AddNewSegment(CreateSegment("PID",
            "1",
            string.Empty,
            patient?.PatientGuid.ToString() ?? vitals.PatientId.ToString(),
            string.Empty,
            ComponentValue(patient?.LastName ?? string.Empty, patient?.FirstName ?? string.Empty),
            string.Empty,
            patient?.DateOfBirth.ToString("yyyyMMdd") ?? string.Empty,
            PatientSexCode(patient?.Sex)));

        message.AddNewSegment(CreateSegment("OBR",
            "1",
            string.Empty,
            string.Empty,
            "85354-9^Blood pressure panel with all children optional^LN"));

        message.AddNewSegment(CreateObservation(1, "NM", CodedElement(VitalLoincCodes.SystolicBp, VitalLoincDisplays.SystolicBp), string.Empty, vitals.SystolicBp.ToString(), UcumUnits.MmHg, deviceSerial));
        message.AddNewSegment(CreateObservation(2, "NM", CodedElement(VitalLoincCodes.DiastolicBp, VitalLoincDisplays.DiastolicBp), string.Empty, vitals.DiastolicBp.ToString(), UcumUnits.MmHg, deviceSerial));
        message.AddNewSegment(CreateObservation(3, "NM", CodedElement(VitalLoincCodes.Spo2, VitalLoincDisplays.Spo2), string.Empty, vitals.Spo2.ToString(), UcumUnits.Percent, deviceSerial));
        message.AddNewSegment(CreateObservation(4, "NM", CodedElement(VitalLoincCodes.HeartRate, VitalLoincDisplays.HeartRate), string.Empty, vitals.HeartRate.ToString(), UcumUnits.PerMinute, deviceSerial));
        message.AddNewSegment(CreateObservation(5, "NM", CodedElement(VitalLoincCodes.BodyWeight, VitalLoincDisplays.BodyWeight), string.Empty, vitals.WeightLbs.ToString("0.0"), UcumUnits.Pounds, deviceSerial));

        AddPulmonaryArteryPressureGroup(
            message,
            6,
            "1",
            vitals.SeatedPaSystolic,
            vitals.SeatedPaDiastolic,
            vitals.SeatedPaMean,
            CodedElement(BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay),
            deviceSerial);

        AddPulmonaryArteryPressureGroup(
            message,
            10,
            "2",
            vitals.SupinePaSystolic,
            vitals.SupinePaDiastolic,
            vitals.SupinePaMean,
            CodedElement(BodyPositionCodes.SupineCode, BodyPositionCodes.SupineDisplay),
            deviceSerial);

        return message.SerializeMessage(false);
    }

    private static void AddPulmonaryArteryPressureGroup(
        Message message,
        int startSetId,
        string observationSubId,
        int systolic,
        int diastolic,
        int mean,
        string bodyPosition,
        string deviceSerial)
    {
        message.AddNewSegment(CreateObservation(startSetId, "NM", CodedElement(VitalLoincCodes.PulmonaryArterySystolic, VitalLoincDisplays.PulmonaryArterySystolic), observationSubId, systolic.ToString(), UcumUnits.MmHg, deviceSerial));
        message.AddNewSegment(CreateObservation(startSetId + 1, "NM", CodedElement(VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic), observationSubId, diastolic.ToString(), UcumUnits.MmHg, deviceSerial));
        message.AddNewSegment(CreateObservation(startSetId + 2, "NM", CodedElement(VitalLoincCodes.PulmonaryArteryMean, VitalLoincDisplays.PulmonaryArteryMean), observationSubId, mean.ToString(), UcumUnits.MmHg, deviceSerial));
        message.AddNewSegment(CreateObservation(startSetId + 3, "CWE", CodedElement(VitalLoincCodes.BodyPosition, VitalLoincDisplays.BodyPosition), observationSubId, bodyPosition, string.Empty, deviceSerial));
    }

    private static Segment CreateObservation(int setId, string valueType, string identifier, string observationSubId, string value, string units, string deviceSerial) =>
        CreateSegment("OBX",
            setId.ToString(),
            valueType,
            identifier,
            observationSubId,
            value,
            units,
            string.Empty,   // OBX-7  References Range
            string.Empty,   // OBX-8  Abnormal Flags
            string.Empty,   // OBX-9  Probability
            string.Empty,   // OBX-10 Nature of Abnormal Test
            "F",            // OBX-11 Observation Result Status
            string.Empty,   // OBX-12 Effective Date of Reference Range
            string.Empty,   // OBX-13 User Defined Access Checks
            string.Empty,   // OBX-14 Date/Time of the Observation
            string.Empty,   // OBX-15 Producer's ID
            string.Empty,   // OBX-16 Responsible Observer
            string.Empty,   // OBX-17 Observation Method
            SanitizeComponent(deviceSerial)); // OBX-18 Equipment Instance Identifier

    private static Segment CreateSegment(string name, params string[] fields)
    {
        var segment = new Segment(name, new HL7Encoding());
        for (var index = 0; index < fields.Length; index++)
        {
            segment.AddNewField(fields[index], index + 1);
        }

        return segment;
    }

    private static string PatientSexCode(Sex? sex) => sex switch
    {
        Sex.Female => "F",
        Sex.Male => "M",
        Sex.Other => "O",
        Sex.Unknown => "U",
        _ => "U"
    };

    private static string ComponentValue(params string[] components) =>
        string.Join('^', components.Select(SanitizeComponent));

    private static string CodedElement(string code, string display, string system = "LN") =>
        $"{SanitizeComponent(code)}^{SanitizeComponent(display)}^{SanitizeComponent(system)}";

    private static string SanitizeComponent(string value) =>
        value.Replace("|", string.Empty).Replace("^", string.Empty).Replace("~", string.Empty).Replace("&", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
