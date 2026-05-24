using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public static class Validation
{
    public static bool TryParseScenario(string value, out PatientScenario scenario) =>
        Enum.TryParse(value, ignoreCase: true, out scenario) && Enum.IsDefined(scenario);

    public static bool TryParseSex(string value, out Sex sex) =>
        Enum.TryParse(value, ignoreCase: true, out sex) && Enum.IsDefined(sex);

    public static string? ValidateClinic(string name) =>
        string.IsNullOrWhiteSpace(name) ? "Clinic name is required." : null;

    public static string? ValidatePatient(string firstName, string lastName, DateOnly dateOfBirth, string sex, string scenario)
    {
        if (string.IsNullOrWhiteSpace(firstName)) return "Patient first name is required.";
        if (string.IsNullOrWhiteSpace(lastName)) return "Patient last name is required.";
        if (dateOfBirth == default) return "Patient date of birth is required.";
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18))) return "Use an adult synthetic date of birth for this demo.";
        if (!TryParseSex(sex, out _)) return $"Sex must be one of: {string.Join(", ", Enum.GetNames<Sex>())}.";
        if (!TryParseScenario(scenario, out _)) return $"Scenario must be one of: {string.Join(", ", Enum.GetNames<PatientScenario>())}.";

        return null;
    }

    public static bool IsAbnormal(int systolicBp, int diastolicBp, int spo2, int heartRate, int paMean) =>
        systolicBp >= 140 || systolicBp < 90 ||
        diastolicBp >= 90 || diastolicBp < 60 ||
        spo2 < 92 ||
        heartRate > 110 || heartRate < 50 ||
        paMean > 25;
}
