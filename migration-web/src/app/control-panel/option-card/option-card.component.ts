import { Component, Input, OnInit, Output, EventEmitter, ViewChild, NgZone } from '@angular/core';
import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import { take } from 'rxjs/operators';
@Component({
  selector: 'app-option-card',
  templateUrl: './option-card.component.html',
  styleUrls: ['./option-card.component.css']
})
export class OptionCardComponent implements OnInit {

  @Input() BusinessRule;
  @Output() onRuleChange = new EventEmitter();
  public EntityName;
  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  triggerResize() {
    // Wait for changes to be applied, then trigger textarea resize.
    this._ngZone.onStable.pipe(take(1))
        .subscribe(() => this.autosize.resizeToFitContent(true));
  }
  constructor(private _ngZone: NgZone) { }

  ngOnInit(): void {
    this.EntityName = this.BusinessRule.EntityName;
  }

  setRequired(completed: boolean) {
    this.BusinessRule.IsRequired = completed;
    this.onRuleChange.emit();
  }

  setActive(completed: boolean) {
    this.BusinessRule.IsActive = completed;
    this.onRuleChange.emit();
  }

  setRegEx(text: string) {
    this.BusinessRule.RegEx = text;
    this.onRuleChange.emit();
  }
  

}
