import { Component, OnInit, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { DashboardSummary, RecentVitalsSubmission } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, MatFormFieldModule, MatIconModule, MatInputModule, MatPaginatorModule, MatSortModule, MatTableModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  displayedColumns = ['submittedAtUtc', 'patientName', 'clinicName', 'scenario', 'bp', 'spo2', 'heartRate', 'weightLbs', 'pulmonaryPressureDisplay'];
  dataSource = new MatTableDataSource<RecentVitalsSubmission>([]);

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
    this.dataSource.filterPredicate = (submission, filter) => {
      const normalized = [
        submission.patientName,
        submission.clinicName,
        submission.scenario,
        `${submission.systolicBp}/${submission.diastolicBp}`,
        submission.spo2.toString(),
        submission.heartRate.toString(),
        submission.weightLbs.toString(),
        submission.pulmonaryPressureDisplay,
        new Date(submission.submittedAtUtc).toLocaleString()
      ].join(' ').toLowerCase();

      return normalized.includes(filter);
    };

    this.api.getDashboardSummary().subscribe({
      next: value => {
        this.summary.set(value);
        this.dataSource.data = value.recentVitalsSubmissions;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load dashboard data. Confirm the API and MySQL container are running.');
        this.loading.set(false);
      }
    });
  }

  applyFilter(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.dataSource.paginator?.firstPage();
  }

}
