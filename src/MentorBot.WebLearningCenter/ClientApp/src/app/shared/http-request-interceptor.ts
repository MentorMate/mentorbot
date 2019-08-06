import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { Observable } from 'rxjs';

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {
  constructor(public auth: AuthService) { }

  isExternalUrl(url: string): boolean {
    return url.startsWith('http://') || url.startsWith('https://');
  }

  path(url: string): string {
    const code = environment.azureCode === null ? '' : ('?code=' + environment.azureCode);
    return environment.apiPath + url + code;
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.isExternalUrl(request.url)) {
      return next.handle(request);
    }

    const token = this.auth.accessToken;
    const setHeaders = token !== null ? { Authorization: `Bearer ${token}` } : { };
    request = request.clone({
      url: this.path(request.url),
      setHeaders: setHeaders
    });

    return next.handle(request);
  }
}
