import { Component, OnInit } from '@angular/core';
import { Question } from '../../question.models';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { QuestionService } from '../../question.service';

@Component({
  selector: 'app-questions',
  templateUrl: './question-page.component.html',
  styleUrls: ['./question-page.component.css'],
})
export class QuestionPageComponent implements OnInit {
  nestedTreeControl: NestedTreeControl<Question>;
  nestedDataSource: MatTreeNestedDataSource<Question>;
  questions: Question[] = [
    { name: 'ha', subQuestions: [] },
    { name: 'hah', subQuestions: [{ name: 'yu', subQuestions: [] }] },
    { name: 'haha', subQuestions: [] },
  ];

  constructor(private questionService: QuestionService) {
    this.nestedTreeControl = new NestedTreeControl<Question>(this._getChildren);
    this.nestedDataSource = new MatTreeNestedDataSource();
  }

  ngOnInit(): void {
    this.nestedDataSource.data = this.questions;
  }

  private _getChildren = (node: Question) => {
    return node.subQuestions;
  };

  hasNestedChild = (_: number, nodeData: Question) => {
    return nodeData.subQuestions.length > 0;
  };
}
