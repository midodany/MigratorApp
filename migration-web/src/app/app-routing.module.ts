import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ControlPanelComponent } from './control-panel/control-panel.component';
import { MigrationLogComponent } from './migration-log/migration-log.component';
import { NewRuleComponent } from './new-rule/new-rule.component';
import { RelationRuleComponent } from './relation-rule/relation-rule.component';


const routes: Routes = [
  {path: 'ActiveRules', component: ControlPanelComponent},
  {path: 'Log', component: MigrationLogComponent},
  {path: 'NewRule', component: NewRuleComponent},
  {path: 'RelationRules', component: RelationRuleComponent},
  {path: '', redirectTo:'/ActiveRules', pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
