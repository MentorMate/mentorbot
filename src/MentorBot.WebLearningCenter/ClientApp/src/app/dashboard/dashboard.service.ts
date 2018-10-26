import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { MessagesStatistic } from './dashboard.models';
import { Observable } from 'rxjs';

@Injectable()
export class DashboardService {
  static readonly getMessagesStatistics = 'get-messages-stats';
  constructor(
    private http: HttpClient) {
  }

  public getData(): Observable<MessagesStatistic[]> {
    return this.http.get<MessagesStatistic[]>(environment.apiPath + DashboardService.getMessagesStatistics)
  }
}
