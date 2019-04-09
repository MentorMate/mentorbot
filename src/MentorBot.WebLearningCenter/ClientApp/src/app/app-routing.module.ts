import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';

import { AuthModule } from './auth/auth.module';

import { AuthGuard } from './auth/auth-guard.service';
import { AuthCallbackComponent } from './auth/auth-callback.component';
import { AuthCallbackLogoutComponent } from './auth/auth-callback-logout.component';
import { AppMainComponent } from './app-main.component';
import { AppRootComponent } from './app-root.component';
import { MenuComponent } from './shared/menu.component';
import { NotFoundPageComponent } from './shared/not-found-page.component';

export const appRoutes: Routes = [
  { path: '', redirectTo: 'app', pathMatch: 'full' },
  { path: 'app/signin-google', component: AuthCallbackComponent },
  { path: 'app/signout-callback', component: AuthCallbackLogoutComponent },
  {
    path: 'app',
    component: AppMainComponent,
    canLoad: [AuthGuard],
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [{
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadChildren: './dashboard/dashboard.module#DashboardModule'
      },
      {
        path: 'settings',
        loadChildren: './settings/settings.module#SettingsModule'
      },
      {
        path: 'about',
        loadChildren: './about/about.module#AboutModule'
      },
      { path: '**', component: NotFoundPageComponent }
    ]
  },
  { path: '**', component: NotFoundPageComponent }
];

@NgModule({
  declarations: [
    AppRootComponent,
    AppMainComponent,
    NotFoundPageComponent,
    MenuComponent
  ],
  imports: [
    CommonModule,
    AuthModule.forRoot(),
    RouterModule.forRoot(appRoutes)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
