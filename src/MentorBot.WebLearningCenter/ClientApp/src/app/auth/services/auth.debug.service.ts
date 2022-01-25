import { Injectable } from '@angular/core';
import { AuthService } from '../auth.service';

@Injectable()
export class DebugAuthService implements AuthService {
  get isLoggedIn(): boolean {
    return true;
  }

  get name(): string {
    return '';
  }

  get accessToken(): string {
    return '';
  }

  startAuthentication() {}

  public completeAuthentication() {
    return Promise.resolve(true);
  }

  signout() {}
}
