import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { Question } from './question.models';

@Injectable()
export class QuestionService {
  static readonly url = 'get-questions';

  static readonly saveQuestions = 'save-questions';

  constructor(private http: HttpClient) {}

  getQuestions(): Observable<Question[]> {
    return this.http.get<Question[]>(QuestionService.url);
  }

  saveQuestions(questions: Question[]): Observable<Object> {
    return this.http.post(QuestionService.saveQuestions, questions);
  }
}
