export class Question {
  id?: string;
  parentId?: string;
  index?: number;
  title: string = '';
  content?: string;
  type?: number;
  subQuestions: Question[] = [];
}

export enum NodeType {
  'Question' = 1,
  'Answer' = 2,
  'Category' = 3,
}
