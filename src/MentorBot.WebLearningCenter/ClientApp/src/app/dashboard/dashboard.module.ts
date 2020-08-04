import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DashboardPageComponent } from './components/dashboard-page/dashboard-page.component';
import { DashboardService } from './dashboard.service';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    DashboardPageComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild([{ path: '', component: DashboardPageComponent }])
  ],
  providers: [DashboardService]
})
export class DashboardModule { }
