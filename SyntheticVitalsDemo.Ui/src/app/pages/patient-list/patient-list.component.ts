import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { ApiService } from '../../core/api.service';
import { Patient } from '../../core/models';

@Component({
  selector: 'app-patient-list',
  imports: [
    CommonModule,
    RouterLink,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule
  ],
  templateUrl: './patient-list.component.html'
})
export class PatientListComponent implements OnInit {
  patients = signal<Patient[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  displayedColumns = ['name', 'dateOfBirth', 'sex', 'scenario', 'bp', 'spo2', 'heartRate', 'weightLbs', 'seatedPulmonaryPressureDisplay', 'supinePulmonaryPressureDisplay', 'vitalsSubmissionCount'];
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

  constructor(private readonly api: ApiService) {}

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
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }
}
