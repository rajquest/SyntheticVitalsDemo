import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Patient, pulmonaryPressureTrendScenarios, VitalsSubmission } from '../../core/models';
import { PayloadDialogComponent } from '../../shared/payload-dialog/payload-dialog.component';
import { VitalsChartsComponent } from '../../shared/vitals-charts/vitals-charts.component';

type SubmissionPayloadFormat = 'hl7' | 'fhir';
type PatientVitalsDateRange = '7d' | '14d' | '1m' | 'ytd' | '12m';

@Component({
  selector: 'app-patient-detail',
  imports: [CommonModule, FormsModule, MatDialogModule, MatIconModule, MatProgressSpinnerModule, RouterLink, VitalsChartsComponent],
  templateUrl: './patient-detail.component.html'
})
export class PatientDetailComponent implements OnInit {
  patient = signal<Patient | null>(null);
  vitals = signal<VitalsSubmission[]>([]);
  filteredVitals = signal<VitalsSubmission[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  dateRanges: { value: PatientVitalsDateRange; label: string }[] = [
    { value: '7d', label: '7 days' },
    { value: '14d', label: '2 weeks' },
    { value: '1m', label: '1 month' },
    { value: 'ytd', label: 'YTD' },
    { value: '12m', label: '12 months' }
  ];
  selectedDateRange = signal<PatientVitalsDateRange>('7d');
  seriesDayOptions = [7, 14, 30, 60, 180, 365] as const;
  selectedDays: 7 | 14 | 30 | 60 | 180 | 365 = 14;
  pulmonaryPressureTrendScenarios = pulmonaryPressureTrendScenarios;
  selectedPulmonaryPressureScenario = 'NormalStable';
  replaceExisting = true;
  generatingSeries = signal(false);
  clearingVitals = signal(false);
  payloadLoading = signal(false);

  private patientId = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ApiService,
    private readonly dialog: MatDialog) {}

  get latestFilteredVitals(): VitalsSubmission | null {
    const values = this.filteredVitals();
    return values.length === 0 ? null : values[values.length - 1];
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('patientId') ?? '';
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getPatient(this.patientId).subscribe({
      next: patient => this.patient.set(patient),
      error: () => this.error.set('Unable to load patient.')
    });
    this.api.getVitals(this.patientId).subscribe({
      next: vitals => {
        this.vitals.set(vitals);
        this.applyDateRange();
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load vitals.');
        this.loading.set(false);
      }
    });
  }

  generateSeries(): void {
    if (this.generatingSeries()) return;
    this.generatingSeries.set(true);
    this.api.generateVitalsSeries(this.patientId, {
      days: this.selectedDays,
      replaceExisting: this.replaceExisting,
      pulmonaryPressureScenario: this.selectedPulmonaryPressureScenario
    }).subscribe({
      next: () => {
        this.generatingSeries.set(false);
        this.load();
      },
      error: () => {
        this.error.set('Unable to generate vitals series.');
        this.generatingSeries.set(false);
      }
    });
  }

  printReport(): void {
    setTimeout(() => window.print(), 100);
  }

  applyDateRange(value: PatientVitalsDateRange = this.selectedDateRange()): void {
    this.selectedDateRange.set(value);
    const startDate = this.getRangeStart(value);
    this.filteredVitals.set(this.vitals().filter(submission => new Date(submission.submittedAtUtc) >= startDate));
  }

  clearVitals(): void {
    if (this.clearingVitals() || !confirm('Delete all vitals for this synthetic patient?')) return;
    this.clearingVitals.set(true);
    this.api.deleteVitals(this.patientId).subscribe({
      next: () => {
        this.clearingVitals.set(false);
        this.load();
      },
      error: () => {
        this.error.set('Unable to delete vitals.');
        this.clearingVitals.set(false);
      }
    });
  }

  viewPayload(submission: VitalsSubmission, format: SubmissionPayloadFormat): void {
    this.payloadLoading.set(true);
    const request = format === 'hl7'
      ? this.api.getVitalsSubmissionHl7(submission.id)
      : this.api.getVitalsSubmissionFhir(submission.id);

    request.subscribe({
      next: payload => {
        this.payloadLoading.set(false);
        this.dialog.open(PayloadDialogComponent, {
          data: {
            title: `${format.toUpperCase()} submission`,
            payload: format === 'hl7' ? payload.replace(/\r\n|\r|\n/g, '\n') : payload,
            fileName: `synthetic-vitals-${submission.id}.${format === 'hl7' ? 'txt' : 'json'}`,
            contentType: format === 'hl7' ? 'text/plain' : 'application/fhir+json'
          },
          width: 'min(1100px, 96vw)',
          maxWidth: '96vw'
        });
      },
      error: () => {
        this.error.set(`Unable to load ${format.toUpperCase()} payload.`);
        this.payloadLoading.set(false);
      }
    });
  }

  private getRangeStart(value: PatientVitalsDateRange): Date {
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
