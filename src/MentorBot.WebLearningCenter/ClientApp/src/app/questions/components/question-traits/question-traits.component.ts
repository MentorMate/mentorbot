import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionType, TraitAction, TraitTypes } from '../../question.models';

@Component({
  selector: 'app-question-traits',
  templateUrl: 'question-traits.component.html',
  styleUrls: ['question-traits.component.scss'],
})
export class QuestionTraitsComponent {
  @Input() title: string = '';
  @Input() type?: TraitTypes;
  @Input() traits?: string[];
  @Output() traitEvent = new EventEmitter<TraitAction>();

  public traitTypes = TraitTypes;

  deleteTrait(trait: string): void {
    this.traitEvent?.emit({ name: trait, type: this.type, actionType: ActionType.Delete });
  }

  addTrait(input: HTMLInputElement): void {
    this.traitEvent?.emit({ name: input.value, type: this.type, actionType: ActionType.Add });
    input.value = '';
  }

  resetTraitFieldInput(input: HTMLInputElement): void {
    input.value = '';
  }
}
