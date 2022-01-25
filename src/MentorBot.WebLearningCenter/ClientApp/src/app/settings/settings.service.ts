import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Plugin } from './settings.models';

import { Observable } from 'rxjs';

@Injectable()
export class SettingsService {
  static readonly getPlugins = 'get-plugins';
  static readonly savePlugins = 'save-plugins';

  constructor(private http: HttpClient) {}

  public getPlugins(): Observable<Plugin[]> {
    return this.http.get<Plugin[]>(SettingsService.getPlugins);
  }

  public savePlugins(plugins: Plugin[]): Observable<Object> {
    return this.http.post(SettingsService.savePlugins, plugins);
  }
}
