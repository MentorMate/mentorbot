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
import { Observable } from 'rxjs/Observable';

import { AuthService } from './auth.service';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {
  constructor(
    private auth: AuthService,
    private router: Router) {
  }

  public canActivate(): boolean {
    return this.IsAuthenticated();
  }

  public canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    return this.IsAuthenticated();
  }

  public canLoad(route: Route): boolean {
    return this.IsAuthenticated();
  }

  private IsAuthenticated(): boolean {
    var auth = this.auth.isLoggedIn;
    if (!auth) {
      this.auth.startAuthentication();
    }

    return auth;
  }
}
