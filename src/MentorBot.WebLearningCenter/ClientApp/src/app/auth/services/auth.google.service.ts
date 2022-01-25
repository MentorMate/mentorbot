import { Injectable } from '@angular/core';

import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';

import { AuthService } from '../auth.service';
import { authConfig } from './auth.google.config';
import { userRole, userInfo, UserInfo } from '../auth.operations';

@Injectable()
export class GoogleAuthService implements AuthService {
  private user: UserInfo | null;

  constructor(private oauthService: OAuthService) {
    this.oauthService.configure(authConfig);
    //this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.user = userInfo();
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  get name(): string {
    return this.user === null ? '' : this.user.given_name;
  }

  get accessToken(): string {
    return this.oauthService.getAccessToken();
  }

  public signout(): void {
    userRole(null);
    userInfo(null);
    this.oauthService.logOut();
  }

  public startAuthentication(): any {
    this.oauthService.loadDiscoveryDocumentAndLogin();
  }

  public completeAuthentication(): Promise<boolean> {
    return new Promise(resolver => {
      this.oauthService.loadDiscoveryDocumentAndTryLogin().then(success => {
        if (success) {
          this.oauthService.loadUserProfile().then((user: object) => {
            this.user = userInfo(user as UserInfo);
            resolver(true);
          });
        } else {
          resolver(false);
        }
      });
    });
  }
}
