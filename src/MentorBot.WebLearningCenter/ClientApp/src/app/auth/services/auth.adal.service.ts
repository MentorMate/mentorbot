import { Injectable } from '@angular/core';

import { Adal5Service } from 'adal-angular5';

import { environment } from '../../../environments/environment';
import { AuthService } from '../auth.service';

@Injectable()
export class AdalAuthService implements AuthService {
  private _user = null;
  private _expireIn: number;

  constructor(private _adal: Adal5Service) {
    this._adal.init(environment.azure);
  }

  get isLoggedIn(): boolean {
    return this._adal.userInfo.authenticated;
  }

  get name(): string {
    return this._user.profile.name;
  }

  public signout(): void {
    this._adal.logOut();
  }

  public startAuthentication(): any {
    this._adal.login();
  }

  public completeAuthentication(): void {
    this._adal.handleWindowCallback();
    this._adal.getUser().subscribe(user => {
      this._user = user;
      this._expireIn = user.profile.exp - new Date().getTime();
    });
  }
}
