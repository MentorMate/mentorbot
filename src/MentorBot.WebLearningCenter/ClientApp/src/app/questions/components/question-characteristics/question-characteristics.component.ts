import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionEvent, NodeType, QuestionPropertiesChange, TodoItemFlatNode } from '../../question.models';

@Component({
  selector: 'app-question-characteristics',
  templateUrl: 'question-characteristics.component.html',
  styleUrls: ['question-characteristics.component.scss'],
})
export class QuestionCharacteristicsComponent {
  @Input() editedNode?: TodoItemFlatNode;
  @Output() questionUpdateEvent = new EventEmitter<QuestionPropertiesChange>();

  public nodeType = NodeType;

  changeEditNodeIsAnswer(type: number, isNotValid: boolean | undefined): void {
    (this.editedNode as TodoItemFlatNode).isAnswer = type.toString() === this.nodeType.Answer.toString();
    if (!(this.editedNode as TodoItemFlatNode).isAnswer) {
      (this.editedNode as TodoItemFlatNode).content = '';
    }
    this.questionUpdateEvent?.emit({
      ...this.editedNode,
      isAnswer: type.toString() === this.nodeType.Answer.toString(),
      isNotValid,
    });
  }

  updateTitle(title: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent?.emit({ ...this.editedNode, item: title, isNotValid });
  }

  updateContent(content: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent?.emit({ ...this.editedNode, content, isNotValid });
  }
}
