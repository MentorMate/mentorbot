import { ifStmt } from '@angular/compiler/src/output/output_ast';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
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

  initialize() {
    this._questionService.getQuestions().subscribe(questions => this.dataChange.next(questions));
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
  ) {
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

    let allQuestions = this.flattenTreeStructure(JSON.parse(JSON.stringify(this.dataChange.value)));

    if (!allQuestions.find(n => n.title === name)) {
      this.dataChange.value.push(node);
    }

    this.dataChange.next(this.data);
  }

  flattenTreeStructure(questions: Question[]): Question[] {
    for (let i = 0; i < questions.length; i++) {
      if (questions[i].subQuestions.length != 0) {
        var subQuestions = questions[i].subQuestions;

        questions[i].subQuestions = [];
        subQuestions.forEach(subQuestion => {
          questions.push(subQuestion);
        });
      }
    }

    let result = questions.reduce(function (r, a) {
      r[a.id as string] = r[a.id as string] || [];
      r[a.id as string].push(a);
      return r;
    }, Object.create(null));

    const resultArray = Object.keys(result).map(index => {
      let person = result[index];
      return person;
    });

    resultArray.forEach(element => {
      element.sort((value: any) => {
        return value.isEdited ? -1 : 1;
      });
    });

    let finalResult: Question[] = [];

    resultArray.forEach((element: any[]) => {
      if (!element[0].id) {
        element.forEach(nestedEl => {
          finalResult.push(nestedEl);
        });
      } else {
        finalResult.push(element[0]);
      }
    });

    console.log(finalResult);

    return finalResult;
  }
}
