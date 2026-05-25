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
import { Clinic } from '../../core/models';

@Component({
  selector: 'app-clinic-list',
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
  templateUrl: './clinic-list.component.html'
})
export class ClinicListComponent implements OnInit {
  clinics = signal<Clinic[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  displayedColumns = ['name', 'patientCount', 'submissionCount', 'createdAtUtc'];
  dataSource = new MatTableDataSource<Clinic>([]);

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
    this.dataSource.filterPredicate = (clinic, filter) => {
      const normalized = [
        clinic.name,
        clinic.patientCount.toString(),
        clinic.submissionCount.toString(),
        new Date(clinic.createdAtUtc).toLocaleDateString()
      ].join(' ').toLowerCase();

      return normalized.includes(filter);
    };
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getClinics().subscribe({
      next: clinics => {
        this.clinics.set(clinics);
        this.dataSource.data = clinics;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load clinics.');
        this.loading.set(false);
      }
    });
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }
}
