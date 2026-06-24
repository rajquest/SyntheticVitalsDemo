using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class PulmonaryPressureTrendGeneratorService
{
    private readonly Random _random = new(4317);

    // Primary entry point — driven by the stable VitalsTrendScenario enum.
    public IReadOnlyList<PulmonaryPressureTrendReading> Generate(
        VitalsTrendScenario scenario,
        int days,
        DateTime endDateUtc)
    {
        days = Math.Clamp(days, 1, 365);
        var start = endDateUtc.Date.AddDays(-(days - 1)).AddHours(8);

        return scenario switch
        {
            // Normal reference range, stable day-to-day
            VitalsTrendScenario.NormalStable =>
                StableSeries(scenario, days, start, systolicMin: 15, systolicMax: 30, diastolicMin: 4, diastolicMax: 14, meanMin: 9, meanMax: 16),

            // Chronically elevated but clinically stable; no trend direction
            VitalsTrendScenario.ChronicHfStable =>
                StableSeries(scenario, days, start, systolicMin: 31, systolicMax: 45, diastolicMin: 15, diastolicMax: 22, meanMin: 17, meanMax: 28),

            // Slow progressive rise — fluid accumulating over days/weeks
            VitalsTrendScenario.EarlyFluidOverload =>
                DirectionalSeries(scenario, days, start, startMeanMin: 16, startMeanMax: 22, endMeanMin: 32, endMeanMax: 48),

            // Sudden large spike then persists at elevated level — acute decompensation
            VitalsTrendScenario.AcuteHfDecompensation =>
                SpikeSeries(scenario, days, start, baselineMeanMin: 20, baselineMeanMax: 28, spikeMeanMin: 45, spikeMeanMax: 68, returnsToBaseline: false),

            // Starts elevated, progressively improves with treatment
            VitalsTrendScenario.DiuresisTreatmentResponse =>
                DirectionalSeries(scenario, days, start, startMeanMin: 32, startMeanMax: 50, endMeanMin: 15, endMeanMax: 24),

            // Single moderate spike then returns cleanly to normal baseline — transient activity
            VitalsTrendScenario.ExerciseActivitySpike =>
                SpikeSeries(scenario, days, start, baselineMeanMin: 13, baselineMeanMax: 20, spikeMeanMin: 28, spikeMeanMax: 44, returnsToBaseline: true),

            _ =>
                StableSeries(VitalsTrendScenario.NormalStable, days, start, 15, 30, 4, 14, 9, 16)
        };
    }

    // Legacy overload — delegates to the VitalsTrendScenario overload via mapping.
    public IReadOnlyList<PulmonaryPressureTrendReading> Generate(
        PulmonaryPressureTrendScenario scenario,
        int days,
        DateTime endDateUtc)
    {
        var mapped = scenario switch
        {
            PulmonaryPressureTrendScenario.NormalStable                   => VitalsTrendScenario.NormalStable,
            PulmonaryPressureTrendScenario.MildlyElevatedStable           => VitalsTrendScenario.ChronicHfStable,
            PulmonaryPressureTrendScenario.ProgressivelyWorseningPaMean   => VitalsTrendScenario.EarlyFluidOverload,
            PulmonaryPressureTrendScenario.SuddenPaPressureSpike          => VitalsTrendScenario.AcuteHfDecompensation,
            PulmonaryPressureTrendScenario.ImprovingAfterDiureticAdjustment => VitalsTrendScenario.DiuresisTreatmentResponse,
            PulmonaryPressureTrendScenario.PersistentlyHighPaDiastolic    => VitalsTrendScenario.ChronicHfStable,
            _ => VitalsTrendScenario.NormalStable
        };
        return Generate(mapped, days, endDateUtc);
    }

    private IReadOnlyList<PulmonaryPressureTrendReading> StableSeries(
        VitalsTrendScenario scenario,
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
        VitalsTrendScenario scenario,
        int days,
        DateTime start,
        int startMeanMin,
        int startMeanMax,
        int endMeanMin,
        int endMeanMax)
    {
        var startMean = _random.Next(startMeanMin, startMeanMax + 1);
        var endMean   = _random.Next(endMeanMin,   endMeanMax + 1);
        var isImproving = endMean < startMean;

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var progress = days <= 1 ? 0 : index / (double)(days - 1);
                var mean = (int)Math.Round(startMean + ((endMean - startMean) * progress), MidpointRounding.AwayFromZero);
                mean = Clamp(mean + Jitter(1), Math.Min(startMeanMin, endMeanMin), Math.Max(startMeanMax, endMeanMax));

                var (sysMin, sysMax, diaMin, diaMax) = isImproving
                    ? (30, 70, 12, 35)
                    : (20, 70, 8, 35);

                return ReadingFromMean(scenario, start.AddDays(index), mean, sysMin, sysMax, diaMin, diaMax);
            })
            .ToArray();
    }

    private IReadOnlyList<PulmonaryPressureTrendReading> SpikeSeries(
        VitalsTrendScenario scenario,
        int days,
        DateTime start,
        int baselineMeanMin,
        int baselineMeanMax,
        int spikeMeanMin,
        int spikeMeanMax,
        bool returnsToBaseline)
    {
        var baselineMean = _random.Next(baselineMeanMin, baselineMeanMax + 1);
        var spikeMean    = _random.Next(spikeMeanMin,    spikeMeanMax + 1);
        var spikeIndex   = _random.Next(days / 2, Math.Max(days / 2 + 1, days - 1));

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var distance = Math.Abs(index - spikeIndex);
                int mean;

                if (returnsToBaseline)
                {
                    // Symmetric spike: rises then falls back to baseline (e.g. exercise)
                    mean = distance switch
                    {
                        0 => spikeMean,
                        1 => (int)Math.Round((baselineMean + spikeMean) / 2.0, MidpointRounding.AwayFromZero),
                        _ => baselineMean + Jitter(2)
                    };
                }
                else
                {
                    // One-sided spike: rises sharply and stays elevated (e.g. acute decompensation)
                    mean = index < spikeIndex
                        ? baselineMean + Jitter(2)
                        : index == spikeIndex
                            ? spikeMean
                            : Clamp(spikeMean - ((index - spikeIndex) * 2) + Jitter(2), baselineMean, spikeMean);
                }

                return ReadingFromMean(scenario, start.AddDays(index), mean, 15, 95, 4, 45);
            })
            .ToArray();
    }

    private PulmonaryPressureTrendReading ReadingFromMean(
        VitalsTrendScenario scenario,
        DateTime readingDateUtc,
        int paMean,
        int systolicMin,
        int systolicMax,
        int diastolicMin,
        int diastolicMax)
    {
        var paDiastolic = Clamp(paMean - _random.Next(6, 12), diastolicMin, diastolicMax);
        var paSystolic  = Clamp(paDiastolic + ((paMean - paDiastolic) * 3) + Jitter(3), systolicMin, systolicMax);

        // Enforce guardrails: systolic > diastolic, values not negative
        paSystolic  = Math.Max(paSystolic,  paDiastolic + 1);
        paDiastolic = Math.Max(paDiastolic, 0);
        paSystolic  = Math.Max(paSystolic,  1);

        paMean = PulmonaryPressureGeneratorService.CalculateMean(paSystolic, paDiastolic);

        var pressure = new PulmonaryPressure(paSystolic, paDiastolic, paMean);
        PulmonaryPressureGeneratorService.ValidateNotHardStop(pressure);

        return new PulmonaryPressureTrendReading(readingDateUtc, paSystolic, paDiastolic, paMean, scenario);
    }

    private int Jitter(int spread) => _random.Next(-spread, spread + 1);

    private static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value));
}

public sealed record PulmonaryPressureTrendReading(
    DateTime ReadingDateUtc,
    int SeatedPaSystolic,
    int SeatedPaDiastolic,
    int SeatedPaMean,
    VitalsTrendScenario Scenario);
