import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnDestroy } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Subscription, take } from 'rxjs';
import { ChecklistDatabase } from '../../check-list-database';
import { NodeType, Question, TodoItemFlatNode } from '../../question.models';
import { QuestionService } from '../../question.service';

@Component({
  selector: 'app-questions',
  templateUrl: 'question-page.component.html',
  styleUrls: ['question-page.component.scss'],
  providers: [ChecklistDatabase],
})
export class QuestionPageComponent implements OnDestroy {
  /** Map from flat node to nested node. This helps us finding the nested node to be modified */
  flatNodeMap = new Map<TodoItemFlatNode, Question>();

  /** Map from nested node to flattened node. This helps us to keep the same object for selection */
  nestedNodeMap = new Map<Question, TodoItemFlatNode>();
  treeControl: FlatTreeControl<TodoItemFlatNode>;
  treeFlattener: MatTreeFlattener<Question, TodoItemFlatNode>;
  dataSource: MatTreeFlatDataSource<Question, TodoItemFlatNode>;
  editedNode?: TodoItemFlatNode;
  isConfirmed: boolean = false;
  savingNode?: TodoItemFlatNode;
  saveButtonIsNotValid?: boolean;
  dataChangeSubscription?: Subscription;

  public nodeType = Object.values(NodeType).filter(value => typeof value === 'string');

  constructor(private database: ChecklistDatabase, private readonly _questionService: QuestionService) {
    this.treeFlattener = new MatTreeFlattener(this.transformer, this.getLevel, this.isExpandable, this.getChildren);
    this.treeControl = new FlatTreeControl<TodoItemFlatNode>(this.getLevel, this.isExpandable);
    this.dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

    this.dataChangeSubscription = database.dataChange.subscribe(data => {
      this.dataSource.data = data;
    });
  }

  getLevel = (node: TodoItemFlatNode) => node.level;

  isExpandable = (node: TodoItemFlatNode) => node.expandable;

  getChildren = (node: Question): Question[] => node.subQuestions;

  transformer = (node: Question, level: number) => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode = existingNode && existingNode.item === node.title ? existingNode : new TodoItemFlatNode();
    flatNode.item = node.title;
    flatNode.content = node.content;
    flatNode.id = node.id;
    flatNode.acquireTraits = node.acquireTraits;
    flatNode.requiredTraits = node.requiredTraits;
    flatNode.parents = node.parents;
    flatNode.isAnswer = node.isAnswer;
    flatNode.level = level;
    flatNode.expandable = !!node.subQuestions;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  addNewItem() {
    this.editedNode = {} as TodoItemFlatNode;
    this.editedNode.acquireTraits = [];
    this.editedNode.requiredTraits = [];
    this.editedNode.parents = {};
    this.savingNode = {} as TodoItemFlatNode;
    this.savingNode.acquireTraits = [];
    this.savingNode.requiredTraits = [];
    this.savingNode.parents = {};
    this.saveButtonIsNotValid = true;
  }

  saveNode() {
    if (this.savingNode?.acquireTraits && this.editedNode?.acquireTraits) {
      this.savingNode.acquireTraits = this.editedNode?.acquireTraits;
    }
    if (this.savingNode?.requiredTraits && this.editedNode?.requiredTraits) {
      this.savingNode.requiredTraits = this.editedNode?.requiredTraits;
    }
    if (this.savingNode?.parents && this.editedNode?.parents) {
      this.savingNode.parents = this.editedNode?.parents;
    }

    const nestedNode = this.flatNodeMap.get(this.savingNode as TodoItemFlatNode);

    if (this.savingNode) {
      this.database.updateItem(
        nestedNode === undefined ? new Question() : nestedNode!,
        this.editedNode?.item,
        this.editedNode?.isAnswer,
        this.editedNode?.content,
        this.savingNode?.acquireTraits,
        this.savingNode?.requiredTraits,
        this.savingNode?.parents
      );
    }
    this.editedNode = undefined;
    this.savingNode = undefined;
  }

