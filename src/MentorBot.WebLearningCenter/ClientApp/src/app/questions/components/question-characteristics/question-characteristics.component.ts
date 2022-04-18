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
  content?: string =
    !this.editedNode?.isAnswer && (this.editedNode?.content !== undefined || this.editedNode?.content !== 'undefined')
      ? ''
      : this.editedNode?.content;

  public nodeType = NodeType;

  changeEditNodeIsAnswer(type: number, isNotValid: boolean | undefined): void {
    if (type.toString() !== this.nodeType.Answer.toString()) {
      this.resetContent();
    }

    this.questionUpdateEvent?.emit({
      ...this.editedNode,
      isAnswer: type.toString() === this.nodeType.Answer.toString(),
      isNotValid,
    });
  }

  private resetContent() {
    if (this.editedNode?.content) {
      this.editedNode.content = '';
    }
  }

  updateTitle(title: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent?.emit({ ...this.editedNode, title, isNotValid });
  }

  updateContent(content: string, isNotValid: boolean | undefined): void {
    this.questionUpdateEvent?.emit({ ...this.editedNode, content, isNotValid });
  }
}
