import { ifStmt } from '@angular/compiler/src/output/output_ast';
import { Injectable } from '@angular/core';
import { BehaviorSubject, take } from 'rxjs';
import { UtilitiesService } from '../shared/utilities.service';
import { Question, TodoItemFlatNode } from './question.models';
import { QuestionService } from './question.service';

/**
 * Checklist database, it can build a tree structured Json object.
 * Each node in Json object represents a question or an answer.
 */
@Injectable()
export class ChecklistDatabase {
  dataChange = new BehaviorSubject<Question[]>([]);

  get data(): Question[] {
    return this.dataChange.value;
  }

  constructor(private readonly _questionService: QuestionService, private readonly _utilitiesService: UtilitiesService) {
    this.initialize();
  }

  initialize(): void {
    this._questionService
      .getQuestions()
      .pipe(take(1))
      .subscribe(questions => this.dataChange.next(questions));
  }

  updateItem(
    node: Question,
    name?: string,
    isAnswer?: boolean,
    content?: string,
    acquireTraits?: string[],
    requiredTraits?: string[],
    parents?: {
      [key: string]: string;
    }
  ): void {
    node.title = name as string;
    node.content = content;
    node.isAnswer = isAnswer as boolean;
    node.isEdited = true;
    if (acquireTraits) {
      node.acquireTraits = acquireTraits;
    }
    if (requiredTraits) {
      node.requiredTraits = requiredTraits;
    }
    if (parents) {
      node.parents = parents;
    }

    if (!this.elementExists(this.dataChange.value, node.title)) {
      this.dataChange.next([...this.data, node]);
    } else {
      this.dataChange.next([...this.data]);
    }
  }

  getFlatTree(): Question[] {
    const allQuestions = this._utilitiesService.flatTree<Question>([...this.dataChange.value], 'subQuestions');

    let result: Question[] = this.getUniqueQuestions(allQuestions);

    return result;
  }

  elementExists = (questions: Question[], title: string): boolean => {
    return questions.some(q => {
      if (q.title === title) {
        return true;
      }

      if (q.subQuestions) {
        return this.elementExists(q.subQuestions, title);
      } else {
        return false;
      }
    });
  };

  private getUniqueQuestions(allQuestions: Question[]) {
    let uniqueQuestions: Question[] = [];

    for (let i = 0; i < allQuestions.length; i++) {
      if (this.questionIsUnique(allQuestions[i].id, uniqueQuestions)) {
        uniqueQuestions.push(allQuestions[i]);
      } else if (allQuestions[i].isEdited) {
        const index = uniqueQuestions.findIndex(a => a.id === allQuestions[i].id);
        uniqueQuestions[index] = allQuestions[i];
      }
    }
    return uniqueQuestions;
  }

  private questionIsUnique(questionId: string | undefined, uniqueQuestions: Question[]) {
    return questionId === undefined || !uniqueQuestions.some(a => a.id === questionId);
  }
}
