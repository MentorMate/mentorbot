import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionType, TraitActionInfo, TraitTypes } from '../../question.models';

@Component({
  selector: 'app-question-traits',
  templateUrl: 'question-traits.component.html',
  styleUrls: ['question-traits.component.scss'],
})
export class QuestionTraitsComponent {
  @Input() title: string = '';
  @Input() type?: TraitTypes;
  @Input() traits?: string[];
  @Output() traitEvent = new EventEmitter<TraitActionInfo>();

  public traitTypes = TraitTypes;

  deleteTrait(trait: string): void {
    this.emitChangeEvent(trait, ActionType.Delete);
  }

  addTrait(input: HTMLInputElement): void {
    this.emitChangeEvent(input.value, ActionType.Add);
    this.resetTraitFieldInput(input);
  }

  emitChangeEvent(name: string, actionType: ActionType) {
    this.traitEvent?.emit({ name, type: this.type, actionType });
  }

  resetTraitFieldInput(input: HTMLInputElement): void {
    input.value = '';
  }
}
