import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NodeType, TodoItemFlatNode } from '../../question.models';

@Component({
  selector: 'app-question-characteristics',
  templateUrl: 'question-characteristics.component.html',
  // styleUrls: ['question-traits.component.scss'],
})
export class QuestionCharacteristicsComponent {
  @Input() editedNode?: TodoItemFlatNode;
  @Output() typeEvent = new EventEmitter<{ type: string; isNotValid: boolean | undefined }>();
  @Output() titleEvent = new EventEmitter<{ title: string; isNotValid: boolean | undefined }>();
  @Output() contentEvent = new EventEmitter<{ content: string; isNotValid: boolean | undefined }>();

  public nodeType = Object.values(NodeType).filter(value => typeof value === 'string');

  changeEditNodeIsAnswer(isAnswer: EventTarget | null, type: string, isNotValid: boolean | undefined) {
    (this.editedNode as TodoItemFlatNode).isAnswer = (isAnswer as HTMLTextAreaElement).value === 'true';
    if (!(this.editedNode as TodoItemFlatNode).isAnswer) {
      (this.editedNode as TodoItemFlatNode).content = '';
    }
    console.log(`${this.editedNode?.isAnswer} && valid content - ${isNotValid}`);
    this.typeEvent.emit({ type, isNotValid });
  }

  updateTitle(title: string, isNotValid: boolean | undefined) {
    this.titleEvent.emit({ title, isNotValid });
  }

  updateContent(content: string, isNotValid: boolean | undefined) {
    this.contentEvent.emit({ content, isNotValid });
  }
}
