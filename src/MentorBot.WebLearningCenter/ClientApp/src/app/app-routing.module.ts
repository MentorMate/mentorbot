import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

import { AuthModule } from './auth/auth.module';

import { AuthGuard } from './auth/auth-guard.service';
import { AuthCallbackComponent } from './auth/components/auth-callback/auth-callback.component';
import { AuthCallbackLogoutComponent } from './auth/components/auth-callback-logout/auth-callback-logout.component';
import { AppMainComponent } from './app-main.component';
import { AppRootComponent } from './app-root.component';
import { MenuComponent } from './shared/components/menu/menu.component';
import { NotFoundPageComponent } from './shared/components/not-found-page/not-found-page.component';
import { NoAccessPageComponent } from './shared/components/no-access/no-access.component';

export const appRoutes: Routes = [
  { path: '', redirectTo: 'app', pathMatch: 'full' },
  { path: 'app/signin-google', component: AuthCallbackComponent },
  { path: 'app/signout-callback', component: AuthCallbackLogoutComponent },
  { path: 'app/no-access', component: NoAccessPageComponent },
  {
    path: 'app',
    component: AppMainComponent,
    canLoad: [AuthGuard],
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
      },
      {
        path: 'settings',
        loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule),
      },
      {
        path: 'users',
        loadChildren: () => import('./users/user.module').then(m => m.UsersModule),
      },
      {
        path: 'about',
        loadChildren: () => import('./about/about.module').then(m => m.AboutModule),
      },
      {
        path: 'questions',
        loadChildren: () => import('./questions/question.module').then(m => m.QuestionsModule),
      },
      { path: '**', component: NotFoundPageComponent },
    ],
  },
  { path: '**', component: NotFoundPageComponent },
];

@NgModule({
  declarations: [AppRootComponent, AppMainComponent, NotFoundPageComponent, NoAccessPageComponent, MenuComponent],
  imports: [CommonModule, AuthModule.forRoot(), RouterModule.forRoot(appRoutes), MatToolbarModule, MatButtonModule],
  exports: [RouterModule],
})
export class AppRoutingModule {}
