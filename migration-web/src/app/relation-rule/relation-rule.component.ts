import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ControlPanelService } from '../control-panel/control-panel.service';
import { RelationRuleService } from './relation-rule.service';

@Component({
  selector: 'app-relation-rule',
  templateUrl: './relation-rule.component.html',
  styleUrls: ['./relation-rule.component.css']
})
export class RelationRuleComponent implements OnInit {

  constructor(private relationRuleService :RelationRuleService, private controlPanelService: ControlPanelService, public router: Router) { }

  public relationRules = [];
  public ruleChanged = false;
  public entityFilter = "";
  public relationFilter = "";

  ngOnInit(): void {
    this.relationRuleService.getConfig().subscribe(data => {
      this.relationRules = data;
      
      this.entityFilter = this.controlPanelService.EntityName;
      this.relationFilter = this.controlPanelService.PropertyRelationName;

      this.controlPanelService.EntityName = "";
      this.controlPanelService.PropertyRelationName = "";
    })
  }

  onRuleChange():void {
    this.ruleChanged = true;
  }

  onSave(){
    this.relationRuleService.saveData(this.relationRules).subscribe(data => {
      console.log(data);
    })
  }

}
