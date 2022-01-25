import { Component, OnInit } from '@angular/core';
import { ChartDataset, ChartData, DefaultDataPoint, ChartType } from 'chart.js';

import { Observable } from 'rxjs';
import { take, map } from 'rxjs/operators';

import { DashboardService } from '../../dashboard.service';
import { MessagesStatistic, TimesheetChartStatistic } from '../../dashboard.models';

type MyChartData = ChartData<ChartType, DefaultDataPoint<ChartType>, string>;

function mapAnswersToDataset(stats: MessagesStatistic[]): ChartDataset[] {
  const data = stats.reduce(
    (obj, it) => {
      obj[it.probabilityPercentage > 60 ? 'answered' : 'unanswered'] += it.count;
      return obj;
    },
    { answered: 0, unanswered: 0 }
  );

  return [
    {
      data: [data.answered, data.unanswered],
      backgroundColor: ['blue', 'red', 'yellow'],
    },
  ];
}

const colors = [
  'rgb(85, 239, 196)',
  'rgb(0, 184, 148)',
  'rgb(255, 234, 167)',
  'rgb(253, 203, 110)',
  'rgb(129, 236, 236)',
  'rgb(0, 206, 201)',
  'rgb(250, 177, 160)',
  'rgb(225, 112, 85)',
  'rgb(116, 185, 255)',
  'rgb(9, 132, 227)',
  'rgb(255, 118, 117)',
  'rgb(214, 48, 49)',
  'rgb(162, 155, 254)',
  'rgb(108, 92, 231)',
  'rgb(253, 121, 168)',
  'rgb(232, 67, 147)',
  'rgb(223, 230, 233)',
  'rgb(178, 190, 195)',
  'rgb(99, 110, 114)',
  'rgb(45, 52, 54)',
];

function mapTimesheetToDatasets(stats: TimesheetChartStatistic[]): MyChartData {
  const labels: string[] = [];
  const departments: string[] = [];
  const datasets: ChartDataset[] = [];

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
    const set: ChartDataset = { label, data: [], fill: false, backgroundColor: color as string, borderColor: color as string };
    labels.forEach(dateVal => {
      const stat = stats.find(it => it.department === label && it.date === dateVal);
      const val = stat !== null && typeof stat !== 'undefined' ? stat.count : 0;
      set.data.push(val);
    });

    datasets.push(set);
  });

  return {
    labels,
    datasets,
  };
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
})
export class DashboardPageComponent implements OnInit {
  answeredLabels = ['Answered', 'Unanswered'];

  answeredData$?: Observable<ChartDataset[]>;

  timesheet$?: Observable<MyChartData>;

  constructor(private readonly _service: DashboardService) {}

  ngOnInit(): void {
    this.answeredData$ = this._service.getData().pipe(take(1), map(mapAnswersToDataset));
    this.timesheet$ = this._service.getTimesheetStats().pipe(take(1), map(mapTimesheetToDatasets));
  }
}
