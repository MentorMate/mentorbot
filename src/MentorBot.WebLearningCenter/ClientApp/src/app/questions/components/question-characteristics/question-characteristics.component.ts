import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NodeType, QuestionPropertiesChange, TodoItemFlatNode } from '../../question.models';

@Component({
  selector: 'app-question-characteristics',
  templateUrl: 'question-characteristics.component.html',
  // styleUrls: ['question-traits.component.scss'],
})
export class QuestionCharacteristicsComponent {
  @Input() editedNode?: TodoItemFlatNode;
  @Output() questionUpdateEvent = new EventEmitter<QuestionPropertiesChange>();

  public nodeType = Object.values(NodeType).filter(value => typeof value === 'string');

  changeEditNodeIsAnswer(isAnswer: EventTarget | null, type: string, isNotValid: boolean | undefined): void {
    (this.editedNode as TodoItemFlatNode).isAnswer = (isAnswer as HTMLTextAreaElement).value === 'true';
    if (!(this.editedNode as TodoItemFlatNode).isAnswer) {
      (this.editedNode as TodoItemFlatNode).content = '';
    }
    this.questionUpdateEvent.emit({ title: this.editedNode?.item, type, content: this.editedNode?.content, isNotValid });
  }

  updateTitle(title: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent.emit({ title, type: this.editedNode?.isAnswer ? 'true' : '', content: this.editedNode?.content, isNotValid });
  }

  updateContent(content: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent.emit({ title: this.editedNode?.item, type: this.editedNode?.isAnswer ? 'true' : '', content, isNotValid });
  }
}
