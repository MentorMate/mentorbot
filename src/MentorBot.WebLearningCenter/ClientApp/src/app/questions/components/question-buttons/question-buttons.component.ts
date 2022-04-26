import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-question-buttons',
  templateUrl: 'question-buttons.component.html',
  styleUrls: ['question-buttons.component.scss', '../question-page/question-page.component.scss'],
})
export class QuestionButtonsComponent {
  @Input() saveButtonIsNotValid?: boolean;
  @Input() nodeExists?: boolean;
  @Output() saveEvent = new EventEmitter();
  @Output() cancelEvent = new EventEmitter();
  @Output() deleteEvent = new EventEmitter();

  save(): void {
    this.saveEvent?.emit();
  }

  cancel(): void {
    this.cancelEvent?.emit();
  }

  delete(): void {
    this.deleteEvent?.emit();
  }
}
