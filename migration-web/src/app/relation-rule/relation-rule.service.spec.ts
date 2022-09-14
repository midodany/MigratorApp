import { TestBed } from '@angular/core/testing';

import { RelationRuleService } from './relation-rule.service';

describe('RelationRuleService', () => {
  let service: RelationRuleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RelationRuleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
