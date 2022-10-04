import { TestBed } from '@angular/core/testing';

import { ActiveRuleService } from './active-rule.service';

describe('ActiveRuleService', () => {
  let service: ActiveRuleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ActiveRuleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
