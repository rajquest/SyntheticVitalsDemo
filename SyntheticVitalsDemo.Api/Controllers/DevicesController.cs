using Microsoft.AspNetCore.Mvc;
using SyntheticVitalsDemo.Api.DTOs;
using SyntheticVitalsDemo.Api.Services;

namespace SyntheticVitalsDemo.Api.Controllers;

[ApiController]
[Route("api/v1/devices")]
public sealed class DevicesController(DeviceService devices) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeviceResponse>>> GetAll() =>
        Ok(await devices.GetAllAsync());

    [HttpGet("{deviceType}/{deviceId}")]
    public async Task<ActionResult<DeviceResponse>> Get(string deviceType, string deviceId)
    {
        var device = await devices.GetAsync(deviceType, deviceId);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> Create(CreateDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceType)) return ValidationProblem("DeviceType is required.");
        if (string.IsNullOrWhiteSpace(request.DeviceId))   return ValidationProblem("DeviceId is required.");

        var (response, error) = await devices.CreateAsync(request);
        if (error is not null) return Conflict(new { error });
        return CreatedAtAction(nameof(Get), new { deviceType = response!.DeviceType, deviceId = response.DeviceId }, response);
    }

    [HttpPut("{deviceType}/{deviceId}")]
    public async Task<ActionResult<DeviceResponse>> Update(
        string deviceType, string deviceId, UpdateDeviceRequest request)
    {
        var device = await devices.UpdateAsync(deviceType, deviceId, request);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpDelete("{deviceType}/{deviceId}")]
    public async Task<IActionResult> Delete(string deviceType, string deviceId) =>
        await devices.DeleteAsync(deviceType, deviceId) ? NoContent() : NotFound();

    [HttpPut("{deviceType}/{deviceId}/assign")]
    public async Task<ActionResult<DeviceResponse>> Assign(
        string deviceType, string deviceId, AssignDeviceRequest request)
    {
        var (response, error) = await devices.AssignAsync(deviceType, deviceId, request.PatientGuid);
        if (response is null && error is null) return NotFound();
        if (error is not null) return UnprocessableEntity(new { error });
        return Ok(response);
    }

    [HttpDelete("{deviceType}/{deviceId}/assign")]
    public async Task<ActionResult<DeviceResponse>> Unassign(string deviceType, string deviceId)
    {
        var (response, error) = await devices.UnassignAsync(deviceType, deviceId);
        if (response is null && error is null) return NotFound();
        if (error is not null) return UnprocessableEntity(new { error });
        return Ok(response);
    }
}
