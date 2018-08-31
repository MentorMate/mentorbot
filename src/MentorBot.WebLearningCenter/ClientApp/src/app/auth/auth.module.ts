import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

import { Adal5Service, Adal5HTTPService } from 'adal-angular5';

import { AuthCallbackComponent } from './auth-callback.component';
import { AuthService } from './auth.service';
import { AuthGuard } from './auth-guard.service';

export const COMPONENTS = [
  AuthCallbackComponent
];

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  declarations: COMPONENTS,
  exports: COMPONENTS
})
export class AuthModule {
  public static forRoot(): ModuleWithProviders {
    return {
      ngModule: RootAuthModule,
      providers: [
        AuthService,
        AuthGuard,
        Adal5Service,
        { provide: Adal5HTTPService, useFactory: Adal5HTTPService.factory, deps: [HttpClient, Adal5Service] }],
    };
  }
}

@NgModule({
  imports: [
    AuthModule,
    RouterModule.forChild([
      {
        path: 'auth-callback',
        component: AuthCallbackComponent
      }
    ])
  ]
})
export class RootAuthModule { }
