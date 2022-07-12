import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Observable, Subscription, take } from 'rxjs';
import { ConfirmDialogComponent } from 'src/app/shared/confirm-dialog/components/confirm-dialog.component';
import { ChecklistDatabase } from '../../check-list-database';
import {
  ActionEvent,
  ActionType,
  Question,
  QuestionPropertiesChange,
  TodoItemFlatNode,
  TraitAction,
  TraitActionInfo,
  TraitTypes,
} from '../../question.models';
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

  constructor(private database: ChecklistDatabase, private readonly _questionService: QuestionService, private dialog: MatDialog) {
    this.treeFlattener = new MatTreeFlattener(this.transformer, this.getLevel, this.isExpandable, this.getChildren);
    this.treeControl = new FlatTreeControl<TodoItemFlatNode>(this.getLevel, this.isExpandable);
    this.dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

    this.dataChangeSubscription = database.dataChange.subscribe(data => {
      this.dataSource.data = data;
    });
  }

  getLevel = (node: TodoItemFlatNode) => node.level ?? 0;

  isExpandable = (node: TodoItemFlatNode) => node.expandable ?? false;

  getChildren = (node: Question): Question[] => node.subQuestions ?? [];

  transformer = (node: Question, level: number): TodoItemFlatNode => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode = existingNode && existingNode.title === node.title ? existingNode : new TodoItemFlatNode();
    flatNode.title = node.title;
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
    this.updateSaveButtonValidness(true);
    this.nodeExists = false;
  }

  saveNode(): void {
    if (!this.savingNode) {
      return;
    }

    const nestedNode = this.flatNodeMap.get(this.savingNode);

    if (this.savingNode) {
      this.database.updateItem(
        nestedNode === undefined ? new Question() : nestedNode!,
        this.editedNode?.title,
        this.editedNode?.isAnswer,
        this.editedNode?.content,
        this.editedNode?.acquireTraits,
        this.editedNode?.requiredTraits,
        this.editedNode?.parents
      );
    }
    this.resetEditedAndSavingNodes();
  }

  editItem(node: TodoItemFlatNode): void {
    this.editedNode = { ...node, parents: { ...node.parents } };
    this.savingNode = node;
    this.updateSaveButtonValidness(false);
    this.nodeExists = true;
  }

  resetEditedAndSavingNodes(): void {
    this.editedNode = undefined;
    this.savingNode = undefined;
  }

  handleDragEnd(e: any): void {
    if (this.editedNode && this.addParent && this.editedNode.parents) {
      this.editedNode.parents[e.toElement.textContent] = e.toElement.textContent;
      this.addParent = false;
    }
  }

  traitAction({ name, type, actionType }: TraitActionInfo): void {
    const traitAction: TraitAction = {
      acquiredelete: () =>
        this.editedNode ? (this.editedNode.acquireTraits = this.filterTraitsByName(this.editedNode?.acquireTraits, name)) : undefined,
      acquireadd: () =>
        this.editedNode ? (this.editedNode.acquireTraits = this.addTrait(this.editedNode?.acquireTraits, name)) : undefined,
      requireddelete: () =>
        this.editedNode ? (this.editedNode.requiredTraits = this.filterTraitsByName(this.editedNode?.requiredTraits, name)) : undefined,
      requiredadd: () =>
        this.editedNode ? (this.editedNode.requiredTraits = this.addTrait(this.editedNode?.requiredTraits, name)) : undefined,
    };

    if (type != undefined && actionType != undefined) {
      traitAction[TraitTypes[type].toLowerCase() + ActionType[actionType].toLowerCase()]();
    }
  }

  addTrait(traits: string[] | undefined, name: string): string[] | undefined {
    if (traits && !traits.includes(name)) {
      return (traits = [...traits, name]);
    }
    return traits;
  }

  filterTraitsByName(traits: string[] | undefined, name: string): string[] | undefined {
    if (traits) {
      return traits.filter(t => t !== name);
    }
    return traits;
  }

  deleteParent(parent: string): void {
    if (this.editedNode?.parents) {
      this.editedNode.parents = Object.fromEntries(Object.entries(this.editedNode.parents).filter(([, v]) => v !== parent));
    }
  }

  changeEditNodeIsAnswer({ type, isNotValid }: { type: string; isNotValid?: boolean }): void {
    if (this.editedNode) {
      this.editedNode.isAnswer = type === 'true';
    }

    this.updateSaveButtonValidness(isNotValid);
  }

  updateNode({ title, isAnswer, content, isNotValid }: QuestionPropertiesChange) {
    if (isAnswer !== undefined) {
      this.editedNode = { ...this.editedNode, title, isAnswer, content };
    }

    this.updateSaveButtonValidness(isNotValid);
  }

  updateSaveButtonValidness(isNotValid: boolean | undefined) {
    this.saveButtonIsNotValid = isNotValid;
  }

  action(action: ActionEvent) {
    if (action === ActionEvent.DragOver) {
      this.addParent = true;
    } else if (action === ActionEvent.DragLeave) {
      this.addParent = false;
    } else if (action === ActionEvent.SaveNode) {
      this.saveNode();
    } else if (action === ActionEvent.CancelEdit) {
      this.resetEditedAndSavingNodes();
    } else if (action === ActionEvent.DeleteQuestion) {
      this.deleteQuestion();
    }
  }

  deleteQuestion(): void {
    const confirmDialog = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Remove Question',
        message: 'Are you sure you want to remove question: ' + this.editedNode?.title,
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
    if (this.savingNode) {
      const question = this.flatNodeMap.get(this.savingNode);
      this.getQuestions(this._questionService.deleteQuestion(question));
      this.resetEditedAndSavingNodes();
    }
  }

  save(): void {
    const result = this.database.getFlatTree();
    this.getQuestions(this._questionService.saveQuestions(result));
    this.resetEditedAndSavingNodes();
  }

  getQuestions(observable: Observable<Object>) {
    return observable.pipe(take(1)).subscribe(() =>
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
