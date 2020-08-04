import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MessagesStatistic, TimesheetChartStatistic } from './dashboard.models';
import { Observable } from 'rxjs';

@Injectable()
export class DashboardService {
  static readonly getMessagesStatistics = 'get-messages-stats';
  static readonly getTimesheetStatistics = 'get-timesheet-stats';

  constructor(private http: HttpClient) { }

  public getData(): Observable<MessagesStatistic[]> {
    return this.http.get<MessagesStatistic[]>(DashboardService.getMessagesStatistics);
  }

  public getTimesheetStats(): Observable<TimesheetChartStatistic[]> {
    return this.http.get<TimesheetChartStatistic[]>(DashboardService.getTimesheetStatistics);
  }
}
