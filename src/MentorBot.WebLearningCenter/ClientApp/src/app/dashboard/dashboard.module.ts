import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { DashboardPageComponent } from './dashboard-page.component';
import { DashboardService } from './dashboard.service';

@NgModule({
  declarations: [
    DashboardPageComponent
  ],
  imports: [
    HttpModule,
    CommonModule,
    RouterModule.forChild([{ path: '', component: DashboardPageComponent }])
  ],
  providers: [DashboardService]
})
export class DashboardModule { }
