import { Component, OnInit } from '@angular/core';
import { ChartDataSets, ChartData } from 'chart.js';

import { Observable } from 'rxjs';
import { take, map } from 'rxjs/operators';

import { DashboardService } from '../../dashboard.service';
import { MessagesStatistic, TimesheetChartStatistic } from '../../dashboard.models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss']
})
export class DashboardPageComponent implements OnInit {
  answeredLabels = ['Answered', 'Unanswered'];
  answeredData$: Observable<ChartDataSets[]>;

  timesheet$: Observable<ChartData>;

  constructor(private readonly _service: DashboardService) { }

  ngOnInit(): void {
    this.answeredData$ = this._service.getData().pipe(take(1), map(mapAnswersToDataset));
    this.timesheet$ = this._service.getTimesheetStats().pipe(take(1), map(mapTimesheetToDatasets));
  }
}

function mapAnswersToDataset(stats: MessagesStatistic[]): ChartDataSets[] {
  const data = stats.reduce((obj, it) => {
    obj[it.probabilityPercentage > 60 ? 'answered' : 'unanswered'] += it.count;
    return obj;
  }, { answered: 0, unanswered: 0 });

  return [{
    data: [data.answered, data.unanswered],
    backgroundColor: [
      'blue',
      'red',
      'yellow'
    ]
  }];
}

const colors = [
  'rgb(255, 99, 132)', // red
  'rgb(54, 162, 235)', // blue
  'rgb(255, 159, 64)', // orange
  'rgb(75, 192, 192)', // green
  'rgb(153, 102, 255)', // purple
  'rgb(255, 205, 86)', // yellow
  'rgb(201, 203, 207)', // gray
  'rgb(255, 20, 147)', // pink
  'rgb(128, 0, 0)', // maroon
  'rgb(0, 0, 128)' // navy
];


function mapTimesheetToDatasets(stats: TimesheetChartStatistic[]): ChartData {
  const labels = [];
  const departments = [];
  const datasets: ChartDataSets[] = [];

  stats.forEach(it => {
    if (!labels.includes(it.date)) {
      labels.push(it.date);
    }

    if (!departments.includes(it.department)) {
      departments.push(it.department);
    }
  });

  departments.forEach((label: string, index: number) => {
    const color = colors.length > index ? colors[index] : null;
    const set: ChartDataSets = { label, data: [], fill: false, backgroundColor: color, borderColor: color };
    labels.forEach(dateVal => {
      const stat = stats.find(it => it.department === label && it.date === dateVal);
      const val = stat !== null && typeof stat !== 'undefined' ? stat.count : 0;
      set.data.push(val);
    });

    datasets.push(set);
  });

  return {
    labels,
    datasets
  };
}
