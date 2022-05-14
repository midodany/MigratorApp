import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ControlPanelComponent } from './control-panel/control-panel.component';
import { NewRuleComponent } from './new-rule/new-rule.component';


const routes: Routes = [
  {path: 'ActiveRules', component: ControlPanelComponent},
  {path: 'NewRule', component: NewRuleComponent},
  {path: '', redirectTo:'/ActiveRules', pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
