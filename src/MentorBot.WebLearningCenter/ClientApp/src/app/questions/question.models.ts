import { data } from '../shared/utilities.models';

export class Question implements data<Question> {
  [childPropertyName: string]: Question[] | unknown;
  id?: string;
  parents?: { [key: string]: string } = {};
  title?: string = '';
  content?: string;
  requiredTraits?: string[] = [];
  acquireTraits?: string[] = [];
  isAnswer: boolean = false;
  subQuestions?: Question[] = [];
  isEdited?: boolean = false;
}

export enum NodeType {
  Question,
  Answer,
}

/** Flat to-do item node with expandable and level information */
export class TodoItemFlatNode extends Question {
  level?: number;
  expandable?: boolean;
  editMode?: boolean;
}

export interface TraitActionInfo {
  name: string;
  type?: TraitTypes;
  actionType?: ActionType;
}

export interface QuestionPropertiesChange {
  title?: string;
  isAnswer?: boolean;
  content?: string;
  isNotValid?: boolean;
}

export enum ActionEvent {
  DragOver,
  DragLeave,
  SaveNode,
  CancelEdit,
  DeleteQuestion,
}

export enum TraitTypes {
  Acquire,
  Required,
}

export enum ActionType {
  Add,
  Delete,
}

export interface TraitAction {
  [acquiredelete: string]: () => string[] | undefined;
  acquireadd: () => string[] | undefined;
  requireddelete: () => string[] | undefined;
  requiredadd: () => string[] | undefined;
}
