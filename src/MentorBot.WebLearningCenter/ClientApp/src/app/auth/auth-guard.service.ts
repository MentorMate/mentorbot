import { Injectable } from '@angular/core';
import {
  CanActivate,
  CanActivateChild,
  CanLoad,
  Route,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router
} from '@angular/router';

import { AuthService, RoleAuthService } from './auth.service';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {
  constructor(
    private auth: AuthService,
    private roleAuth: RoleAuthService,
    private router: Router) {
  }

  public canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> {
    return this.IsAuthenticated(state.url);
  }

  public canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> {
    return this.IsAuthenticated(state.url);
  }

  public canLoad(route: Route): boolean {
    return true;
  }

  private IsAuthenticated(url: string): Observable<boolean> {
    var auth = this.auth.isLoggedIn;
    if (!auth) {
      this.auth.startAuthentication();
      return of(false);
    }

    return this.roleAuth
      .checkUrlAccess(url)
      .pipe(tap(valid => {
        if (!valid) {
          this.router.navigateByUrl('/app/no-access');
        }
      }));
  }
}
