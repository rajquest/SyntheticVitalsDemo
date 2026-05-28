import { Routes } from '@angular/router';
import { ClinicDetailComponent } from './pages/clinic-detail/clinic-detail.component';
import { ClinicListComponent } from './pages/clinic-list/clinic-list.component';
import { AdminSettingsComponent } from './pages/admin-settings/admin-settings.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { PatientListComponent } from './pages/patient-list/patient-list.component';
import { PatientDetailComponent } from './pages/patient-detail/patient-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'clinics', component: ClinicListComponent },
  { path: 'clinics/:clinicId', component: ClinicDetailComponent },
  { path: 'patients', component: PatientListComponent },
  { path: 'patients/:patientId', component: PatientDetailComponent },
  { path: 'admin-settings', component: AdminSettingsComponent },
  { path: '**', redirectTo: 'dashboard' }
];
