import { TestBed } from '@angular/core/testing';

import { MigrationLogService } from './migration-log.service';

describe('MigrationLogService', () => {
  let service: MigrationLogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MigrationLogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
