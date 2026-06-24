export interface Clinic {
  id: string;
  name: string;
  siteId?: string | null;
  createdAtUtc: string;
  patientCount: number;
  submissionCount: number;
}

export interface Patient {
  patientGuid: string;
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
  seatedPaSystolic: number;
  seatedPaDiastolic: number;
  seatedPaMean: number;
  supinePaSystolic: number;
  supinePaDiastolic: number;
  supinePaMean: number;
  seatedPulmonaryPressureDisplay: string;
  supinePulmonaryPressureDisplay: string;
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
  seatedPaSystolic: number;
  seatedPaDiastolic: number;
  seatedPaMean: number;
  supinePaSystolic: number;
  supinePaDiastolic: number;
  supinePaMean: number;
  seatedPulmonaryPressureDisplay: string;
  supinePulmonaryPressureDisplay: string;
  scenario: string;
  trendScenario: string;
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

export interface Device {
  deviceType: string;
  deviceId: string;
  imeiNumber?: string | null;
  bluetoothAddress?: string | null;
  dateTimeCreated: string;
  dateTimeLastUpdated: string;
  dateTimeDeactivated?: string | null;
  dateTimePatientAssigned?: string | null;
  patientGuid?: string | null;
  isActive: boolean;
  isAssigned: boolean;
}

export interface CreateDeviceRequest {
  deviceType: string;
  deviceId: string;
  imeiNumber?: string | null;
  bluetoothAddress?: string | null;
}

export interface UpdateDeviceRequest {
  imeiNumber?: string | null;
  bluetoothAddress?: string | null;
}

export interface AssignDeviceRequest {
  patientGuid: string;
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
  days: 2 | 7 | 14 | 30 | 60 | 180 | 365;
  endDateUtc?: string | null;
  replaceExisting: boolean;
  vitalsTrendScenario: string;
}

export interface GeneratePatientsRequest {
  count: 1 | 5 | 10 | 25 | 50 | 100;
  malePercentage: number;
  vitalsTrendScenario: string;
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

export interface TrendScenarioOption {
  value: string;
  label: string;
}

export const trendScenarioOptions: TrendScenarioOption[] = [
  { value: 'NormalStable',              label: 'Normal (Stable)' },
  { value: 'ChronicHfStable',           label: 'Chronic HF (Stable High)' },
  { value: 'EarlyFluidOverload',        label: 'Early Fluid Overload (Slow Rise)' },
  { value: 'AcuteHfDecompensation',     label: 'Acute HF Decompensation (Rapid Rise)' },
  { value: 'DiuresisTreatmentResponse', label: 'Diuresis / Treatment Response (Improving)' },
  { value: 'ExerciseActivitySpike',     label: 'Exercise / Activity Spike (Temporary Spike)' }
];

export const sexes = ['Female', 'Male', 'Other', 'Unknown'];
