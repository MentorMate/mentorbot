import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Chart } from 'chart.js';
import { DashboardService } from '../../dashboard.service';

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
  selector: 'app-dashboard',
  template: template,
  styles: [style]
})
export class DashboardPageComponent implements AfterViewInit {
  @ViewChild('pieAnsweredQuestions')
  chartRef: ElementRef;
  chart: Chart;

  constructor(
    private service: DashboardService) { }

  ngAfterViewInit(): void {
    this.service.getData().subscribe(next => {
      let answeredCount = 0;
      let unansweredCount = 0;
      next.forEach(it => {
        if (it.probabilityPercentage > 60) {
          answeredCount += it.count;
        } else {
          unansweredCount += it.count;
        }
      });

      this.chart = new Chart(this.chartRef.nativeElement, {
        type: 'pie',
        data: {
          labels: ['Answered', 'Unanswered'],
          datasets: [{
            data: [answeredCount, unansweredCount],
            backgroundColor: [
              'blue',
              'red',
              'yellow'
            ]
          }]
        }
      });
    });

  }
}
