namespace SyntheticVitalsDemo.Api.DTOs;

public sealed record DeviceResponse(
    string DeviceType,
    string DeviceId,
    string? ImeiNumber,
    string? BluetoothAddress,
    DateTime DateTimeCreated,
    DateTime DateTimeLastUpdated,
    DateTime? DateTimeDeactivated,
    DateTime? DateTimePatientAssigned,
    Guid? PatientGuid,
    bool IsActive,
    bool IsAssigned);

public sealed record CreateDeviceRequest(
    string DeviceType,
    string DeviceId,
    string? ImeiNumber,
    string? BluetoothAddress);

public sealed record UpdateDeviceRequest(
    string? ImeiNumber,
    string? BluetoothAddress);

public sealed record AssignDeviceRequest(Guid PatientGuid);
