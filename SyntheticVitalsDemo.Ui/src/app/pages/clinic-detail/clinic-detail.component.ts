import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Clinic, Patient, TrendScenarioOption, trendScenarioOptions } from '../../core/models';

@Component({
  selector: 'app-clinic-detail',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSliderModule
  ],
  templateUrl: './clinic-detail.component.html'
})
export class ClinicDetailComponent implements OnInit {
  clinic = signal<Clinic | null>(null);
  patients = signal<Patient[]>([]);
  loading = signal(true);
  generating = signal(false);
  deleting = signal(false);
  selectedPatientIds = signal<Set<string>>(new Set());
  error = signal<string | null>(null);
  countOptions = [1, 5, 10, 25, 50, 100] as const;
  trendDayOptions = [1, 7, 14, 30, 60, 180, 365] as const;
  trendScenarioOptions: TrendScenarioOption[] = trendScenarioOptions;
  addPatientsForm = {
    count: 10 as 1 | 5 | 10 | 25 | 50 | 100,
    malePercentage: 50,
    trendScenario: 'NormalStable',
    trendDays: 14
  };

  private clinicId = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.clinicId = this.route.snapshot.paramMap.get('clinicId') ?? '';
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getClinic(this.clinicId).subscribe({
      next: clinic => this.clinic.set(clinic),
      error: () => this.error.set('Unable to load clinic.')
    });
    this.api.getPatients(this.clinicId).subscribe({
      next: patients => {
        this.patients.set(patients);
        this.selectedPatientIds.set(new Set());
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load patients.');
        this.loading.set(false);
      }
    });
  }

  get genderMixLabel(): string {
    return `Male ${this.addPatientsForm.malePercentage}% / Female ${100 - this.addPatientsForm.malePercentage}%`;
  }

  get selectedPatientCount(): number {
    return this.selectedPatientIds().size;
  }

  allPatientsSelected(): boolean {
    const patients = this.patients();
    return patients.length > 0 && patients.every(patient => this.selectedPatientIds().has(patient.patientGuid));
  }

  somePatientsSelected(): boolean {
    const count = this.selectedPatientCount;
    return count > 0 && !this.allPatientsSelected();
  }

  toggleAllPatients(checked: boolean): void {
    this.selectedPatientIds.set(checked ? new Set(this.patients().map(patient => patient.patientGuid)) : new Set());
  }

  togglePatient(patientId: string, checked: boolean): void {
    const next = new Set(this.selectedPatientIds());
    if (checked) {
      next.add(patientId);
    } else {
      next.delete(patientId);
    }
    this.selectedPatientIds.set(next);
  }

  generatePatients(): void {
    if (!this.clinicId || this.generating()) return;
    if (!this.addPatientsForm.count) {
      this.error.set('Patient count is required.');
      return;
    }
    if (this.addPatientsForm.malePercentage < 0 || this.addPatientsForm.malePercentage > 100) {
      this.error.set('Gender mix must be between 0 and 100.');
      return;
    }
    if (!this.addPatientsForm.trendScenario) {
      this.error.set('Vitals trend scenario is required.');
      return;
    }
    if (![1, 7, 14, 30, 60, 180, 365].includes(this.addPatientsForm.trendDays)) {
      this.error.set('Trend readings must be 1, 7, 14, 30, 60, 180, or 365.');
      return;
    }

    this.generating.set(true);
    this.error.set(null);
    this.api.generatePatients(this.clinicId, {
      count: this.addPatientsForm.count,
      malePercentage: this.addPatientsForm.malePercentage,
      vitalsTrendScenario: this.addPatientsForm.trendScenario,
      trendDays: this.addPatientsForm.trendDays
    }).subscribe({
      next: response => {
        this.patients.set(response.patients);
        const clinic = this.clinic();
        if (clinic) {
          this.clinic.set({ ...clinic, patientCount: response.updatedPatientCount });
        }
        this.generating.set(false);
        this.load();
      },
      error: () => {
        this.error.set('Unable to add synthetic patients.');
        this.generating.set(false);
      }
    });
  }

  deleteSelectedPatients(): void {
    const selectedIds = [...this.selectedPatientIds()];
    if (selectedIds.length === 0 || this.deleting()) return;
    if (!confirm(`Delete ${selectedIds.length} selected synthetic patient${selectedIds.length === 1 ? '' : 's'}?`)) return;

    this.deleting.set(true);
    this.error.set(null);
    forkJoin(selectedIds.map(id => this.api.deletePatient(id))).subscribe({
      next: () => {
        this.deleting.set(false);
        this.load();
      },
      error: () => {
        this.error.set('Unable to delete selected patients.');
        this.deleting.set(false);
      }
    });
  }
}
