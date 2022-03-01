import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatTreeModule } from '@angular/material/tree';

import { SettingsService } from '../settings/settings.service';
import { SharedModule } from '../shared/shared.module';
import { QuestionPageComponent } from './components/question-page/question-page.component';
import { QuestionService } from './question.service';

@NgModule({
  declarations: [QuestionPageComponent],
  imports: [CommonModule, SharedModule, RouterModule.forChild([{ path: '', component: QuestionPageComponent }]), MatTreeModule],
  providers: [QuestionService, SettingsService],
})
export class QuestionsModule {}
