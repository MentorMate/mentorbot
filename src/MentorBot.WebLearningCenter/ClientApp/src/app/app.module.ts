import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppRootComponent } from './app-root.component';
import { HttpRequestInterceptor } from './shared/http-request-interceptor';

@NgModule({
  imports: [BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }), HttpClientModule, AppRoutingModule],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpRequestInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppRootComponent],
})
export class AppModule {}
