import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Patient, pulmonaryPressureTrendScenarios, VitalsSubmission } from '../../core/models';
import { VitalsChartsComponent } from '../../shared/vitals-charts/vitals-charts.component';

@Component({
  selector: 'app-patient-detail',
  imports: [CommonModule, FormsModule, RouterLink, VitalsChartsComponent],
  templateUrl: './patient-detail.component.html'
})
export class PatientDetailComponent implements OnInit {
  patient = signal<Patient | null>(null);
  vitals = signal<VitalsSubmission[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  selectedDays: 7 | 14 | 30 = 14;
  pulmonaryPressureTrendScenarios = pulmonaryPressureTrendScenarios;
  selectedPulmonaryPressureScenario = 'NormalStable';
  replaceExisting = true;

  private patientId = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

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
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load vitals.');
        this.loading.set(false);
      }
    });
  }

  generateSeries(): void {
    this.api.generateVitalsSeries(this.patientId, {
      days: this.selectedDays,
      replaceExisting: this.replaceExisting,
      pulmonaryPressureScenario: this.selectedPulmonaryPressureScenario
    }).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Unable to generate vitals series.')
    });
  }

  clearVitals(): void {
    if (!confirm('Delete all vitals for this synthetic patient?')) return;
    this.api.deleteVitals(this.patientId).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Unable to delete vitals.')
    });
  }
}
