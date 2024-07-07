import { TestBed } from '@angular/core/testing';

import { CargoRealtimeClientService } from './cargo-realtime-client.service';

describe('CargoRealtimeClientService', () => {
  let service: CargoRealtimeClientService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CargoRealtimeClientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
