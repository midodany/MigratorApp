import { Component, OnInit } from '@angular/core';
import { ActiveRuleService } from './active-rule.service';
import { Router } from '@angular/router';

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

  constructor(private activeRuleService: ActiveRuleService, public router: Router) { }

  ngOnInit(): void {
    this.selectedValue = this.origins[0];
    this.activeRuleService.getConfig(this.selectedValue.name).subscribe(data => {
      this.businessRules = data;
    })
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
