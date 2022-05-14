import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NewRuleComponent } from './new-rule.component';

describe('NewRuleComponent', () => {
  let component: NewRuleComponent;
  let fixture: ComponentFixture<NewRuleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NewRuleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewRuleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
