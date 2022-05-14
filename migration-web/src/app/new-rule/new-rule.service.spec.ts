import { TestBed } from '@angular/core/testing';

import { NewRuleService } from './new-rule.service';

describe('NewRuleService', () => {
  let service: NewRuleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NewRuleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
