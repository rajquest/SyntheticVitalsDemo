import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  Clinic,
  CreateClinicRequest,
  CreatePatientRequest,
  DashboardSummary,
  GenerateVitalsRequest,
  GenerateVitalsSeriesRequest,
  Patient,
  VitalsSubmission
} from './models';

const apiBaseUrl = 'http://localhost:5217/api';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  getDashboardSummary() {
    return this.http.get<DashboardSummary>(`${apiBaseUrl}/dashboard/summary`);
  }

  getClinics() {
    return this.http.get<Clinic[]>(`${apiBaseUrl}/clinics`);
  }

  getClinic(id: string) {
    return this.http.get<Clinic>(`${apiBaseUrl}/clinics/${id}`);
  }

  createClinic(request: CreateClinicRequest) {
    return this.http.post<Clinic>(`${apiBaseUrl}/clinics`, request);
  }

  updateClinic(id: string, request: CreateClinicRequest) {
    return this.http.put<Clinic>(`${apiBaseUrl}/clinics/${id}`, request);
  }

  deleteClinic(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/clinics/${id}`);
  }

  getPatients(clinicId: string) {
    return this.http.get<Patient[]>(`${apiBaseUrl}/clinics/${clinicId}/patients`);
  }

  getPatient(id: string) {
    return this.http.get<Patient>(`${apiBaseUrl}/patients/${id}`);
  }

  createPatient(clinicId: string, request: CreatePatientRequest) {
    return this.http.post<Patient>(`${apiBaseUrl}/clinics/${clinicId}/patients`, request);
  }

  generatePatients(clinicId: string, count: number) {
    return this.http.post<Patient[]>(`${apiBaseUrl}/clinics/${clinicId}/generate-patients`, { count });
  }

  updatePatient(id: string, request: CreatePatientRequest) {
    return this.http.put<Patient>(`${apiBaseUrl}/patients/${id}`, request);
  }

  deletePatient(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/patients/${id}`);
  }

  getVitals(patientId: string) {
    return this.http.get<VitalsSubmission[]>(`${apiBaseUrl}/patients/${patientId}/vitals`);
  }

  generateVitals(patientId: string, request: GenerateVitalsRequest = {}) {
    return this.http.post<VitalsSubmission>(`${apiBaseUrl}/patients/${patientId}/generate-vitals`, request);
  }

  generateVitalsSeries(patientId: string, request: GenerateVitalsSeriesRequest) {
    return this.http.post<VitalsSubmission[]>(`${apiBaseUrl}/patients/${patientId}/generate-vitals-series`, request);
  }

  deleteVitals(patientId: string) {
    return this.http.delete<void>(`${apiBaseUrl}/patients/${patientId}/vitals`);
  }
}
