import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MarketingTeams } from './marketing-teams';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { of } from 'rxjs';

describe('MarketingTeams', () => {
  let component: MarketingTeams;
  let fixture: ComponentFixture<MarketingTeams>;

  beforeEach(async () => {
    const api = {
      getTeams: () => of({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
      getAvailableSalesmen: () => of([]),
      getTeam: () => of(null)
    };

    await TestBed.configureTestingModule({
      imports: [MarketingTeams],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null }, url: [] } } },
        { provide: ApiService, useValue: api }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MarketingTeams);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
