import { Component, OnInit } from '@angular/core';
import { ActiveRuleService } from './active-rule.service';
import { Router } from '@angular/router';
import { ControlPanelService } from '../control-panel/control-panel.service';

@Component({
  selector: 'app-active-rule',
  templateUrl: './active-rule.component.html',
  styleUrls: ['./active-rule.component.css']
})
export class ActiveRuleComponent implements OnInit {

  public businessRules = [];
  public ruleChanged = false;
  public entityFilter = "";
  public propertyFilter = "";
  public selectedValue = null;
  origins = [
    {id:1, name: 'Source'},
    {id:2, name: 'Target'}
  ]

  constructor(private activeRuleService: ActiveRuleService, private controlPanelService: ControlPanelService, public router: Router) { }

  ngOnInit(): void {
    this.selectedValue = this.controlPanelService.Domain == "Target" ? this.origins[1] : this.origins[0];

    this.activeRuleService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
    })
    
    this.entityFilter = this.controlPanelService.EntityName;
    this.propertyFilter = this.controlPanelService.PropertyRelationName;


    this.controlPanelService.EntityName = "";
    this.controlPanelService.PropertyRelationName = "";
    this.controlPanelService.Domain = "";
  }

  onOriginChange():void {
    this.activeRuleService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
      this.ruleChanged = false;
    })
  }

  onRuleChange():void {
    this.ruleChanged = true;
  }

  onSave(){
    this.activeRuleService.saveData(this.businessRules).subscribe(data => {
      console.log(data);
    })
  }

}
