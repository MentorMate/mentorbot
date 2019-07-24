import { Component, OnInit } from '@angular/core';
import { SettingsService } from './settings.service';
import { ProcessorSettings } from './settings.models';

const template = `
  <div *ngIf="processors">
    <form
      class="settings-form"
      #settingsForm="ngForm">
      <h2>Command processors</h2>
      <div *ngFor="let processor of processors">
        <label class="switch">
          <input type="checkbox"
            name="{{ processor.name }}"
            [(ngModel)]="processor.enabled"
            [checked]="processor.enabled"
           >
          <span class="slider round"></span>
          <span class="text">{{ processor.name }}</span>
        </label>
        <table *ngIf="processor.enabled && processor.data && processor.data.length">
          <tr *ngFor="let config of processor.data">
            <td>{{config.key}}:</td>
            <td><input type="text"
                     [name]="config.key"
                     [(ngModel)]="config.value"></td>
          </tr>
        </table>
      </div>

      <button
        class="btn btn-primary"
        [disabled]="!settingsForm.dirty"
        (click)="onSave(settingsForm)">
        Save
      </button>
    </form>
  </div>
`;

const style = `
  .settings-form {
    padding: 20px;
  }

  .switch {
    position: relative;
    display: inline-block;
    width: 50px;
    height: 37px;
  }

  .switch input { 
    opacity: 0;
    width: 0;
    height: 0;
  }

  .slider {
    position: absolute;
    cursor: pointer;
    top: 10px;
    left: 20px;
    right: 0;
    bottom: 10px;
    background-color: #ccc;
    -webkit-transition: .4s;
    transition: .4s;
  }

  .slider:before {
    position: absolute;
    content: "";
    height: 13px;
    width: 13px;
    left: 2px;
    bottom: 2px;
    background-color: white;
    -webkit-transition: .4s;
    transition: .4s;
  }

  input:checked + .slider {
    background-color: #2196F3;
  }

  input:focus + .slider {
    box-shadow: 0 0 1px #2196F3;
  }

  input:checked + .slider:before {
    -webkit-transform: translateX(13px);
    -ms-transform: translateX(13px);
    transform: translateX(13px);
  }

  /* Rounded sliders */
  .slider.round {
    border-radius: 17px;
  }

  .slider.round:before {
    border-radius: 50%;
  }

  .switch .text {
    position: absolute;
    left: 60px;
    top: 9px;
  }
`;

@Component({
  selector: 'lp-settings',
  template: template,
  styles: [style]
})
export class SettingsPageComponent implements OnInit {
  processors: ProcessorSettings[];

  constructor(
    private service: SettingsService) { }

  ngOnInit(): void {
    this.service.getSettings().subscribe(
      data => {
        this.processors = data;
    });
  }

  onSave(form): void {
    this.service.saveSettings(this.processors).subscribe(
      data => { },
      error => {
        console.log("Error", error);
      }
    );
  }
}
