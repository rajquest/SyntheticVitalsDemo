using Microsoft.EntityFrameworkCore;
using SyntheticVitalsDemo.Api.Data;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Models;

namespace SyntheticVitalsDemo.Api.Services;

public sealed class DeviceService(AppDbContext db)
{
    public async Task<IReadOnlyList<DeviceResponse>> GetAllAsync() =>
        await db.Devices
            .OrderBy(x => x.DeviceType)
            .ThenBy(x => x.DeviceId)
            .Select(x => new DeviceResponse(
                x.DeviceType,
                x.DeviceId,
                x.ImeiNumber,
                x.BluetoothAddress,
                x.DateTimeCreated,
                x.DateTimeLastUpdated,
                x.DateTimeDeactivated,
                x.DateTimePatientAssigned,
                x.PatientGuid,
                x.DateTimeDeactivated == null,
                x.PatientGuid != null))
            .ToArrayAsync();

    public async Task<DeviceResponse?> GetAsync(string deviceType, string deviceId)
    {
        var device = await db.Devices.FindAsync(deviceType, deviceId);
        return device?.ToResponse();
    }

    public async Task<(DeviceResponse? Response, string? Error)> CreateAsync(CreateDeviceRequest request)
    {
        var existing = await db.Devices.FindAsync(request.DeviceType, request.DeviceId);
        if (existing is not null)
            return (null, $"Device '{request.DeviceType}/{request.DeviceId}' already exists.");

        var device = new Device
        {
            DeviceType = request.DeviceType.Trim(),
            DeviceId   = request.DeviceId.Trim(),
            ImeiNumber = request.ImeiNumber?.Trim(),
            BluetoothAddress = request.BluetoothAddress?.Trim()
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return (device.ToResponse(), null);
    }

    public async Task<DeviceResponse?> UpdateAsync(string deviceType, string deviceId, UpdateDeviceRequest request)
    {
        var device = await db.Devices.FindAsync(deviceType, deviceId);
        if (device is null) return null;

        device.ImeiNumber        = request.ImeiNumber?.Trim();
        device.BluetoothAddress  = request.BluetoothAddress?.Trim();
        device.DateTimeLastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return device.ToResponse();
    }

    public async Task<bool> DeleteAsync(string deviceType, string deviceId)
    {
        var device = await db.Devices.FindAsync(deviceType, deviceId);
        if (device is null) return false;

        device.DateTimeDeactivated = DateTime.UtcNow;
        device.DateTimeLastUpdated = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<(DeviceResponse? Response, string? Error)> AssignAsync(
        string deviceType, string deviceId, Guid patientGuid)
    {
        var device = await db.Devices.FindAsync(deviceType, deviceId);
        if (device is null) return (null, null);

        if (device.DateTimeDeactivated is not null)
            return (null, "Device is deactivated and cannot be assigned.");

        if (device.PatientGuid is not null && device.PatientGuid != patientGuid)
            return (null, "Device is already assigned to another patient.");

        if (!await db.Patients.AnyAsync(x => x.PatientGuid == patientGuid))
            return (null, "Patient not found.");

        // Enforce one device per patient: auto-unassign any existing device for this patient
        var currentDevice = await db.Devices
            .FirstOrDefaultAsync(x => x.PatientGuid == patientGuid
                                   && !(x.DeviceType == deviceType && x.DeviceId == deviceId));
        if (currentDevice is not null)
        {
            currentDevice.PatientGuid             = null;
            currentDevice.DateTimePatientAssigned = null;
            currentDevice.DateTimeLastUpdated     = DateTime.UtcNow;
        }

        device.PatientGuid             = patientGuid;
        device.DateTimePatientAssigned = DateTime.UtcNow;
        device.DateTimeLastUpdated     = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (device.ToResponse(), null);
    }

    public async Task<(DeviceResponse? Response, string? Error)> UnassignAsync(
        string deviceType, string deviceId)
    {
        var device = await db.Devices.FindAsync(deviceType, deviceId);
        if (device is null) return (null, null);

        if (device.PatientGuid is null)
            return (null, "Device is not currently assigned to a patient.");

        device.PatientGuid             = null;
        device.DateTimePatientAssigned = null;
        device.DateTimeLastUpdated     = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (device.ToResponse(), null);
    }
}
