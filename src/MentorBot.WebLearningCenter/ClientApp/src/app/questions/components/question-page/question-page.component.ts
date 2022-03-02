import { Component, Injectable, OnInit } from '@angular/core';
import { Question } from '../../question.models';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { QuestionService } from '../../question.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Component({
  selector: 'app-questions',
  templateUrl: './question-page.component.html',
  styleUrls: ['./question-page.component.css'],
})
export class QuestionPageComponent implements OnInit {
  nestedTreeControl: NestedTreeControl<Question>;
  nestedDataSource: MatTreeNestedDataSource<Question>;
  // questions: Question[] = [
  //   { content: 'ha', index: 1, type: 1, subQuestions: [] },
  //   { content: 'hah', index: 1, type: 1, subQuestions: [] },
  //   {
  //     content: 'haha',
  //     index: 1,
  //     type: 1,
  //     subQuestions: [{ content: 'hahaaaaa', index: 1, type: 1, subQuestions: [] }],
  //   },
  // ];
  questions$?: Observable<Question[]>;

  constructor(private readonly _questionService: QuestionService) {
    this.nestedTreeControl = new NestedTreeControl<Question>(this._getChildren);
    this.nestedDataSource = new MatTreeNestedDataSource();
  }

  ngOnInit(): void {
    // this.questions$ = this._questionService.getQuestions().pipe(tap(questions => (this.nestedDataSource.data = questions)));
    this.questions$ = this._questionService.getQuestions();
  }

  createField(nodeName: string) {
    var field = document.createElement('input');
    var subCategoriesList = document.querySelector(`#${nodeName}`);
    field.addEventListener('focusout', event => {});
    field.innerHTML = 'success';
    subCategoriesList?.appendChild(field);
  }

  save() {
    // this._questionService.saveQuestions(this.questions).subscribe();
  }

  private _getChildren = (node: Question) => {
    return node.subQuestions;
  };

  hasNestedChild = (_: number, nodeData: Question) => {
    return nodeData.subQuestions.length > 0;
  };
}
