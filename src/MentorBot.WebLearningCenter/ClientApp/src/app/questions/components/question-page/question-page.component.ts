import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Subscription, take } from 'rxjs';
import { ConfirmDialogComponent } from 'src/app/shared/confirm-dialog/components/confirm-dialog.component';
import { ChecklistDatabase } from '../../check-list-database';
import { NodeType, Question, QuestionPropertiesChange, TodoItemFlatNode, TraitAction } from '../../question.models';
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
  savingNode?: TodoItemFlatNode;
  saveButtonIsNotValid?: boolean;
  dataChangeSubscription?: Subscription;
  addParent?: boolean;
  nodeExists?: boolean;

  public nodeType = Object.values(NodeType).filter(value => typeof value === 'string');

  constructor(private database: ChecklistDatabase, private readonly _questionService: QuestionService, private dialog: MatDialog) {
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

  transformer = (node: Question, level: number): TodoItemFlatNode => {
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

  addNewItem(): void {
    const emptyNode = {
      title: '',
      acquireTraits: [],
      requiredTraits: [],
      parents: {},
      item: '',
      level: 0,
      expandable: false,
      editMode: false,
      isAnswer: false,
      subQuestions: [],
      isEdited: false,
    };

    this.editedNode = { ...emptyNode };
    this.savingNode = { ...emptyNode };
    this.saveButtonIsNotValid = true;
    this.nodeExists = false;
  }

  saveNode(): void {
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
    this.resetEditedAndSavingNodes();
  }

  editItem(node: TodoItemFlatNode): void {
    this.editedNode = { ...node };
    this.savingNode = node;
    this.saveButtonIsNotValid = false;
    this.nodeExists = true;
  }

  resetEditedAndSavingNodes(): void {
    this.editedNode = undefined;
    this.savingNode = undefined;
  }

  handleDragEnd(e: any): void {
    if (this.editedNode && this.addParent) {
      this.editedNode.parents[e.toElement.textContent] = e.toElement.textContent;
    }
  }

  traitAction({ name, type, actionType }: TraitAction): void {
    if (type === 'acquire' && actionType === 'delete') {
      if (this.editedNode?.acquireTraits) {
        this.editedNode.acquireTraits = this.editedNode.acquireTraits.filter(t => t !== name);
      }
    } else if (type === 'acquire' && actionType === 'add') {
      if (this.editedNode?.acquireTraits && !this.editedNode?.acquireTraits.includes(name)) {
        this.editedNode.acquireTraits = [...this.editedNode?.acquireTraits, name];
      }
    } else if (type === 'required' && actionType === 'delete') {
      if (this.editedNode?.requiredTraits) {
        this.editedNode.requiredTraits = this.editedNode.requiredTraits.filter(t => t !== name);
      }
    } else if (type === 'required' && actionType === 'add') {
      if (this.editedNode?.requiredTraits && !this.editedNode?.requiredTraits.includes(name)) {
        this.editedNode.requiredTraits = [...this.editedNode?.requiredTraits, name];
      }
    }
  }

  deleteParent(parent: string): void {
    if (this.editedNode?.parents) {
      this.editedNode.parents = Object.fromEntries(Object.entries(this.editedNode.parents).filter(([k, v]) => v !== parent));
    }
  }

  changeEditNodeIsAnswer({ type, isNotValid }: { type: string; isNotValid?: boolean }): void {
    (this.editedNode as TodoItemFlatNode).isAnswer = type === 'true';
    this.saveButtonIsNotValid = isNotValid;
  }

  updateNode({ title, type, content, isNotValid }: QuestionPropertiesChange) {
    (this.editedNode as TodoItemFlatNode).isAnswer = type === 'true';
    (this.editedNode as TodoItemFlatNode).item = title;
    (this.editedNode as TodoItemFlatNode).content = content;
    this.saveButtonIsNotValid = isNotValid;
  }

  action(action: string) {
    if (action === 'drag-over') {
      this.addParent = true;
    } else if (action === 'drag-leave') {
      this.addParent = false;
    } else if (action === 'save-node') {
      this.saveNode();
    } else if (action === 'cancel-edit') {
      this.resetEditedAndSavingNodes();
    } else if (action === 'delete-question') {
      this.deleteQuestion();
    }
  }

  deleteQuestion(): void {
    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Remove Question',
        message: 'Are you sure, you want to remove question: ' + this.editedNode?.item,
      },
    });
    confirmDialog
      .afterClosed()
      .pipe(take(1))
      .subscribe(result => {
        if (result === true) {
          this.onConfirm();
        }
      });
  }

  onConfirm(): void {
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

    this.resetEditedAndSavingNodes();
  }

  save(): void {
    this.resetEditedAndSavingNodes();
    const result = this.database.flattenTreeStructure(this.database.data);
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

  ngOnDestroy(): void {
    this.dataChangeSubscription?.unsubscribe();
  }
}
