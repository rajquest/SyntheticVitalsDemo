using Bogus;
using Bogus.DataSets;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;
using BogusGender = Bogus.DataSets.Name.Gender;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class SyntheticPatientGeneratorService(PulmonaryPressureGeneratorService pressureGenerator)
{
    private readonly Faker _faker = new("en_US");

    public IReadOnlyList<Patient> Generate(Guid clinicId, GeneratePatientsRequest request, PatientScenario scenario)
    {
        var count = Math.Clamp(request.Count, 1, 100);
        var maleCount = (int)Math.Round(count * (request.MalePercentage / 100.0), MidpointRounding.AwayFromZero);

        return Enumerable.Range(0, count)
            .Select(index => GenerateOne(clinicId, index < maleCount ? Sex.Male : Sex.Female, scenario))
            .ToArray();
    }

    private Patient GenerateOne(Guid clinicId, Sex sex, PatientScenario scenario)
    {
        var bogusGender = sex == Sex.Male ? BogusGender.Male : BogusGender.Female;
        var firstName = _faker.Name.FirstName(bogusGender);
        var lastName = _faker.Name.LastName(bogusGender);
        var pressure = pressureGenerator.Generate(scenario);
        var vitals = GenerateVitals(scenario);

        return new Patient
        {
            ClinicId = clinicId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = DateOnly.FromDateTime(_faker.Date.Past(85, DateTime.Today.AddYears(-18))),
            Sex = sex,
            Scenario = scenario,
            SystolicBp = vitals.SystolicBp,
            DiastolicBp = vitals.DiastolicBp,
            Spo2 = vitals.Spo2,
            HeartRate = vitals.HeartRate,
            WeightLbs = vitals.WeightLbs,
            PaSystolic = pressure.Systolic,
            PaDiastolic = pressure.Diastolic,
            PaMean = pressure.Mean
        };
    }

    private GeneratedVitals GenerateVitals(PatientScenario scenario)
    {
        var hypertensionBias = scenario is
            PatientScenario.MildPulmonaryHypertension or
            PatientScenario.ModeratePulmonaryHypertension or
            PatientScenario.SeverePulmonaryHypertension or
            PatientScenario.HighPaMeanPressure;

        var systolic = hypertensionBias ? _faker.Random.Int(126, 158) : _faker.Random.Int(104, 132);
        var diastolic = hypertensionBias ? _faker.Random.Int(76, 96) : _faker.Random.Int(62, 84);
        var spo2 = scenario == PatientScenario.SeverePulmonaryHypertension
            ? _faker.Random.Int(90, 96)
            : _faker.Random.Int(94, 99);
        var heartRate = hypertensionBias ? _faker.Random.Int(72, 105) : _faker.Random.Int(58, 88);
        var weight = Math.Round(_faker.Random.Decimal(112m, 245m), 1);

        return new GeneratedVitals(systolic, diastolic, spo2, heartRate, weight);
    }

    private sealed record GeneratedVitals(int SystolicBp, int DiastolicBp, int Spo2, int HeartRate, decimal WeightLbs);
}
