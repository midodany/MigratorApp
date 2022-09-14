import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RelattionCardComponent } from './relattion-card.component';

describe('RelattionCardComponent', () => {
  let component: RelattionCardComponent;
  let fixture: ComponentFixture<RelattionCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RelattionCardComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RelattionCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
