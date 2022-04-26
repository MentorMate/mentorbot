import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ActionEvent, NodeType, QuestionPropertiesChange, TodoItemFlatNode } from '../../question.models';

@Component({
  selector: 'app-question-characteristics',
  templateUrl: 'question-characteristics.component.html',
  styleUrls: ['question-characteristics.component.scss', '../question-page/question-page.component.scss'],
})
export class QuestionCharacteristicsComponent implements OnChanges {
  @Input() editedNode?: TodoItemFlatNode;
  @Output() questionUpdateEvent = new EventEmitter<QuestionPropertiesChange>();
  content?: string;

  public nodeType = NodeType;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editedNode'].previousValue) {
      this.content = this.editedNode?.content;
    }
  }

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
