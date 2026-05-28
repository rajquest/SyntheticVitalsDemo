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

        return vitals is null ? null : BuildMessage(vitals);
    }

    private static string BuildMessage(VitalsSubmission vitals)
    {
        var patient = vitals.Patient;
        var clinic = patient?.Clinic?.Name ?? "SyntheticVitalsDemo";
        var submittedAt = vitals.SubmittedAtUtc.ToUniversalTime().ToString("yyyyMMddHHmmss");
        var messageControlId = vitals.Id.ToString("N");

        var message = new Message();
        message.AddSegmentMSH(
            "SyntheticVitalsDemo",
            clinic,
            "SyntheticVitalsDemo",
            "SyntheticVitalsDemo",
            string.Empty,
            "ORU^R01",
            messageControlId,
            "P",
            "2.5.1");
        message.SetValue("MSH.7", submittedAt);

        message.AddNewSegment(CreateSegment("PID",
            "1",
            string.Empty,
            patient?.Id.ToString() ?? vitals.PatientId.ToString(),
            string.Empty,
            ComponentValue(patient?.LastName ?? string.Empty, patient?.FirstName ?? string.Empty)));

        message.AddNewSegment(CreateSegment("OBR",
            "1",
            string.Empty,
            string.Empty,
            "85354-9^Blood pressure panel with all children optional^LN"));

        message.AddNewSegment(CreateObservation(1, "NM", CodedElement(VitalLoincCodes.SystolicBp, VitalLoincDisplays.SystolicBp), string.Empty, vitals.SystolicBp.ToString(), UcumUnits.MmHg));
        message.AddNewSegment(CreateObservation(2, "NM", CodedElement(VitalLoincCodes.DiastolicBp, VitalLoincDisplays.DiastolicBp), string.Empty, vitals.DiastolicBp.ToString(), UcumUnits.MmHg));
        message.AddNewSegment(CreateObservation(3, "NM", CodedElement(VitalLoincCodes.Spo2, VitalLoincDisplays.Spo2), string.Empty, vitals.Spo2.ToString(), UcumUnits.Percent));
        message.AddNewSegment(CreateObservation(4, "NM", CodedElement(VitalLoincCodes.HeartRate, VitalLoincDisplays.HeartRate), string.Empty, vitals.HeartRate.ToString(), UcumUnits.PerMinute));
        message.AddNewSegment(CreateObservation(5, "NM", CodedElement(VitalLoincCodes.BodyWeight, VitalLoincDisplays.BodyWeight), string.Empty, vitals.WeightLbs.ToString("0.0"), UcumUnits.Pounds));

        AddPulmonaryArteryPressureGroup(
            message,
            6,
            "1",
            vitals.SeatedPaSystolic,
            vitals.SeatedPaDiastolic,
            vitals.SeatedPaMean,
            CodedElement(BodyPositionCodes.SittingCode, BodyPositionCodes.SittingDisplay));

        AddPulmonaryArteryPressureGroup(
            message,
            10,
            "2",
            vitals.SupinePaSystolic,
            vitals.SupinePaDiastolic,
            vitals.SupinePaMean,
            CodedElement(BodyPositionCodes.SupineCode, BodyPositionCodes.SupineDisplay));

        return message.SerializeMessage(false);
    }

    private static void AddPulmonaryArteryPressureGroup(
        Message message,
        int startSetId,
        string observationSubId,
        int systolic,
        int diastolic,
        int mean,
        string bodyPosition)
    {
        message.AddNewSegment(CreateObservation(startSetId, "NM", CodedElement(VitalLoincCodes.PulmonaryArterySystolic, VitalLoincDisplays.PulmonaryArterySystolic), observationSubId, systolic.ToString(), UcumUnits.MmHg));
        message.AddNewSegment(CreateObservation(startSetId + 1, "NM", CodedElement(VitalLoincCodes.PulmonaryArteryDiastolic, VitalLoincDisplays.PulmonaryArteryDiastolic), observationSubId, diastolic.ToString(), UcumUnits.MmHg));
        message.AddNewSegment(CreateObservation(startSetId + 2, "NM", CodedElement(VitalLoincCodes.PulmonaryArteryMean, VitalLoincDisplays.PulmonaryArteryMean), observationSubId, mean.ToString(), UcumUnits.MmHg));
        message.AddNewSegment(CreateObservation(startSetId + 3, "CWE", CodedElement(VitalLoincCodes.BodyPosition, VitalLoincDisplays.BodyPosition), observationSubId, bodyPosition, string.Empty));
    }

    private static Segment CreateObservation(int setId, string valueType, string identifier, string observationSubId, string value, string units) =>
        CreateSegment("OBX",
            setId.ToString(),
            valueType,
            identifier,
            observationSubId,
            value,
            units,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "F");

    private static Segment CreateSegment(string name, params string[] fields)
    {
        var segment = new Segment(name, new HL7Encoding());
        for (var index = 0; index < fields.Length; index++)
        {
            segment.AddNewField(fields[index], index + 1);
        }

        return segment;
    }

    private static string ComponentValue(params string[] components) =>
        string.Join('^', components.Select(SanitizeComponent));

    private static string CodedElement(string code, string display, string system = "LN") =>
        $"{SanitizeComponent(code)}^{SanitizeComponent(display)}^{SanitizeComponent(system)}";

    private static string SanitizeComponent(string value) =>
        value.Replace("|", string.Empty).Replace("^", string.Empty).Replace("~", string.Empty).Replace("&", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
