import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { DashboardSummary } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.getDashboardSummary().subscribe({
      next: value => {
        this.summary.set(value);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load dashboard data. Confirm the API and MySQL container are running.');
        this.loading.set(false);
      }
    });
  }
}
