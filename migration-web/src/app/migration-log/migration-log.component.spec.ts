import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MigrationLogComponent } from './migration-log.component';

describe('MigrationLogComponent', () => {
  let component: MigrationLogComponent;
  let fixture: ComponentFixture<MigrationLogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MigrationLogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MigrationLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
