import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Clinic } from '../../core/models';

@Component({
  selector: 'app-clinic-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './clinic-list.component.html'
})
export class ClinicListComponent implements OnInit {
  clinics = signal<Clinic[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  form = { name: '', location: '' };
  editingId = signal<string | null>(null);

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getClinics().subscribe({
      next: clinics => {
        this.clinics.set(clinics);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load clinics.');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (!this.form.name.trim()) {
      this.error.set('Clinic name is required.');
      return;
    }

    const request = { name: this.form.name.trim(), location: this.form.location.trim() || null };
    const editingId = this.editingId();
    const save = editingId ? this.api.updateClinic(editingId, request) : this.api.createClinic(request);
    save.subscribe({
      next: () => {
        this.form = { name: '', location: '' };
        this.editingId.set(null);
        this.error.set(null);
        this.load();
      },
      error: () => this.error.set('Unable to save clinic.')
    });
  }

  edit(clinic: Clinic): void {
    this.editingId.set(clinic.id);
    this.form = { name: clinic.name, location: clinic.location ?? '' };
  }

  remove(clinic: Clinic): void {
    if (!confirm(`Delete ${clinic.name}?`)) return;
    this.api.deleteClinic(clinic.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Unable to delete clinic.')
    });
  }
}
