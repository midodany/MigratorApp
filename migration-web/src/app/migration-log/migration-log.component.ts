import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { MigrationLogService } from "./migration-log.service";
import { MatTableDataSource } from "@angular/material/table";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";

@Component({
  selector: "app-migration-log",
  templateUrl: "./migration-log.component.html",
  styleUrls: ["./migration-log.component.css"],
})
export class MigrationLogComponent implements OnInit {
  displayedColumns = [
    "objectId",
    "Domain",
    "TableName",
    "PropertyName",
    "ValidationMessage",
  ];
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  dataSource: MatTableDataSource<any>;
  public selectedValue = null;
  batches = [];
  //logObjects = [];

  constructor(
    private migrationLogService: MigrationLogService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.getBatches();
  }

  onBatchChange(): void {
    this.getLogObjects(this.selectedValue.BatchId);
  }

  applyFilter(filterValue: string) {
    filterValue = filterValue.trim();
    filterValue = filterValue.toLowerCase();
    this.dataSource.filter = filterValue;
  }

  getLogObjects(batchId: string) {
    this.migrationLogService.GetLogObjects(batchId).subscribe((data) => {
      //debugger;
      this.dataSource = new MatTableDataSource(data);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  getBatches(): void {
    this.migrationLogService.getBatches().subscribe((data) => {
      this.batches = data;
      if (this.batches.length > 0) {
        this.selectedValue = this.batches[0];
        this.getLogObjects(this.selectedValue.BatchId);
      }
    });
  }
}
