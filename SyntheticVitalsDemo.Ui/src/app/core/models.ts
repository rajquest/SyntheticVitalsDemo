export interface Clinic {
  id: string;
  name: string;
  location?: string | null;
  createdAtUtc: string;
  patientCount: number;
}

export interface Patient {
  id: string;
  clinicId: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  sex: string;
  scenario: string;
  createdAtUtc: string;
  vitalsSubmissionCount: number;
}

export interface VitalsSubmission {
  id: string;
  patientId: string;
  submittedAtUtc: string;
  systolicBp: number;
  diastolicBp: number;
  spo2: number;
  heartRate: number;
  weightLbs: number;
  paSystolic: number;
  paDiastolic: number;
  paMean: number;
  scenario: string;
  notes?: string | null;
}

export interface RecentVitalsSubmission extends VitalsSubmission {
  patientName: string;
  clinicName: string;
}

export interface DashboardSummary {
  totalClinics: number;
  totalPatients: number;
  totalVitalsSubmissions: number;
  abnormalVitalsCount: number;
  recentVitalsSubmissions: RecentVitalsSubmission[];
}

export interface CreateClinicRequest {
  name: string;
  location?: string | null;
}

export interface CreatePatientRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  sex: string;
  scenario: string;
}

export interface GenerateVitalsRequest {
  submittedAtUtc?: string | null;
}

export interface GenerateVitalsSeriesRequest {
  days: 1 | 7 | 30 | 90;
  endDateUtc?: string | null;
  replaceExisting: boolean;
}

export const scenarios = [
  'Normal',
  'Hypertension',
  'Hypotension',
  'HeartFailureStable',
  'HeartFailureWorsening',
  'HeartFailureImproving',
  'LowSpo2Episode',
  'WeightGainTrend',
  'ElevatedPaPressure'
];

export const sexes = ['Female', 'Male', 'Other', 'Unknown'];
