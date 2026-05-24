import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Clinic, Patient, scenarios, sexes } from '../../core/models';

@Component({
  selector: 'app-clinic-detail',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './clinic-detail.component.html'
})
export class ClinicDetailComponent implements OnInit {
  clinic = signal<Clinic | null>(null);
  patients = signal<Patient[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  scenarios = scenarios;
  sexes = sexes;
  patientForm = {
    firstName: '',
    lastName: '',
    dateOfBirth: '1970-01-01',
    sex: 'Female',
    scenario: 'Normal'
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
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load patients.');
        this.loading.set(false);
      }
    });
  }

  addPatient(): void {
    this.api.createPatient(this.clinicId, this.patientForm).subscribe({
      next: () => {
        this.patientForm = { firstName: '', lastName: '', dateOfBirth: '1970-01-01', sex: 'Female', scenario: 'Normal' };
        this.load();
      },
      error: () => this.error.set('Unable to add patient. Check required fields.')
    });
  }

  generatePatients(): void {
    this.api.generatePatients(this.clinicId, 10).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Unable to generate patients.')
    });
  }

  remove(patient: Patient): void {
    if (!confirm(`Delete ${patient.firstName} ${patient.lastName}?`)) return;
    this.api.deletePatient(patient.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Unable to delete patient.')
    });
  }
}
