import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SettingsService } from '../settings/settings.service';
import { UserPageComponent } from './components/user-page/user-page.component';
import { UserService } from './user.service';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    UserPageComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild([{ path: '', component: UserPageComponent }])
  ],
  providers: [UserService, SettingsService]
})
export class UsersModule { }
