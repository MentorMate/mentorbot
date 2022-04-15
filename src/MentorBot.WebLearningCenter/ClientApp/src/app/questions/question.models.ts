export class Question {
  id?: string;
  parents: { [key: string]: string } = {};
  index?: string;
  title: string = '';
  content?: string;
  requiredTraits: string[] = [];
  acquireTraits: string[] = [];
  isAnswer: boolean = false;
  subQuestions: Question[] = [];
  isEdited: boolean = false;
}

export enum NodeType {
  Question,
  Answer,
}

/** Flat to-do item node with expandable and level information */
export class TodoItemFlatNode extends Question {
  item?: string;
  level!: number;
  expandable!: boolean;
  editMode: boolean = false;
}

export interface TraitAction {
  name: string;
  type?: TraitTypes;
  actionType?: ActionType;
}

export interface QuestionPropertiesChange {
  item?: string;
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
