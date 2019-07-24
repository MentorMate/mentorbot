import { Injectable } from '@angular/core';
import { AuthService } from '../auth.service';

@Injectable()
export class DebugAuthService implements AuthService {
  get isLoggedIn() { return true; }
  get name() { return ''; }
  get accessToken() { return null; }
  startAuthentication() { }
  public completeAuthentication() { return Promise.resolve(true); }
  signout() { }
}
