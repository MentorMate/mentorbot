import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Chart } from 'chart.js';

const template = `
  Dashboard
  <canvas #pieAnsweredQuestions></canvas>
`;

const style = `
  :host {
    display: block;
  }

  canvas { max-width: 800px; }
`;

@Component({
  selector: 'lp-dashboard',
  template: template,
  styles: [style]
})
export class DashboardPageComponent implements AfterViewInit {
  @ViewChild('pieAnsweredQuestions')
  chartRef: ElementRef;
  chart: Chart;

  ngAfterViewInit(): void {
    this.chart = new Chart(this.chartRef.nativeElement, {
      type: 'pie',
      data: {
        labels: ['Answered', 'Unanswered'],
        datasets: [{
          data: [40, 60],
          backgroundColor: [
            'blue',
            'red'
          ]
        }]
      }
    });
  }
}
