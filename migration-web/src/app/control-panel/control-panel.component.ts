import { Component, OnInit } from '@angular/core';
import { ControlPanelService } from './contol-panel.service';
@Component({
  selector: 'app-control-panel',
  templateUrl: './control-panel.component.html',
  styleUrls: ['./control-panel.component.css']
})
export class ControlPanelComponent implements OnInit {

  public businessRules = [];
  public ruleChanged = false;

  constructor(private controlPanelService :ControlPanelService) { }

  ngOnInit(): void {
    this.controlPanelService.getConfig().subscribe(data => {
      console.log(data);
      this.businessRules = data;
    })
  }

  onRuleChange():void {
    console.log("changed");
    this.ruleChanged = true;
  }

}
