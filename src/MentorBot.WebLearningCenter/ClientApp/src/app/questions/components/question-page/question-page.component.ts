import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, Injectable, OnInit } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { NodeType, Question } from '../../question.models';
import { QuestionService } from '../../question.service';

/** Flat to-do item node with expandable and level information */
export class TodoItemFlatNode {
  id?: string;
  item?: string;
  content?: string;
  level!: number;
  expandable!: boolean;
  editMode: boolean = false;
  index?: string;
  parents: { [key: string]: string } = {};
  requiredTraits: string[] = [];
  acquireTraits: string[] = [];
  isAnswer: boolean = false;
  subQuestions: Question[] = [];
  isEdited: boolean = false;
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
  insertItem(isAnswer: boolean, requiredTraits: string[], acquireTraits: string[], parents: string[], name: string, content: string) {
    var node = {
      title: name,
      content: content,
      subQuestions: [] as Question[],
      parents: {} as { [key: string]: string },
      requiredTraits: requiredTraits,
      acquireTraits: acquireTraits,
      isAnswer: isAnswer,
      isEdited: false,
    };

    if (parents) {
      parents.forEach(element => {
        node.parents[element] = 'new parent';
      });

      this.dataChange.value.push(node);
    }
    this.dataChange.next(this.data);
  }

  insertTopItem(isAnswer: boolean, requiredTraits: string[], acquireTraits: string[], name: string, content: string) {
    this.dataChange.value.push({
      title: name,
      subQuestions: [],
      parents: {},
      requiredTraits: requiredTraits,
      acquireTraits: acquireTraits,
      isAnswer: isAnswer,
      isEdited: false,
    } as Question);
    this.dataChange.next(this.data);
  }

  updateItem(
    node: Question,
    name: string,
    isAnswer: boolean,
    content: string,
    addedParents: string[],
    addedAcquireTraits: string[],
    addedRequiredTraits: string[],
    deletedParents: string[],
    deletedAcquireTraits: string[],
    deletedRequiredTraits: string[]
  ) {
    node.title = name;
    node.content = content;
    node.isAnswer = isAnswer;
    node.isEdited = true;
    addedParents.forEach(element => {
      node.parents[element] = 'new parent';
    });
    addedAcquireTraits.forEach(element => {
      if (!node.acquireTraits.some(e => e === element)) {
        node.acquireTraits.push(element);
      }
    });
    addedRequiredTraits.forEach(element => {
      if (!node.requiredTraits.some(e => e === element)) {
        node.requiredTraits.push(element);
      }
    });
    deletedParents.forEach(element => {
      var elementToDelete = Object.keys(node.parents).find(key => node.parents[key] === element);
      if (elementToDelete) {
        delete node.parents[elementToDelete];
      }
    });
    deletedAcquireTraits.forEach(element => {
      var deleteIndex = node.acquireTraits.indexOf(element);
      node.acquireTraits.splice(deleteIndex, 1);
    });
    deletedRequiredTraits.forEach(element => {
      var deleteIndex = node.requiredTraits.indexOf(element);
      node.requiredTraits.splice(deleteIndex, 1);
    });

    this.dataChange.next(this.data);
  }
}

/**
 * @title Tree with checkboxes
 */