  editItem(node: TodoItemFlatNode) {
    this.editedNode = JSON.parse(JSON.stringify(node));
    this.savingNode = node;
    this.saveButtonIsNotValid = false;
  }

  cancelEdit() {
    this.editedNode = undefined;
    this.savingNode = undefined;
  }

  handleDragEnd(e: any) {
    if (this.editedNode) {
      this.editedNode.parents[e.toElement.textContent] = e.toElement.textContent;
    }
  }

  deleteTrait({ name, type }: { name: string; type: string }) {
    if (type === 'acquire') {
      let index = this.editedNode?.acquireTraits.indexOf(name);
      if (index != undefined) {
        this.editedNode?.acquireTraits.splice(index, 1);
      }
    } else if (type === 'required') {
      let index = this.editedNode?.requiredTraits.indexOf(name);
      if (index != undefined) {
        this.editedNode?.requiredTraits.splice(index, 1);
      }
    }
  }

  addTrait({ name, type }: { name: string; type: string }) {
    if (name === '') {
      return;
    }

    if (type === 'acquire') {
      if (!this.editedNode?.acquireTraits.includes(name)) {
        this.editedNode?.acquireTraits.push(name);
      }
    } else if (type === 'required') {
      if (!this.editedNode?.requiredTraits.includes(name)) {
        this.editedNode?.requiredTraits.push(name);
      }
    }
  }

  deleteParent(parent: string) {
    if (this.editedNode?.parents) {
      let key = Object.keys(this.editedNode?.parents).find(key => this.editedNode?.parents[key] === parent);
      if (key) {
        delete this.editedNode?.parents[key];
        delete this.savingNode?.parents[key];
      }
    }
  }

  changeEditNodeIsAnswer({ type, isNotValid }: { type: string; isNotValid?: boolean }) {
    (this.editedNode as TodoItemFlatNode).isAnswer = type === 'true';
    this.saveButtonIsNotValid = isNotValid;
  }

  updateTitle({ title, isNotValid }: { title: string; isNotValid?: boolean }) {
    (this.editedNode as TodoItemFlatNode).item = title;
    this.saveButtonIsNotValid = isNotValid;
  }

  updateContent({ content, isNotValid }: { content: string; isNotValid?: boolean }) {
    (this.editedNode as TodoItemFlatNode).content = content;
    this.saveButtonIsNotValid = isNotValid;
  }

  deleteQuestion() {
    this.isConfirmed = true;
  }

  onConfirm() {
    const question = this.flatNodeMap.get(this.savingNode as TodoItemFlatNode);

    this._questionService
      .deleteQuestion(question)
      .pipe(take(1))
      .subscribe(q =>
        this._questionService
          .getQuestions()
          .pipe(take(1))
          .subscribe(questions => this.database.dataChange.next(questions))
      );

    this.editedNode = undefined;
    this.savingNode = undefined;
    this.isConfirmed = false;
  }

  save() {
    this.editedNode = undefined;
    this.savingNode = undefined;
    var result = this.flattenTreeStructure(this.database.data);
    this._questionService
      .saveQuestions(result)
      .pipe(take(1))
      .subscribe(d =>
        this._questionService
          .getQuestions()
          .pipe(take(1))
          .subscribe(questions => this.database.dataChange.next(questions))
      );
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
      element.sort((value: Question) => {
        return value.isEdited ? -1 : 1;
      });
    });

    let finalResult: Question[] = [];

    resultArray.forEach((element: Question[]) => {
      if (!element[0].id) {
        element.forEach(nestedEl => {
          finalResult.push(nestedEl);
        });
      } else {
        finalResult.push(element[0]);
      }
    });

    return finalResult;
  }

  ngOnDestroy() {
    this.dataChangeSubscription?.unsubscribe();
  }
}
