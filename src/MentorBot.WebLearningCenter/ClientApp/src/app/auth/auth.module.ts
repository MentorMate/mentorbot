import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { OAuthModule } from 'angular-oauth2-oidc';

import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';
import { AuthCallbackLogoutComponent } from './components/auth-callback-logout/auth-callback-logout.component';

import { AuthService, RoleAuthService } from './auth.service';
import { GoogleAuthService } from './services/auth.google.service';
import { DebugAuthService } from './services/auth.debug.service';
import { AuthGuard } from './auth-guard.service';

const allowSkipLogin = false;

export const COMPONENTS = [AuthCallbackComponent, AuthCallbackLogoutComponent];

@NgModule({
  imports: [CommonModule, ReactiveFormsModule, OAuthModule.forRoot()],
  declarations: COMPONENTS,
  exports: COMPONENTS,
})
export class AuthModule {
  public static forRoot(): ModuleWithProviders<AuthModule> {
    return {
      ngModule: AuthModule,
      providers: [{ provide: AuthService, useClass: allowSkipLogin ? DebugAuthService : GoogleAuthService }, AuthGuard, RoleAuthService],
    };
  }
}
