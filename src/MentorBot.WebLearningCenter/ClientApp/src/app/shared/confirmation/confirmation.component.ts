import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DialogService } from './dialog.service';

@Component({
  selector: 'confirmation',
  templateUrl: './confirmation.component.html',
  styleUrls: ['./confirmation.component.scss'],
})
export class ConfirmationComponent {
  constructor(private dialogService: DialogService) {}

  @Input() message: string = '';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onConfirm() {
    this.dialogService.isConfirmed$.next(true);
    this.confirm.emit();
  }

  onCancel() {
    this.dialogService.isConfirmed$.next(false);
    this.cancel.emit();
  }
}
