import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TraitAction } from '../../question.models';

@Component({
  selector: 'app-question-traits',
  templateUrl: 'question-traits.component.html',
  styleUrls: ['question-traits.component.scss'],
})
export class QuestionTraitsComponent {
  @Input() title: string = '';
  @Input() type: string = '';
  @Input() traits?: string[];
  @Output() traitEvent = new EventEmitter<TraitAction>();

  deleteTrait(trait: string): void {
    this.traitEvent.emit({ name: trait, type: this.type, actionType: 'delete' });
  }

  addTrait(input: HTMLInputElement): void {
    this.traitEvent.emit({ name: input.value, type: this.type, actionType: 'add' });
    input.value = '';
  }

  resetTraitFieldInput(input: HTMLInputElement): void {
    input.value = '';
  }
}
