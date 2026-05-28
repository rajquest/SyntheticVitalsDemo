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

type DashboardDateRange = '7d' | '14d' | '1m' | 'ytd' | '12m';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, MatFormFieldModule, MatIconModule, MatInputModule, MatPaginatorModule, MatSortModule, MatTableModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  displayedColumns = ['submittedAtUtc', 'patientName', 'clinicName', 'scenario', 'bp', 'spo2', 'heartRate', 'weightLbs', 'seatedPulmonaryPressureDisplay', 'supinePulmonaryPressureDisplay'];
  dataSource = new MatTableDataSource<RecentVitalsSubmission>([]);
  dateRanges: { value: DashboardDateRange; label: string }[] = [
    { value: '7d', label: '7 days' },
    { value: '14d', label: '2 weeks' },
    { value: '1m', label: '1 month' },
    { value: 'ytd', label: 'YTD' },
    { value: '12m', label: '12 months' }
  ];
  selectedDateRange = signal<DashboardDateRange>('7d');
  private allSubmissions: RecentVitalsSubmission[] = [];
  private searchText = '';

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
        submission.seatedPulmonaryPressureDisplay,
        submission.supinePulmonaryPressureDisplay,
        new Date(submission.submittedAtUtc).toLocaleString()
      ].join(' ').toLowerCase();

      return normalized.includes(filter);
    };

    this.api.getDashboardSummary().subscribe({
      next: value => {
        this.summary.set(value);
        this.allSubmissions = value.recentVitalsSubmissions;
        this.applyDateRange();
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load dashboard data. Confirm the API and MySQL container are running.');
        this.loading.set(false);
      }
    });
  }

  applyFilter(value: string): void {
    this.searchText = value.trim().toLowerCase();
    this.dataSource.filter = this.searchText;
    this.dataSource.paginator?.firstPage();
  }

  applyDateRange(value: DashboardDateRange = this.selectedDateRange()): void {
    this.selectedDateRange.set(value);
    const startDate = this.getRangeStart(value);
    this.dataSource.data = this.allSubmissions.filter(submission => new Date(submission.submittedAtUtc) >= startDate);
    this.dataSource.filter = this.searchText;
    this.dataSource.paginator?.firstPage();
  }

  private getRangeStart(value: DashboardDateRange): Date {
    const now = new Date();
    const start = new Date(now);

    switch (value) {
      case '7d':
        start.setDate(now.getDate() - 7);
        break;
      case '14d':
        start.setDate(now.getDate() - 14);
        break;
      case '1m':
        start.setMonth(now.getMonth() - 1);
        break;
      case 'ytd':
        start.setMonth(0, 1);
        start.setHours(0, 0, 0, 0);
        break;
      case '12m':
        start.setFullYear(now.getFullYear() - 1);
        break;
    }

    return start;
  }
}
