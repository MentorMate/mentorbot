import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppRootComponent } from './app-root.component';
import { AuthModule } from './auth/auth.module';

@NgModule({
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    AppRoutingModule,
    AuthModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppRootComponent]
})
export class AppModule { }
