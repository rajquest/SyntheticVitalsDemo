using Bogus;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed record PulmonaryPressure(int Systolic, int Diastolic, int Mean);

public sealed class PulmonaryPressureGeneratorService
{
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
            PatientScenario.NormalPaPressure => new PressureRange(15, 30, 4, 12, 9, 20),
            PatientScenario.MildPulmonaryHypertension => new PressureRange(31, 45, 13, 20, 21, 30),
            PatientScenario.ModeratePulmonaryHypertension => new PressureRange(46, 60, 18, 28, 31, 40),
            PatientScenario.SeverePulmonaryHypertension => new PressureRange(61, 90, 25, 40, 41, 60),
            PatientScenario.ElevatedPaDiastolicPressure => new PressureRange(35, 60, 22, 38, 28, 45),
            PatientScenario.HighPaMeanPressure => new PressureRange(45, 75, 18, 35, 35, 55),
            _ => new PressureRange(15, 30, 4, 12, 9, 20)
        };

        var systolic = _faker.Random.Int(range.SystolicMin, range.SystolicMax);
        var diastolic = _faker.Random.Int(range.DiastolicMin, Math.Min(range.DiastolicMax, systolic - 1));
        var calculatedMean = (int)Math.Round(diastolic + (systolic - diastolic) / 3.0, MidpointRounding.AwayFromZero);
        var mean = Math.Clamp(calculatedMean, range.MeanMin, range.MeanMax);

        return new PulmonaryPressure(systolic, diastolic, mean);
    }

    private sealed record PressureRange(
        int SystolicMin,
        int SystolicMax,
        int DiastolicMin,
        int DiastolicMax,
        int MeanMin,
        int MeanMax);
}
