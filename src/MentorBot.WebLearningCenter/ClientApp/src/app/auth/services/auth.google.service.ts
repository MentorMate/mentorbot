import { Injectable } from '@angular/core';

import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';

import { AuthService } from '../auth.service';
import { authConfig } from './auth.google.config';

function userStore(user?: any) : any {
  const key = 'mentorbot-learner-center-user';
  if (!user) {
    user = JSON.parse(sessionStorage.getItem(key));
  } else {
    sessionStorage.setItem(key, JSON.stringify(user))
  }

  return user;
}

@Injectable()
export class GoogleAuthService implements AuthService {
  private user: any;

  constructor(private oauthService: OAuthService) {
    this.oauthService.configure(authConfig);
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.user = userStore();
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  get name(): string {
    return this.user!.given_name;
  }

  public signout(): void {
    this.oauthService.logOut();
  }

  public startAuthentication(): any {
    this.oauthService.loadDiscoveryDocumentAndLogin();
  }

  public completeAuthentication(): Promise<boolean> {
    return new Promise(resolver => {
      this.oauthService.loadDiscoveryDocumentAndTryLogin().then(success => {
        if (success) {
          this.oauthService.loadUserProfile().then(user => {
            this.user = userStore(user);
            resolver(true);
          });
        }
        else {
          resolver(false);
        }
      });
    });
  }
}
