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
  'Question' = 1,
  'Answer' = 2,
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
  type: string;
  actionType: string;
}

export interface QuestionPropertiesChange {
  title?: string;
  type?: string;
  content?: string;
  isNotValid?: boolean;
}
