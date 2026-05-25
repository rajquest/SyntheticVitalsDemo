import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ResetPatientDataResponse } from '../../core/models';

@Component({
  selector: 'app-admin-settings',
  imports: [CommonModule, FormsModule, MatIconModule, RouterLink],
  templateUrl: './admin-settings.component.html'
})
export class AdminSettingsComponent {
  confirmReset = false;
  resetting = signal(false);
  error = signal<string | null>(null);
  result = signal<ResetPatientDataResponse | null>(null);

  constructor(private readonly api: ApiService) {}

  resetPatientData(): void {
    if (!this.confirmReset || this.resetting()) {
      return;
    }

    this.resetting.set(true);
    this.error.set(null);
    this.result.set(null);

    this.api.resetPatientData().subscribe({
      next: response => {
        this.result.set(response);
        this.confirmReset = false;
        this.resetting.set(false);
      },
      error: () => {
        this.error.set('Unable to reset patient data. Confirm the API and database are running.');
        this.resetting.set(false);
      }
    });
  }
}
