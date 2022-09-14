import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { Routes } from "@angular/router";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { ControlPanelComponent } from "./control-panel/control-panel.component";
import { OptionCardComponent } from "./control-panel/option-card/option-card.component";
import { FilterBR } from "./pipes/filterBR.pipe";
import { HttpClientModule } from "@angular/common/http";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { MatCheckboxModule } from "@angular/material/checkbox";
import {MatSelectModule} from '@angular/material/select';
import { MatInputModule } from "@angular/material/input";
import { FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { NewRuleComponent } from "./new-rule/new-rule.component";
import { MigrationLogComponent } from "./migration-log/migration-log.component";
import { MatTableModule } from "@angular/material/table";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { RelationRuleComponent } from './relation-rule/relation-rule.component';
import { RelattionCardComponent } from './relation-rule/relattion-card/relattion-card.component';

const appRoutes: Routes = [];

@NgModule({
  declarations: [
    AppComponent,
    ControlPanelComponent,
    OptionCardComponent,
    FilterBR,
    NewRuleComponent,
    MigrationLogComponent,
    RelationRuleComponent,
    RelattionCardComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatCheckboxModule,
    MatSelectModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
