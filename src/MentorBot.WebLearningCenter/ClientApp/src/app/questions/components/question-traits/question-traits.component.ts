import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-question-traits',
  templateUrl: 'question-traits.component.html',
  styleUrls: ['question-traits.component.scss'],
})
export class QuestionTraitsComponent {
  @Input() title: string = '';
  @Input() type: string = '';
  @Input() traits?: string[];
  @Output() deleteTraitEvent = new EventEmitter<{ name: string; type: string }>();
  @Output() addTraitEvent = new EventEmitter<{ name: string; type: string }>();

  deleteRequireTrait(trait: string) {
    this.deleteTraitEvent.emit({ name: trait, type: this.type });
  }

  addTrait(input: HTMLInputElement) {
    this.addTraitEvent.emit({ name: input.value, type: this.type });
    input.value = '';
  }

  resetTraitFieldInput(input: HTMLInputElement) {
    input.value = '';
  }
}
