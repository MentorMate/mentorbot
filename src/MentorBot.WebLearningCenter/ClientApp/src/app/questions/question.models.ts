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
