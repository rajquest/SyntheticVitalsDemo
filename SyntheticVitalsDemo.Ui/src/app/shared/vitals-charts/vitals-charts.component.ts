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
        { label: 'Systolic', data: this.vitals.map(v => v.systolicBp), borderColor: '#1f6feb' },
        { label: 'Diastolic', data: this.vitals.map(v => v.diastolicBp), borderColor: '#0f766e' }
      ],
      spo2: [{ label: 'SpO2', data: this.vitals.map(v => v.spo2), borderColor: '#0f766e' }],
      hr: [{ label: 'Heart rate', data: this.vitals.map(v => v.heartRate), borderColor: '#be123c' }],
      weight: [{ label: 'Weight', data: this.vitals.map(v => v.weightLbs), borderColor: '#7c3aed' }],
      pa: [
        { label: 'PA systolic', data: this.vitals.map(v => v.paSystolic), borderColor: '#c2410c' },
        { label: 'PA diastolic', data: this.vitals.map(v => v.paDiastolic), borderColor: '#0891b2' },
        { label: 'PA mean', data: this.vitals.map(v => v.paMean), borderColor: '#4d7c0f' }
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
