import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/api.service';
import { Device, Patient } from '../../core/models';

export interface DeviceAssignDialogData {
  patient: Patient;
}

export interface DeviceAssignDialogResult {
  changed: boolean;
}

@Component({
  selector: 'app-device-assign-dialog',
  imports: [CommonModule, FormsModule, MatDialogModule, MatFormFieldModule, MatIconModule, MatSelectModule, MatTooltipModule],
  templateUrl: './device-assign-dialog.component.html'
})
export class DeviceAssignDialogComponent implements OnInit {
  assignedDevices = signal<Device[]>([]);
  availableDevices = signal<Device[]>([]);
  selectedDeviceKey = '';
  error = signal<string | null>(null);
  private changed = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public readonly data: DeviceAssignDialogData,
    private readonly dialogRef: MatDialogRef<DeviceAssignDialogComponent, DeviceAssignDialogResult>,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.loadDevices();
  }

  loadDevices(): void {
    this.api.getDevices().subscribe({
      next: devices => {
        this.assignedDevices.set(devices.filter(d => d.patientGuid === this.data.patient.patientGuid));
        this.availableDevices.set(devices.filter(d => d.isActive && !d.isAssigned));
      }
    });
  }

  assign(): void {
    if (!this.selectedDeviceKey) return;
    const [deviceType, deviceId] = this.selectedDeviceKey.split('||');
    this.error.set(null);
    this.api.assignDevice(deviceType, deviceId, { patientGuid: this.data.patient.patientGuid }).subscribe({
      next: () => {
        this.selectedDeviceKey = '';
        this.changed = true;
        this.loadDevices();
      },
      error: err => this.error.set(err?.error?.error ?? 'Failed to assign device.')
    });
  }

  unassign(device: Device): void {
    this.error.set(null);
    this.api.unassignDevice(device.deviceType, device.deviceId).subscribe({
      next: () => {
        this.changed = true;
        this.loadDevices();
      },
      error: err => this.error.set(err?.error?.error ?? 'Failed to unassign device.')
    });
  }

  close(): void {
    this.dialogRef.close({ changed: this.changed });
  }
}
