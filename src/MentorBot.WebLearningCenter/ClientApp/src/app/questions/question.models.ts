export class Question {
  id?: string;
  parentId?: string;
  index?: string;
  title: string = '';
  content?: string;
  type?: string;
  mentorMaterType: boolean[] = [false, false, false, false];
  subQuestions: Question[] = [];
}

export enum NodeType {
  'Question' = 1,
  'Answer' = 2,
}
