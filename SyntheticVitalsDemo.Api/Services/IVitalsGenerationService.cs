using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public interface IVitalsGenerationService
{
    VitalsSubmission Generate(Patient patient, DateTime submittedAtUtc, int index = 0, int total = 1);
    IReadOnlyList<VitalsSubmission> GenerateSeries(Patient patient, int days, DateTime endDateUtc);
    int CalculatePaMean(int paSystolic, int paDiastolic);
}
