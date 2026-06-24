import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/api.service';
import { Device, Patient } from '../../core/models';
import { DeviceAssignDialogComponent } from '../../shared/device-assign-dialog/device-assign-dialog.component';

@Component({
  selector: 'app-patient-list',
  imports: [
    CommonModule,
    RouterLink,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule
  ],
  templateUrl: './patient-list.component.html'
})
export class PatientListComponent implements OnInit {
  patients = signal<Patient[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  patientDevices = signal<Map<string, Device[]>>(new Map());
  displayedColumns = ['name', 'dateOfBirth', 'sex', 'scenario', 'bp', 'spo2', 'heartRate', 'weightLbs', 'seatedPulmonaryPressureDisplay', 'supinePulmonaryPressureDisplay', 'vitalsSubmissionCount', 'device'];
  dataSource = new MatTableDataSource<Patient>([]);

  @ViewChild(MatPaginator) set paginator(value: MatPaginator | undefined) {
    if (value) {
      this.dataSource.paginator = value;
    }
  }

  @ViewChild(MatSort) set sort(value: MatSort | undefined) {
    if (value) {
      this.dataSource.sort = value;
    }
  }

  constructor(
    private readonly api: ApiService,
    private readonly dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.dataSource.filterPredicate = (patient, filter) => {
      const normalized = [
        patient.firstName,
        patient.lastName,
        patient.dateOfBirth,
        patient.sex,
        patient.scenario,
        patient.bloodPressureDisplay,
        patient.spo2.toString(),
        patient.heartRate.toString(),
        patient.weightLbs.toString(),
        patient.seatedPulmonaryPressureDisplay,
        patient.supinePulmonaryPressureDisplay,
        patient.vitalsSubmissionCount.toString()
      ].join(' ').toLowerCase();

      return normalized.includes(filter);
    };
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getAllPatients().subscribe({
      next: patients => {
        this.patients.set(patients);
        this.dataSource.data = patients;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load patients.');
        this.loading.set(false);
      }
    });
    this.loadDevices();
  }

  loadDevices(): void {
    this.api.getDevices().subscribe({
      next: devices => {
        const map = new Map<string, Device[]>();
        for (const d of devices) {
          if (d.isAssigned && d.patientGuid) {
            if (!map.has(d.patientGuid)) map.set(d.patientGuid, []);
            map.get(d.patientGuid)!.push(d);
          }
        }
        this.patientDevices.set(map);
      }
    });
  }

  getAssignedDevices(patientGuid: string): Device[] {
    return this.patientDevices().get(patientGuid) ?? [];
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }

  openDeviceDialog(patient: Patient): void {
    const ref = this.dialog.open(DeviceAssignDialogComponent, {
      data: { patient },
      width: '580px',
      maxWidth: '96vw'
    });
    ref.afterClosed().subscribe(result => {
      if (result?.changed) this.loadDevices();
    });
  }
}
