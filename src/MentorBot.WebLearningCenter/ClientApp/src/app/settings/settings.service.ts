import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ProcessorSettings } from './settings.models';

import { Observable } from 'rxjs';

@Injectable()
export class SettingsService {
  static readonly getSettings = 'get-settings';
  static readonly saveSettings = 'save-settings';

  constructor(
    private http: HttpClient) {
  }

  public getSettings(): Observable<ProcessorSettings[]> {
    return this.http.get<ProcessorSettings[]>(environment.apiPath + SettingsService.getSettings);
  }

  public saveSettings(settings: ProcessorSettings[]): Observable<Object> {
    return this.http.post(environment.apiPath + SettingsService.saveSettings, settings);
  }
}
