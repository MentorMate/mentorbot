import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionEvent, Question, QuestionPropertiesChange, TodoItemFlatNode, TraitAction, TraitTypes } from '../../question.models';

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
  @Output() actionEvent = new EventEmitter<ActionEvent>();

  public traitTypes = TraitTypes;

  traitAction({ name, type, actionType }: TraitAction): void {
    this.traitEvent?.emit({ name, type, actionType });
  }

  deleteParent(parent: string): void {
    this.deleteParentEvent?.emit(parent);
  }

  questionUpdate({ item, isAnswer, content, isNotValid }: QuestionPropertiesChange) {
    this.questionUpdateEvent?.emit({ item, isAnswer, content, isNotValid });
  }

  draggedOver(e: DragEvent): void {
    e.preventDefault();
    this.actionEvent?.emit(ActionEvent.DragOver);
  }

  dragLeave(): void {
    this.actionEvent?.emit(ActionEvent.DragLeave);
  }

  saveNode(): void {
    this.actionEvent?.emit(ActionEvent.SaveNode);
  }

  cancelEdit(): void {
    this.actionEvent?.emit(ActionEvent.CancelEdit);
  }

  deleteQuestion(): void {
    this.actionEvent?.emit(ActionEvent.DeleteQuestion);
  }
}
