export interface Clinic {
  id: string;
  name: string;
  createdAtUtc: string;
  patientCount: number;
  submissionCount: number;
}

export interface Patient {
  id: string;
  clinicId: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  sex: string;
  scenario: string;
  systolicBp: number;
  diastolicBp: number;
  bloodPressureDisplay: string;
  spo2: number;
  heartRate: number;
  weightLbs: number;
  paSystolic: number;
  paDiastolic: number;
  paMean: number;
  pulmonaryPressureDisplay: string;
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
  pulmonaryPressureDisplay: string;
  scenario: string;
  trendScenario: string;
  notes?: string | null;
}

export interface RecentVitalsSubmission extends VitalsSubmission {
  patientName: string;
  clinicName: string;
  pulmonaryPressureDisplay: string;
}

export interface DashboardSummary {
  totalClinics: number;
  totalPatients: number;
  totalVitalsSubmissions: number;
  abnormalVitalsCount: number;
  recentVitalsSubmissions: RecentVitalsSubmission[];
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
  days: 7 | 14 | 30;
  endDateUtc?: string | null;
  replaceExisting: boolean;
  pulmonaryPressureScenario: string;
}

export interface GeneratePatientsRequest {
  count: 5 | 10 | 25 | 50 | 100;
  malePercentage: number;
  pulmonaryPressureScenario: string;
  trendDays: number;
}

export interface GeneratePatientsResponse {
  clinicId: string;
  generatedCount: number;
  updatedPatientCount: number;
  patients: Patient[];
}

export interface ResetPatientDataResponse {
  deletedVitalsSubmissions: number;
  deletedPatients: number;
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

export const pulmonaryPressureScenarios = [
  { value: 'NormalPaPressure', label: 'Normal PA Pressure' },
  { value: 'MildPulmonaryHypertension', label: 'Mild Pulmonary Hypertension' },
  { value: 'ModeratePulmonaryHypertension', label: 'Moderate Pulmonary Hypertension' },
  { value: 'SeverePulmonaryHypertension', label: 'Severe Pulmonary Hypertension' },
  { value: 'ElevatedPaDiastolicPressure', label: 'Elevated PA Diastolic Pressure' },
  { value: 'HighPaMeanPressure', label: 'High PA Mean Pressure' },
  { value: 'MixedPulmonaryPressureVariability', label: 'Mixed Pulmonary Pressure Variability' }
];

export const pulmonaryPressureTrendScenarios = [
  { value: 'NormalStable', label: 'Normal and stable' },
  { value: 'MildlyElevatedStable', label: 'Mildly elevated but stable' },
  { value: 'ProgressivelyWorseningPaMean', label: 'Progressively worsening PA mean' },
  { value: 'SuddenPaPressureSpike', label: 'Sudden PA pressure spike' },
  { value: 'ImprovingAfterDiureticAdjustment', label: 'Improving after diuretic adjustment' },
  { value: 'PersistentlyHighPaDiastolic', label: 'Persistently high PA diastolic' }
];

export const sexes = ['Female', 'Male', 'Other', 'Unknown'];
