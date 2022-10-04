import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActiveRuleComponent } from './active-rule.component';

describe('ActiveRuleComponent', () => {
  let component: ActiveRuleComponent;
  let fixture: ComponentFixture<ActiveRuleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ActiveRuleComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActiveRuleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