@Component({
  selector: 'app-questions',
  templateUrl: 'question-page.component.html',
  styleUrls: ['question-page.component.scss'],
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

  editedNode?: TodoItemFlatNode;

  dragAndDrop: string = '';

  dragAndDropElement: any;

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
    // if (!!node.mentorMaterType) {
    //   flatNode.mentorMaterType = node.mentorMaterType;
    // }
    //   if(Object.prototype.toString.call(node.parents) !== '[object Array]') {
    //     var nodeParent = Object.assign({}, node.parents );
    //     flatNode.parents.push(nodeParent);
    // }
    flatNode.id = node.id;
    flatNode.acquireTraits = node.acquireTraits;
    flatNode.requiredTraits = node.requiredTraits;
    flatNode.parents = node.parents;
    flatNode.isAnswer = node.isAnswer;
    flatNode.level = level;
    flatNode.expandable = !!node.subQuestions;
    // flatNode.type = node.type;
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
  addNewItem() {
    var addTraits = document.getElementsByClassName('add-trait');
    for (var i = 0; i < addTraits.length; i++) {
      addTraits[i].remove();
      i--;
    }
    this.editedNode = {} as TodoItemFlatNode;
  }

  addNewTopItem() {
    var addTraits = document.getElementsByClassName('add-trait');
    for (var i = 0; i < addTraits.length; i++) {
      addTraits[i].remove();
      i--;
    }
    this.editedNode = {} as TodoItemFlatNode;
    // this.database.insertTopItem();
  }

  /** Save the node to database */
  //this will take htmlElement with paragraphs containing the newly added parents
  saveNode(
    node: TodoItemFlatNode | undefined,
    itemValue: string,
    answer: string,
    itemContent: string,
    parentsElement: any,
    acquireTraitsElement: any,
    requiredTraitsElement: any,
    deletedParentsElement: any,
    deletedAcquireTraitsElement: any,
    deletedRequiredTraitsElement: any
  ) {
    var isAnswer = answer === 'true';
    console.log(isAnswer);
    if (!isAnswer) {
      itemContent = '';
    }
    this.editedNode = undefined;
    (node as TodoItemFlatNode).expandable = true;
    (node as TodoItemFlatNode).editMode = false;
    var addedParents: string[] = [];
    if (parentsElement != null) {
      for (var i = 0; i < parentsElement.children.length; i++) {
        addedParents.push(parentsElement.children[i].textContent ?? '');
      }
    }
    var addedAcquireTraits: string[] = [];
    if (acquireTraitsElement != null) {
      for (var i = 0; i < acquireTraitsElement.children.length; i++) {
        addedAcquireTraits.push(acquireTraitsElement.children[i].textContent ?? '');
      }
    }
    var addedRequiredTraits: string[] = [];
    if (requiredTraitsElement != null) {
      for (var i = 0; i < requiredTraitsElement.children.length; i++) {
        addedRequiredTraits.push(requiredTraitsElement.children[i].textContent ?? '');
      }
    }
    var deletedParents: string[] = [];
    if (deletedParentsElement != null) {
      for (var i = 0; i < deletedParentsElement.children.length; i++) {
        deletedParents.push(deletedParentsElement.children[i].textContent ?? '');
      }
    }
    var deletedAcquireTraits: string[] = [];
    if (deletedAcquireTraitsElement != null) {
      for (var i = 0; i < deletedAcquireTraitsElement.children.length; i++) {
        deletedAcquireTraits.push(deletedAcquireTraitsElement.children[i].textContent ?? '');
      }
    }
    var deletedRequiredTraits: string[] = [];
    if (deletedRequiredTraitsElement != null) {
      for (var i = 0; i < deletedRequiredTraitsElement.children.length; i++) {
        deletedRequiredTraits.push(deletedRequiredTraitsElement.children[i].textContent ?? '');
      }
    }

    const nestedNode = this.flatNodeMap.get(node as TodoItemFlatNode);
    if (nestedNode === undefined) {
      if (addedParents.length === 0) {
        this.database.insertTopItem(isAnswer, addedRequiredTraits, addedAcquireTraits, itemValue, itemContent);
      } else {
        this.database.insertItem(isAnswer, addedRequiredTraits, addedAcquireTraits, addedParents, itemValue, itemContent);
      }
    } else {
      this.database.updateItem(
        nestedNode!,
        itemValue,
        isAnswer,
        itemContent,
        addedParents,
        addedAcquireTraits,
        addedRequiredTraits,
        deletedParents,
        deletedAcquireTraits,
        deletedRequiredTraits
      );
    }
  }

  editItem(node: TodoItemFlatNode) {
    this.editedNode = node;
    var addTraits = document.getElementsByClassName('add-trait');
    for (var i = 0; i < addTraits.length; i++) {
      addTraits[i].remove();
      i--;
    }
  }

  cancelEdit() {
    this.editedNode = undefined;
  }

  handleDragEnd(e: any) {
    e.preventDefault();
    var childNode = document.createElement('p');
    childNode.textContent = e.toElement.textContent;
    this.dragAndDropElement.appendChild(childNode);
    this.dragAndDropElement = null;
  }

  handleDropEvent(e: any) {
    e.preventDefault();
    // this.dragAndDropElement = e.toElement;
    this.dragAndDropElement = document.getElementById('addedParentQuestions');
  }

  prepareForDelete(value: string, element: any) {
    var li = document.createElement('li');
    li.className = 'list-experience-item me-2 mb-3 add-trait';
    li.textContent = value;
    element.appendChild(li);
  }

  addTrait(ulElement: any, input: any) {
    var list = ulElement.childNodes as Array<any>;
    var exists = false;
    list.forEach(element => {
      console.log(element);
      if (element.textContent === input.value) {
        exists = true;
      }
    });
    if (!exists) {
      var li = document.createElement('li');
      li.className = 'list-experience-item me-2 mb-3 add-trait';
      li.textContent = input.value;
      ulElement.appendChild(li);
    }

    input.value = '';
  }

  resetTraitFieldInput(input: any) {
    input.value = '';
  }

  changeEditNodeIsAnswer(isAnswer: any) {
    (this.editedNode as TodoItemFlatNode).isAnswer = (isAnswer as HTMLTextAreaElement).value === 'true';
  }

  save() {
    console.log(this.database.data);
    this._questionService
      .saveQuestions(this.database.data)
      .subscribe(d => this._questionService.getQuestions().subscribe(questions => this.database.dataChange.next(questions)));
  }
}

/**  Copyright 2018 Google Inc. All Rights Reserved.
    Use of this source code is governed by an MIT-style license that
    can be found in the LICENSE file at http://angular.io/license */
