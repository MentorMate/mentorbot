import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-question-buttons',
  templateUrl: 'question-buttons.component.html',
  styleUrls: ['question-buttons.component.scss'],
})
export class QuestionButtonsComponent {
  @Input() saveButtonIsNotValid?: boolean;
  @Output() saveEvent = new EventEmitter();
  @Output() cancelEvent = new EventEmitter();
  @Output() deleteEvent = new EventEmitter();

  save() {
    this.saveEvent.emit();
  }

  cancel() {
    this.cancelEvent.emit();
  }

  delete() {
    this.deleteEvent.emit();
  }
}
