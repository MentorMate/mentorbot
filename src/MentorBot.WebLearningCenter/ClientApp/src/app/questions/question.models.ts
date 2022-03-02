export interface Question {
  id?: string;
  questionId?: string;
  index: number;
  content: string;
  type: number;
  subQuestions: Question[];
}
