import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { userRole, checkPath } from './auth.operations';

export abstract class AuthService {
  abstract get isLoggedIn(): boolean;
  abstract get name(): string;
  abstract get accessToken(): string;
  abstract startAuthentication(): void;
  abstract completeAuthentication(): Promise<boolean>;
  abstract signout(): void;
}

@Injectable()
export class RoleAuthService {
  static readonly getUserInfo = 'get-user-info';

  constructor(private http: HttpClient) {}

  public checkUrlAccess(url: string): Observable<boolean> {
    const storedRoleName = userRole();
    if (storedRoleName === null) {
      return this.http.get<UserInfo>(RoleAuthService.getUserInfo).pipe(
        map(it => (it.isValid ? it.role : null)),
        tap(it => userRole(it)),
        map(() => checkPath(url))
      );
    }

    return of(checkPath(url));
  }
}

interface UserInfo {
  isValid: boolean;
  role: string;
}
