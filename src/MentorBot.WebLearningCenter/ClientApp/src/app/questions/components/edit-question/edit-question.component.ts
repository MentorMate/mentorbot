import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Question, QuestionPropertiesChange, TodoItemFlatNode, TraitAction } from '../../question.models';

@Component({
  selector: 'app-edit-question',
  templateUrl: 'edit-question.component.html',
  styleUrls: ['edit-question.component.scss'],
})
export class EditQuestionComponent {
  @Input() nodeExists?: boolean;
  @Input() editedNode?: TodoItemFlatNode;
  @Input() saveButtonIsNotValid?: boolean;
  @Output() traitEvent = new EventEmitter<TraitAction>();
  @Output() deleteParentEvent = new EventEmitter<string>();
  @Output() questionUpdateEvent = new EventEmitter<QuestionPropertiesChange>();
  @Output() actionEvent = new EventEmitter<string>();

  traitAction({ name, type, actionType }: TraitAction): void {
    this.traitEvent.emit({ name, type, actionType });
  }

  deleteParent(parent: string): void {
    this.deleteParentEvent.emit(parent);
  }

  questionUpdate({ title, type, content, isNotValid }: QuestionPropertiesChange) {
    this.questionUpdateEvent.emit({ title, type, content, isNotValid });
  }

  draggedOver(e: DragEvent): void {
    e.preventDefault();
    this.actionEvent.emit('drag-over');
  }

  dragLeave(): void {
    this.actionEvent.emit('drag-leave');
  }

  saveNode(): void {
    this.actionEvent.emit('save-node');
  }

  cancelEdit(): void {
    this.actionEvent.emit('cancel-edit');
  }

  deleteQuestion(): void {
    this.actionEvent.emit('delete-question');
  }
}
