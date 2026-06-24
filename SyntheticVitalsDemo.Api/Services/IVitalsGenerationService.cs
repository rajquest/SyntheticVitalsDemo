using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public interface IVitalsGenerationService
{
    VitalsSubmission Generate(Patient patient, DateTime submittedAtUtc, int index = 0, int total = 1);
    IReadOnlyList<VitalsSubmission> GenerateSeries(Patient patient, int days, DateTime endDateUtc);
    IReadOnlyList<VitalsSubmission> GenerateSeries(Patient patient, int days, DateTime endDateUtc, PulmonaryPressureTrendScenario trendScenario);
    IReadOnlyList<VitalsSubmission> GenerateSeries(Patient patient, int days, DateTime endDateUtc, VitalsTrendScenario trendScenario);
    int CalculatePaMean(int paSystolic, int paDiastolic);
}
