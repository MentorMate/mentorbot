import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Settings } from './settings.models';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

@Injectable()
export class SettingsService {
  static readonly getSettings = 'settings';

  constructor(
    private http: HttpClient) {
  }

  public getSettings(): Observable<Settings> {
    return this.http.get<Settings>(environment.apiPath + SettingsService.getSettings);
  }

  public saveSettings(settings: Settings): Observable<Settings> {
    return this.http.post<Settings>(environment.apiPath + SettingsService.getSettings, settings);
  }

  /**
   * Handle Http operation that failed.
   * Let the app continue.
   * @param operation - name of the operation that failed
   * @param result - optional value to return as the observable result
   */
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {

      // TODO: send the error to remote logging infrastructure
      console.error(error); // log to console instead

      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }
}
