import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { VitalsSubmission } from '../../core/models';

@Component({
  selector: 'app-vitals-charts',
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './vitals-charts.component.html'
})
export class VitalsChartsComponent {
  @Input({ required: true }) vitals: VitalsSubmission[] = [];

  chartData(kind: 'bp' | 'spo2' | 'hr' | 'weight' | 'pa'): ChartConfiguration<'line'>['data'] {
    const series = {
      bp: [
        { label: 'Systolic', data: this.vitals.map(v => v.systolicBp), borderColor: '#000000' },
        { label: 'Diastolic', data: this.vitals.map(v => v.diastolicBp), borderColor: '#898D8D', borderDash: [6, 4] }
      ],
      spo2: [{ label: 'SpO2', data: this.vitals.map(v => v.spo2), borderColor: '#000000' }],
      hr: [{ label: 'Heart rate', data: this.vitals.map(v => v.heartRate), borderColor: '#333333' }],
      weight: [{ label: 'Weight', data: this.vitals.map(v => v.weightLbs), borderColor: '#898D8D' }],
      pa: [
        { label: 'Seated PA systolic', data: this.vitals.map(v => v.seatedPaSystolic), borderColor: '#000000' },
        { label: 'Seated PA diastolic', data: this.vitals.map(v => v.seatedPaDiastolic), borderColor: '#898D8D', borderDash: [6, 4] },
        { label: 'Seated PA mean', data: this.vitals.map(v => v.seatedPaMean), borderColor: '#333333', borderDash: [2, 4] },
        { label: 'Supine PA systolic', data: this.vitals.map(v => v.supinePaSystolic), borderColor: '#006D77' },
        { label: 'Supine PA diastolic', data: this.vitals.map(v => v.supinePaDiastolic), borderColor: '#E29578', borderDash: [6, 4] },
        { label: 'Supine PA mean', data: this.vitals.map(v => v.supinePaMean), borderColor: '#7A3E9D', borderDash: [2, 4] }
      ]
    }[kind];

    return {
      labels: this.vitals.map(x => new Date(x.submittedAtUtc).toLocaleDateString()),
      datasets: series.map(item => ({
        ...item,
        tension: 0.25,
        pointRadius: this.vitals.length > 45 ? 0 : 2,
        borderWidth: 2
      }))
    };
  }

  options(title: string): ChartConfiguration<'line'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'bottom' },
        title: { display: true, text: title }
      },
      scales: {
        x: { ticks: { maxTicksLimit: 8 } }
      }
    };
  }
}
