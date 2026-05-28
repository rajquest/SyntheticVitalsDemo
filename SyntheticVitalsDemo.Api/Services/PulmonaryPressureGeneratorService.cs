using Bogus;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed record PulmonaryPressure(int Systolic, int Diastolic, int Mean);
public sealed record PulmonaryPressurePair(PulmonaryPressure Seated, PulmonaryPressure Supine);

public enum PulmonaryPressureSeverity
{
    Reference,
    Elevated,
    Warning,
    HardStop
}

public sealed class PulmonaryPressureGeneratorService
{
    public const int ReferenceSystolicMin = 15;
    public const int ReferenceSystolicMax = 30;
    public const int ReferenceDiastolicMin = 4;
    public const int ReferenceDiastolicMax = 14;
    public const int ReferenceMeanMin = 9;
    public const int ReferenceMeanMax = 16;
    public const int WarningSystolicThreshold = 60;
    public const int WarningDiastolicThreshold = 30;
    public const int WarningMeanThreshold = 40;
    public const int HardStopSystolicThreshold = 150;
    public const int HardStopDiastolicThreshold = 80;
    public const int HardStopMeanThreshold = 100;

    private readonly Faker _faker = new("en_US");

    public PulmonaryPressure Generate(PatientScenario scenario)
    {
        var selectedScenario = scenario == PatientScenario.MixedPulmonaryPressureVariability
            ? _faker.PickRandom(
                PatientScenario.NormalPaPressure,
                PatientScenario.MildPulmonaryHypertension,
                PatientScenario.ModeratePulmonaryHypertension,
                PatientScenario.SeverePulmonaryHypertension)
            : scenario;

        var range = selectedScenario switch
        {
            PatientScenario.NormalPaPressure => new PressureRange(ReferenceSystolicMin, ReferenceSystolicMax, ReferenceDiastolicMin, ReferenceDiastolicMax, ReferenceMeanMin, ReferenceMeanMax),
            PatientScenario.MildPulmonaryHypertension => new PressureRange(31, 45, 15, 22, 17, 28),
            PatientScenario.ModeratePulmonaryHypertension => new PressureRange(46, WarningSystolicThreshold, 18, WarningDiastolicThreshold, 27, WarningMeanThreshold),
            PatientScenario.SeverePulmonaryHypertension => new PressureRange(WarningSystolicThreshold + 1, 95, WarningDiastolicThreshold + 1, 45, WarningMeanThreshold + 1, 65),
            PatientScenario.ElevatedPaDiastolicPressure => new PressureRange(35, WarningSystolicThreshold, 22, 38, 28, 45),
            PatientScenario.HighPaMeanPressure => new PressureRange(45, 95, 18, 45, 35, 65),
            _ => new PressureRange(ReferenceSystolicMin, ReferenceSystolicMax, ReferenceDiastolicMin, ReferenceDiastolicMax, ReferenceMeanMin, ReferenceMeanMax)
        };

        return GenerateInRange(range);
    }

    public PulmonaryPressurePair GeneratePair(PatientScenario scenario)
    {
        var seated = Generate(scenario);
        var supine = GenerateSupineFromSeated(seated);
        return new PulmonaryPressurePair(seated, supine);
    }

    public PulmonaryPressure GenerateSupineFromSeated(PulmonaryPressure seated)
    {
        ValidateNotHardStop(seated);

        for (var attempt = 0; attempt < 50; attempt++)
        {
            var systolic = Math.Clamp(seated.Systolic + _faker.Random.Int(-2, 3), ReferenceSystolicMin, HardStopSystolicThreshold);
            var diastolic = Math.Clamp(seated.Diastolic + _faker.Random.Int(-1, 2), ReferenceDiastolicMin, Math.Min(HardStopDiastolicThreshold, systolic - 1));
            var pressure = Create(systolic, diastolic);

            if (Classify(pressure) != PulmonaryPressureSeverity.HardStop)
            {
                return pressure;
            }
        }

        throw new InvalidOperationException("Unable to generate supine PA pressure below hard-stop thresholds.");
    }

    public static PulmonaryPressure Create(int systolic, int diastolic) =>
        new(systolic, diastolic, CalculateMean(systolic, diastolic));

    public static int CalculateMean(int systolic, int diastolic) =>
        (int)Math.Round(diastolic + (systolic - diastolic) / 3.0, MidpointRounding.AwayFromZero);

    public static PulmonaryPressureSeverity Classify(PulmonaryPressure pressure)
    {
        if (pressure.Systolic > HardStopSystolicThreshold ||
            pressure.Diastolic > HardStopDiastolicThreshold ||
            pressure.Mean > HardStopMeanThreshold)
        {
            return PulmonaryPressureSeverity.HardStop;
        }

        if (pressure.Systolic > WarningSystolicThreshold ||
            pressure.Diastolic > WarningDiastolicThreshold ||
            pressure.Mean > WarningMeanThreshold)
        {
            return PulmonaryPressureSeverity.Warning;
        }

        if (IsReference(pressure))
        {
            return PulmonaryPressureSeverity.Reference;
        }

        return PulmonaryPressureSeverity.Elevated;
    }

    public static bool IsReference(PulmonaryPressure pressure) =>
        pressure.Systolic is >= ReferenceSystolicMin and <= ReferenceSystolicMax &&
        pressure.Diastolic is >= ReferenceDiastolicMin and <= ReferenceDiastolicMax &&
        pressure.Mean is >= ReferenceMeanMin and <= ReferenceMeanMax;

    public static void ValidateNotHardStop(PulmonaryPressure pressure)
    {
        if (Classify(pressure) == PulmonaryPressureSeverity.HardStop)
        {
            throw new InvalidOperationException("Generated PA pressure exceeded hard-stop thresholds.");
        }
    }

    private PulmonaryPressure GenerateInRange(PressureRange range)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var systolic = _faker.Random.Int(range.SystolicMin, range.SystolicMax);
            var diastolic = _faker.Random.Int(range.DiastolicMin, Math.Min(range.DiastolicMax, systolic - 1));
            var pressure = Create(systolic, diastolic);

            if (pressure.Mean >= range.MeanMin &&
                pressure.Mean <= range.MeanMax &&
                Classify(pressure) != PulmonaryPressureSeverity.HardStop)
            {
                return pressure;
            }
        }

        throw new InvalidOperationException("Unable to generate PA pressure within the configured clinical range.");
    }

    private sealed record PressureRange(
        int SystolicMin,
        int SystolicMax,
        int DiastolicMin,
        int DiastolicMax,
        int MeanMin,
        int MeanMax);
}
