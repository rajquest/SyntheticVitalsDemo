import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, signal } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/api.service';
import { Device, Patient } from '../../core/models';

@Component({
  selector: 'app-device-list',
  imports: [
    CommonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule
  ],
  templateUrl: './device-list.component.html'
})
export class DeviceListComponent implements OnInit {
  devices = signal<Device[]>([]);
  patients = signal<Patient[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  displayedColumns = ['deviceType', 'deviceId', 'imeiNumber', 'status', 'patientGuid', 'dateTimePatientAssigned'];
  dataSource = new MatTableDataSource<Device>([]);

  @ViewChild(MatPaginator) set paginator(value: MatPaginator | undefined) {
    if (value) this.dataSource.paginator = value;
  }

  @ViewChild(MatSort) set sort(value: MatSort | undefined) {
    if (value) this.dataSource.sort = value;
  }

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.dataSource.filterPredicate = (device, filter) => {
      const normalized = [
        device.deviceType,
        device.deviceId,
        device.imeiNumber ?? '',
        device.patientGuid ?? '',
        device.isActive ? 'active' : 'inactive',
        device.isAssigned ? 'assigned' : 'unassigned'
      ].join(' ').toLowerCase();
      return normalized.includes(filter);
    };
    this.load();
    this.api.getAllPatients().subscribe({ next: p => this.patients.set(p) });
  }

  load(): void {
    this.loading.set(true);
    this.api.getDevices().subscribe({
      next: devices => {
        this.devices.set(devices);
        this.dataSource.data = devices;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load devices.');
        this.loading.set(false);
      }
    });
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }

  patientLabel(guid: string): string {
    const patient = this.patients().find(p => p.patientGuid === guid);
    return patient ? `${patient.firstName} ${patient.lastName}` : guid;
  }
}
