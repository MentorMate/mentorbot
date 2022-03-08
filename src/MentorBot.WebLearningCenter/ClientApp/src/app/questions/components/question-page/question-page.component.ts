import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, Injectable, OnInit } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { NodeType, Question } from '../../question.models';
import { QuestionService } from '../../question.service';

/** Flat to-do item node with expandable and level information */
export class TodoItemFlatNode {
  item?: string;
  content?: string;
  level!: number;
  type?: number;
  expandable!: boolean;
  editMode: boolean = false;
}

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
    // this._questionService.getQuestions().subscribe(questions => this.dataChange.next(questions));
    this._questionService.getQuestions().subscribe(questions => this.dataChange.next(questions));
  }

  /**
   * Build the file structure tree. The `value` is the Json object, or a sub-tree of a Json object.
   * The return value is the list of `TodoItemNode`.
   */
  buildFileTree(obj: object, level: number): Question[] {
    return Object.keys(obj).reduce<Question[]>((accumulator, key) => {
      const value = (obj as any)[key];
      const node = new Question();
      node.content = value.content;

      if (value != null) {
        if (typeof value === 'object') {
          node.subQuestions = this.buildFileTree(value.subQuestions, level + 1);
        } else {
          node.content = value;
        }
      }

      return accumulator.concat(node);
    }, []);
  }

  /** Add an item to to-do list */
  insertItem(parent: Question, name: string) {
    if (parent.subQuestions) {
      parent.subQuestions.push({ title: name, subQuestions: [] } as Question);
      this.dataChange.next(this.data);
    } else {
      parent.subQuestions = new Array<Question>();
      parent.subQuestions.push({ title: name, subQuestions: [] } as Question);
      this.dataChange.next(this.data);
    }
  }

  insertTopItem() {
    this.dataChange.value.push({ title: '', subQuestions: [] } as Question);
    this.dataChange.next(this.data);
  }

  updateItem(node: Question, name: string, type: number, content: string) {
    node.title = name;
    node.type = type;
    node.content = content;
    this.dataChange.next(this.data);
  }
}

/**
 * @title Tree with checkboxes
 */
@Component({
  selector: 'app-questions',
  templateUrl: 'question-page.component.html',
  styleUrls: ['question-page.component.css'],
  providers: [ChecklistDatabase],
})
export class QuestionPageComponent {
  /** Map from flat node to nested node. This helps us finding the nested node to be modified */
  flatNodeMap = new Map<TodoItemFlatNode, Question>();

  /** Map from nested node to flattened node. This helps us to keep the same object for selection */
  nestedNodeMap = new Map<Question, TodoItemFlatNode>();

  /** A selected parent node to be inserted */
  selectedParent: TodoItemFlatNode | null = null;

  /** The new item's name */
  newItemName = '';

  treeControl: FlatTreeControl<TodoItemFlatNode>;

  treeFlattener: MatTreeFlattener<Question, TodoItemFlatNode>;

  dataSource: MatTreeFlatDataSource<Question, TodoItemFlatNode>;

  /** The selection for checklist */
  checklistSelection = new SelectionModel<TodoItemFlatNode>(true /* multiple */);

  public nodeType = Object.values(NodeType).filter(value => typeof value === 'string');

  constructor(private database: ChecklistDatabase, private readonly _questionService: QuestionService) {
    this.treeFlattener = new MatTreeFlattener(this.transformer, this.getLevel, this.isExpandable, this.getChildren);
    this.treeControl = new FlatTreeControl<TodoItemFlatNode>(this.getLevel, this.isExpandable);
    this.dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

    database.dataChange.subscribe(data => {
      this.dataSource.data = data;
    });
  }

  getLevel = (node: TodoItemFlatNode) => node.level;

  isExpandable = (node: TodoItemFlatNode) => node.expandable;

  getChildren = (node: Question): Question[] => node.subQuestions;

  hasNoContent = (_: number, _nodeData: TodoItemFlatNode) => _nodeData.item === '' || _nodeData.editMode;

  hasChild = (_: number, _nodeData: TodoItemFlatNode) => _nodeData.expandable && !_nodeData.editMode;

  /**
   * Transformer to convert nested node to flat node. Record the nodes in maps for later use.
   */
  transformer = (node: Question, level: number) => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode = existingNode && existingNode.item === node.title ? existingNode : new TodoItemFlatNode();
    flatNode.item = node.title;
    flatNode.content = node.content;
    flatNode.level = level;
    flatNode.expandable = !!node.subQuestions;
    flatNode.type = node.type;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  /** Whether all the descendants of the node are selected */
  descendantsAllSelected(node: TodoItemFlatNode): boolean {
    const descendants = this.treeControl.getDescendants(node);
    return descendants.every(child => this.checklistSelection.isSelected(child));
  }

  /** Whether part of the descendants are selected */
  descendantsPartiallySelected(node: TodoItemFlatNode): boolean {
    const descendants = this.treeControl.getDescendants(node);
    const result = descendants.some(child => this.checklistSelection.isSelected(child));
    return result && !this.descendantsAllSelected(node);
  }

  /** Toggle the to-do item selection. Select/deselect all the descendants node */
  todoItemSelectionToggle(node: TodoItemFlatNode): void {
    this.checklistSelection.toggle(node);
    const descendants = this.treeControl.getDescendants(node);
    this.checklistSelection.isSelected(node)
      ? this.checklistSelection.select(...descendants)
      : this.checklistSelection.deselect(...descendants);
  }

  /** Select the category so we can insert the new item. */
  addNewItem(node: TodoItemFlatNode) {
    const parentNode = this.flatNodeMap.get(node);
    this.database.insertItem(parentNode!, '');
    // if (!this.treeControl.isExpandable(node)) {
    //   node.expandable = true;
    // }
    this.treeControl.expand(node);
  }

  addNewTopItem() {
    this.database.insertTopItem();
  }

  /** Save the node to database */
  saveNode(node: TodoItemFlatNode, itemValue: string, itemType: string, itemContent: string) {
    if (itemType != 'Answer') {
      itemContent = '';
    }
    node.expandable = true;
    const nestedNode = this.flatNodeMap.get(node);
    var type: NodeType = (<any>NodeType)[itemType];
    node.editMode = false;
    this.database.updateItem(nestedNode!, itemValue, type, itemContent);
  }

  editItem(node: TodoItemFlatNode) {
    console.log(node);
    console.log(this.hasNoContent(1, node));
    node.editMode = true;
    console.log(this.hasNoContent(1, node));
  }

  save() {
    console.log(this.database.data);
    this._questionService.saveQuestions(this.database.data).subscribe();
  }
}

/**  Copyright 2018 Google Inc. All Rights Reserved.
    Use of this source code is governed by an MIT-style license that
    can be found in the LICENSE file at http://angular.io/license */
