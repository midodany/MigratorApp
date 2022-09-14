import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RelationRuleComponent } from './relation-rule.component';

describe('RelationRuleComponent', () => {
  let component: RelationRuleComponent;
  let fixture: ComponentFixture<RelationRuleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RelationRuleComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RelationRuleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
