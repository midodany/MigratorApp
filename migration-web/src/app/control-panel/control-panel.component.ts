import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-control-panel',
  templateUrl: './control-panel.component.html',
  styleUrls: ['./control-panel.component.css']
})
export class ControlPanelComponent{

  selected = 0;

  rowClicked(row: any) {

    console.log(row.IsActive,row.TableName,row.PropertyName,row.Domain,row.RuleType);

    if(row.RuleType == 2) {
      this.selected = 2;
    }
    else {
      this.selected = row.IsActive ? 0 : 1;
    }
  }

  constructor() { }

}
