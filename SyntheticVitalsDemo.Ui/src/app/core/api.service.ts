import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  Clinic,
  CreatePatientRequest,
  DashboardSummary,
  GeneratePatientsRequest,
  GeneratePatientsResponse,
  GenerateVitalsRequest,
  GenerateVitalsSeriesRequest,
  Patient,
  ResetPatientDataResponse,
  VitalsSubmission
} from './models';

const apiBaseUrl = 'http://localhost:7000/api';

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

  getPatients(clinicId: string) {
    return this.http.get<Patient[]>(`${apiBaseUrl}/clinics/${clinicId}/patients`);
  }

  getAllPatients() {
    return this.http.get<Patient[]>(`${apiBaseUrl}/patients`);
  }

  getPatient(id: string) {
    return this.http.get<Patient>(`${apiBaseUrl}/patients/${id}`);
  }

  createPatient(clinicId: string, request: CreatePatientRequest) {
    return this.http.post<Patient>(`${apiBaseUrl}/clinics/${clinicId}/patients`, request);
  }

  generatePatients(clinicId: string, request: GeneratePatientsRequest) {
    return this.http.post<GeneratePatientsResponse>(`${apiBaseUrl}/clinics/${clinicId}/patients/generate`, request);
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

  getVitalsSubmissionHl7(submissionId: string) {
    return this.http.get(`${apiBaseUrl}/export/vitals/${submissionId}/hl7`, { responseType: 'text' });
  }

  getVitalsSubmissionFhir(submissionId: string) {
    return this.http.get(`${apiBaseUrl}/export/vitals/${submissionId}/fhir`, { responseType: 'text' });
  }

  resetPatientData() {
    return this.http.delete<ResetPatientDataResponse>(`${apiBaseUrl}/admin/patient-data`);
  }
}
