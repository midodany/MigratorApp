import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ControlPanelService } from '../control-panel/control-panel.service';
import { NewRuleService } from './new-rule.service';

@Component({
  selector: 'app-new-rule',
  templateUrl: './new-rule.component.html',
  styleUrls: ['./new-rule.component.css']
})
export class NewRuleComponent implements OnInit {

  public businessRules = [];
  public ruleChanged = false;
  public entityFilter = "";
  public propertyFilter = "";
  public selectedValue = null;
  origins = [
    {id:1, name: 'Source'},
    {id:2, name: 'Target'}
  ]

  constructor(private newRuleService :NewRuleService, private controlPanelService: ControlPanelService, public router: Router) { }

  ngOnInit(): void {
    this.selectedValue = this.controlPanelService.Domain == "Target" ? this.origins[1] : this.origins[0];
    this.newRuleService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
    })


    this.entityFilter = this.controlPanelService.EntityName;
    this.propertyFilter = this.controlPanelService.PropertyRelationName;


    this.controlPanelService.EntityName = "";
    this.controlPanelService.PropertyRelationName = "";
    this.controlPanelService.Domain = "";
  }

  onOriginChange():void {
    this.newRuleService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
      this.ruleChanged = false;
    })
  }

  onRuleChange():void {
    this.ruleChanged = true;
  }

  onSave(){
    this.newRuleService.saveData(this.businessRules).subscribe(data => {
      console.log(data);
    })
  }

}
