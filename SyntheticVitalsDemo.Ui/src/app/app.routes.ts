import { Routes } from '@angular/router';
import { ClinicDetailComponent } from './pages/clinic-detail/clinic-detail.component';
import { ClinicListComponent } from './pages/clinic-list/clinic-list.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { PatientDetailComponent } from './pages/patient-detail/patient-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'clinics', component: ClinicListComponent },
  { path: 'clinics/:clinicId', component: ClinicDetailComponent },
  { path: 'patients/:patientId', component: PatientDetailComponent },
  { path: '**', redirectTo: 'dashboard' }
];
