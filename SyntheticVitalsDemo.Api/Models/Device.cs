namespace SyntheticVitalsDemo.Api.Models;

public sealed class Device
{
    public string DeviceType { get; set; } = default!;
    public string DeviceId { get; set; } = default!;
    public string? ImeiNumber { get; set; }
    public string? BluetoothAddress { get; set; }
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeLastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? DateTimeDeactivated { get; set; }
    public DateTime? DateTimePatientAssigned { get; set; }
    public Guid? PatientGuid { get; set; }
}
