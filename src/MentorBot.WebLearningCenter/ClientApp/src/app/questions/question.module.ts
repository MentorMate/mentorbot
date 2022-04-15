import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MDBBootstrapModule } from 'angular-bootstrap-md';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

import { SettingsService } from '../settings/settings.service';
import { SharedModule } from '../shared/shared.module';
import { QuestionPageComponent } from './components/question-page/question-page.component';
import { QuestionService } from './question.service';
import { MatTreeModule } from '@angular/material/tree';
import { FormsModule } from '@angular/forms';
import { EditQuestionComponent } from './components/edit-question/edit-question.component';
import { QuestionTraitsComponent } from './components/question-traits/question-traits.component';
import { QuestionCharacteristicsComponent } from './components/question-characteristics/question-characteristics.component';
import { QuestionButtonsComponent } from './components/question-buttons/question-buttons.component';
import { MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogModule } from '../shared/confirm-dialog/confirm-dialog.module';

@NgModule({
  declarations: [
    QuestionPageComponent,
    EditQuestionComponent,
    QuestionTraitsComponent,
    QuestionCharacteristicsComponent,
    QuestionButtonsComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild([{ path: '', component: QuestionPageComponent }]),
    MatIconModule,
    MatTreeModule,
    FormsModule,
    MatDialogModule,
    ConfirmDialogModule,
    MDBBootstrapModule,
    MatButtonModule,
    MatSelectModule,
  ],
  entryComponents: [QuestionPageComponent],
  bootstrap: [QuestionPageComponent],
  providers: [QuestionService, SettingsService],
})
export class QuestionsModule {}
