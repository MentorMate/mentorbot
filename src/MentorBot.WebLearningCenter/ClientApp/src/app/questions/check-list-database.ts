import { ifStmt } from '@angular/compiler/src/output/output_ast';
import { Injectable } from '@angular/core';
import { BehaviorSubject, take } from 'rxjs';
import { Question, TodoItemFlatNode } from './question.models';
import { QuestionService } from './question.service';

/**
 * Checklist database, it can build a tree structured Json object.
 * Each node in Json object represents a to-do item or a category.
 * If a node is a category, it has children items and new items can be added under the category.
 */
@Injectable()
export class ChecklistDatabase {
  dataChange = new BehaviorSubject<Question[]>([]);

  get data(): Question[] {
    return this.dataChange.value;
  }

  constructor(private readonly _questionService: QuestionService) {
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

    let allQuestions = this.flattenTreeStructure([...this.dataChange.value]);

    if (!allQuestions.find(n => n.title === name)) {
      this.dataChange.next([...this.data, node]);
    } else {
      this.dataChange.next([...this.data]);
    }
  }

  flattenTreeStructure(questions: Question[]): Question[] {
    return questions.reduce(function (r, a): Question[] {
      if (a.subQuestions && a.subQuestions.length != 0) {
        a.subQuestions.forEach(element => {
          if (!questions.find(q => q.title === element.title)) {
            questions = [...questions, element];
          } else if (element.isEdited) {
            let editedElement = questions.find(q => q.title === element.title);
            editedElement = element;
          }
        });
      }
      return questions;
    }, Object.create(null));
  }
}
