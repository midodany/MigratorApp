import { Component, OnInit } from '@angular/core';
import { ControlPanelService } from './control-panel.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-control-panel',
  templateUrl: './control-panel.component.html',
  styleUrls: ['./control-panel.component.css']
})
export class ControlPanelComponent implements OnInit {

  public businessRules = [];
  public ruleChanged = false;
  public entityFilter = "";
  public propertyFilter = "";
  public selectedValue = null;
  origins = [
    {id:1, name: 'Source'},
    {id:2, name: 'Target'}
  ]

  constructor(private controlPanelService :ControlPanelService, public router: Router) { }

  ngOnInit(): void {
    this.selectedValue = this.origins[0];
    this.controlPanelService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
    })
  }

  onOriginChange():void {
    this.controlPanelService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
      this.ruleChanged = false;
    })
  }

  onRuleChange():void {
    this.ruleChanged = true;
  }

  onSave(){
    this.controlPanelService.saveData(this.businessRules).subscribe(data => {
      console.log(data);
    })
  }

}
