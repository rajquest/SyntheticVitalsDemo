using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class PulmonaryPressureTrendGeneratorService
{
    private readonly Random _random = new(4317);

    public IReadOnlyList<PulmonaryPressureTrendReading> Generate(
        PulmonaryPressureTrendScenario scenario,
        int days,
        DateTime endDateUtc)
    {
        days = Math.Clamp(days, 1, 365);
        var start = endDateUtc.Date.AddDays(-(days - 1)).AddHours(8);

        return scenario switch
        {
            PulmonaryPressureTrendScenario.NormalStable => StableSeries(scenario, days, start, 15, 30, 4, 14, 9, 16),
            PulmonaryPressureTrendScenario.MildlyElevatedStable => StableSeries(scenario, days, start, 31, 45, 15, 22, 17, 28),
            PulmonaryPressureTrendScenario.ProgressivelyWorseningPaMean => DirectionalSeries(scenario, days, start, 21, 28, 35, 50),
            PulmonaryPressureTrendScenario.SuddenPaPressureSpike => SpikeSeries(scenario, days, start),
            PulmonaryPressureTrendScenario.ImprovingAfterDiureticAdjustment => DirectionalSeries(scenario, days, start, 32, 50, 18, 30),
            PulmonaryPressureTrendScenario.PersistentlyHighPaDiastolic => StableSeries(scenario, days, start, 40, 70, 22, 40, 30, 55),
            _ => StableSeries(PulmonaryPressureTrendScenario.NormalStable, days, start, 15, 30, 4, 14, 9, 16)
        };
    }

    private IReadOnlyList<PulmonaryPressureTrendReading> StableSeries(
        PulmonaryPressureTrendScenario scenario,
        int days,
        DateTime start,
        int systolicMin,
        int systolicMax,
        int diastolicMin,
        int diastolicMax,
        int meanMin,
        int meanMax)
    {
        var meanBase = _random.Next(meanMin + 1, meanMax);
        return Enumerable.Range(0, days)
            .Select(index => ReadingFromMean(
                scenario,
                start.AddDays(index),
                Clamp(meanBase + Jitter(2), meanMin, meanMax),
                systolicMin,
                systolicMax,
                diastolicMin,
                diastolicMax))
            .ToArray();
    }

    private IReadOnlyList<PulmonaryPressureTrendReading> DirectionalSeries(
        PulmonaryPressureTrendScenario scenario,
        int days,
        DateTime start,
        int startMeanMin,
        int startMeanMax,
        int endMeanMin,
        int endMeanMax)
    {
        var startMean = _random.Next(startMeanMin, startMeanMax + 1);
        var endMean = _random.Next(endMeanMin, endMeanMax + 1);

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var progress = days <= 1 ? 0 : index / (double)(days - 1);
                var mean = (int)Math.Round(startMean + ((endMean - startMean) * progress), MidpointRounding.AwayFromZero);
                mean += Jitter(1);

                var isImproving = endMean < startMean;
                var systolicMin = isImproving ? 30 : 31;
                var systolicMax = isImproving ? 70 : 75;
                var diastolicMin = isImproving ? 12 : 13;
                var diastolicMax = isImproving ? 35 : 38;

                return ReadingFromMean(scenario, start.AddDays(index), mean, systolicMin, systolicMax, diastolicMin, diastolicMax);
            })
            .ToArray();
    }

    private IReadOnlyList<PulmonaryPressureTrendReading> SpikeSeries(
        PulmonaryPressureTrendScenario scenario,
        int days,
        DateTime start)
    {
        var baselineMean = _random.Next(18, 29);
        var spikeMean = _random.Next(35, 56);
        var spikeIndex = _random.Next(days / 2, Math.Max(days / 2 + 1, days - 1));

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var distance = Math.Abs(index - spikeIndex);
                var mean = distance switch
                {
                    0 => spikeMean,
                    1 => (int)Math.Round((baselineMean + spikeMean) / 2.0, MidpointRounding.AwayFromZero),
                    _ => baselineMean + Jitter(2)
                };

                return ReadingFromMean(scenario, start.AddDays(index), mean, 25, 75, 9, 35);
            })
            .ToArray();
    }

    private PulmonaryPressureTrendReading ReadingFromMean(
        PulmonaryPressureTrendScenario scenario,
        DateTime readingDateUtc,
        int paMean,
        int systolicMin,
        int systolicMax,
        int diastolicMin,
        int diastolicMax)
    {
        var paDiastolic = Clamp(paMean - _random.Next(6, 12), diastolicMin, diastolicMax);
        var paSystolic = Clamp(paDiastolic + ((paMean - paDiastolic) * 3) + Jitter(3), systolicMin, systolicMax);
        paMean = PulmonaryPressureGeneratorService.CalculateMean(paSystolic, paDiastolic);
        var pressure = new PulmonaryPressure(paSystolic, paDiastolic, paMean);
        PulmonaryPressureGeneratorService.ValidateNotHardStop(pressure);

        return new PulmonaryPressureTrendReading(readingDateUtc, paSystolic, paDiastolic, paMean, scenario);
    }

    private static int CalculatePaMean(int paSystolic, int paDiastolic) =>
        PulmonaryPressureGeneratorService.CalculateMean(paSystolic, paDiastolic);

    private int Jitter(int spread) => _random.Next(-spread, spread + 1);

    private static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value));
}

public sealed record PulmonaryPressureTrendReading(
    DateTime ReadingDateUtc,
    int SeatedPaSystolic,
    int SeatedPaDiastolic,
    int SeatedPaMean,
    PulmonaryPressureTrendScenario Scenario);
