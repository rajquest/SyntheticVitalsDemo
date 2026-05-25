using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class VitalsGenerationService(PulmonaryPressureTrendGeneratorService pressureTrendGenerator) : IVitalsGenerationService
{
    private readonly Random _random = new(1729);

    public int CalculatePaMean(int paSystolic, int paDiastolic) =>
        (int)Math.Round(paDiastolic + (paSystolic - paDiastolic) / 3.0, MidpointRounding.AwayFromZero);

    public IReadOnlyList<VitalsSubmission> GenerateSeries(Patient patient, int days, DateTime endDateUtc)
    {
        if (days is not (1 or 7 or 14 or 30 or 90))
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be 1, 7, 14, 30, or 90.");
        }

        var start = endDateUtc.Date.AddDays(-(days - 1)).AddHours(8);
        return Enumerable.Range(0, days)
            .Select(i => Generate(patient, start.AddDays(i), i, days))
            .ToArray();
    }

    public IReadOnlyList<VitalsSubmission> GenerateSeries(
        Patient patient,
        int days,
        DateTime endDateUtc,
        PulmonaryPressureTrendScenario trendScenario)
    {
        days = Math.Clamp(days, 7, 30);
        var pressureReadings = pressureTrendGenerator.Generate(trendScenario, days, endDateUtc);

        return pressureReadings
            .Select((pressure, index) =>
            {
                var vitals = Generate(patient, pressure.ReadingDateUtc, index, pressureReadings.Count);
                vitals.PaSystolic = pressure.PaSystolic;
                vitals.PaDiastolic = pressure.PaDiastolic;
                vitals.PaMean = pressure.PaMean;
                vitals.TrendScenario = trendScenario;
                vitals.Notes = $"Synthetic PA pressure trend: {trendScenario}.";
                return vitals;
            })
            .ToArray();
    }

    public VitalsSubmission Generate(Patient patient, DateTime submittedAtUtc, int index = 0, int total = 1)
    {
        var progress = total <= 1 ? 0m : index / (decimal)(total - 1);
        var baselineWeight = 150m + StableOffset(patient.Id, 0, 75);
        var trend = patient.Scenario switch
        {
            PatientScenario.HeartFailureWorsening => progress,
            PatientScenario.HeartFailureImproving => 1m - progress,
            PatientScenario.WeightGainTrend => progress,
            _ => 0m
        };

        var bp = patient.Scenario switch
        {
            PatientScenario.Hypertension => (Sys: 150, Dia: 94),
            PatientScenario.Hypotension => (Sys: 88, Dia: 56),
            PatientScenario.HeartFailureWorsening => (Sys: 124 + (int)(trend * 10), Dia: 78 + (int)(trend * 6)),
            PatientScenario.HeartFailureImproving => (Sys: 134 - (int)((1m - trend) * 8), Dia: 84 - (int)((1m - trend) * 5)),
            _ => (Sys: 118, Dia: 74)
        };

        var pa = patient.Scenario switch
        {
            PatientScenario.ElevatedPaPressure => (Sys: 48, Dia: 24),
            PatientScenario.Hypertension => (Sys: 36, Dia: 17),
            PatientScenario.HeartFailureStable => (Sys: 38, Dia: 18),
            PatientScenario.HeartFailureWorsening => (Sys: 32 + (int)(trend * 18), Dia: 15 + (int)(trend * 9)),
            PatientScenario.HeartFailureImproving => (Sys: 48 - (int)((1m - trend) * 16), Dia: 24 - (int)((1m - trend) * 8)),
            _ => (Sys: 26, Dia: 11)
        };

        var lowSpo2Episode = patient.Scenario == PatientScenario.LowSpo2Episode && total > 1 && index >= total / 2 - 1 && index <= total / 2 + 1;
        var spo2Base = patient.Scenario switch
        {
            PatientScenario.LowSpo2Episode when lowSpo2Episode => 88,
            PatientScenario.HeartFailureWorsening => 97 - (int)(progress * 4),
            PatientScenario.HeartFailureImproving => 93 + (int)(progress * 4),
            _ => 97
        };

        var hrBase = patient.Scenario switch
        {
            PatientScenario.Hypotension => 92,
            PatientScenario.LowSpo2Episode when lowSpo2Episode => 102,
            PatientScenario.HeartFailureWorsening => 78 + (int)(progress * 18),
            PatientScenario.HeartFailureImproving => 96 - (int)(progress * 16),
            _ => 74
        };

        var weightTrend = patient.Scenario switch
        {
            PatientScenario.HeartFailureWorsening => progress * 9m,
            PatientScenario.HeartFailureImproving => (1m - progress) * 6m,
            PatientScenario.WeightGainTrend => progress * 11m,
            _ => 0m
        };

        var paSystolic = Clamp(pa.Sys + Jitter(2), 10, 90);
        var paDiastolic = Clamp(pa.Dia + Jitter(2), 5, Math.Min(45, paSystolic - 4));

        return new VitalsSubmission
        {
            PatientId = patient.Id,
            SubmittedAtUtc = DateTime.SpecifyKind(submittedAtUtc, DateTimeKind.Utc),
            SystolicBp = Clamp(bp.Sys + Jitter(5), 70, 220),
            DiastolicBp = Clamp(bp.Dia + Jitter(4), 40, 130),
            Spo2 = Clamp(spo2Base + Jitter(1), 75, 100),
            HeartRate = Clamp(hrBase + Jitter(5), 40, 160),
            WeightLbs = Math.Round(baselineWeight + weightTrend + (decimal)Jitter(1) / 2, 1),
            PaSystolic = paSystolic,
            PaDiastolic = paDiastolic,
            PaMean = CalculatePaMean(paSystolic, paDiastolic),
            Scenario = patient.Scenario,
            TrendScenario = PulmonaryPressureTrendScenario.NormalStable,
            Notes = patient.Scenario == PatientScenario.LowSpo2Episode && lowSpo2Episode
                ? "Synthetic low SpO2 episode for demo purposes."
                : "Synthetic demo vitals."
        };
    }

    private int Jitter(int spread) => _random.Next(-spread, spread + 1);

    private static int StableOffset(Guid id, int min, int max)
    {
        var value = Math.Abs(BitConverter.ToInt32(id.ToByteArray(), 0));
        return min + value % (max - min + 1);
    }

    private static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value));
}
