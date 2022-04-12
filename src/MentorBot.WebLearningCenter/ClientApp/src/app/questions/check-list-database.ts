import { ifStmt } from '@angular/compiler/src/output/output_ast';
import { Injectable } from '@angular/core';
import { BehaviorSubject, take } from 'rxjs';
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

    const elementExists = (questions: Question[], title: string): boolean => {
      return questions.some(q => {
        if (q.title === title) {
          return true;
        }

        return elementExists(q.subQuestions, title);
      });
    };

    if (!elementExists(this.dataChange.value, node.title)) {
      this.dataChange.next([...this.data, node]);
    } else {
      this.dataChange.next([...this.data]);
    }
  }

  getFlatTree(): Question[] {
    const flatTree = (nestedObjects: Question[]) => {
      return nestedObjects.reduce(function (resultArray: Question[], nestedObject: Question): Question[] {
        if (Object.keys(resultArray).length == 0) {
          resultArray = [];
        }
        resultArray.push(nestedObject);
        if (nestedObject.subQuestions && nestedObject.subQuestions.length != 0) {
          resultArray.push(...flatTree(nestedObject.subQuestions));
        }
        return resultArray;
      }, Object.create(null));
    };

    const allQuestions = flatTree([...this.dataChange.value]);

    let result: Question[] = [];

    for (let i = 0; i < allQuestions.length; i++) {
      if (!result.some(a => a.id === allQuestions[i].id)) {
        result.push(allQuestions[i]);
      } else if (allQuestions[i].isEdited) {
        const index = result.findIndex(a => a.id === allQuestions[i].id);
        result[index] = allQuestions[i];
      }
    }

    return result;
  }
}
