import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TodoItemFlatNode } from '../../question.models';

@Component({
  selector: 'app-edit-question',
  templateUrl: 'edit-question.component.html',
  styleUrls: ['edit-question.component.scss'],
})
export class EditQuestionComponent {
  @Input() editedNode?: TodoItemFlatNode;
  @Input() saveButtonIsNotValid?: boolean;
  @Output() deleteTraitEvent = new EventEmitter<{ name: string; type: string }>();
  @Output() addTraitEvent = new EventEmitter<{ name: string; type: string }>();
  @Output() deleteParentEvent = new EventEmitter<string>();
  @Output() typeEvent = new EventEmitter<{ type: string; isNotValid?: boolean }>();
  @Output() titleEvent = new EventEmitter<{ title: string; isNotValid?: boolean }>();
  @Output() contentEvent = new EventEmitter<{ content: string; isNotValid?: boolean }>();
  @Output() saveEvent = new EventEmitter();
  @Output() cancelEvent = new EventEmitter();
  @Output() deleteEvent = new EventEmitter();

  deleteTrait({ name, type }: { name: string; type: string }) {
    this.deleteTraitEvent.emit({ name, type });
  }

  addTrait({ name, type }: { name: string; type: string }) {
    this.addTraitEvent.emit({ name, type });
  }

  deleteParent(parent: string) {
    this.deleteParentEvent.emit(parent);
  }

  changeEditNodeIsAnswer({ type, isNotValid }: { type: string; isNotValid?: boolean }) {
    this.typeEvent.emit({ type, isNotValid });
  }

  updateTitle({ title, isNotValid }: { title: string; isNotValid?: boolean }) {
    this.titleEvent.emit({ title, isNotValid });
  }

  updateContent({ content, isNotValid }: { content: string; isNotValid?: boolean }) {
    this.contentEvent.emit({ content, isNotValid });
  }

  saveNode() {
    this.saveEvent.emit();
  }

  cancelEdit() {
    this.cancelEvent.emit();
  }

  deleteQuestion() {
    this.deleteEvent.emit();
  }
}
