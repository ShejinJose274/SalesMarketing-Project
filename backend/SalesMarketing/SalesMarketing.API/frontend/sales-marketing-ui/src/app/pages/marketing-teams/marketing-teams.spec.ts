import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MarketingTeams } from './marketing-teams';
import { ActivatedRoute } from '@angular/router';

describe('MarketingTeams', () => {
  let component: MarketingTeams;
  let fixture: ComponentFixture<MarketingTeams>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MarketingTeams],
      providers: [{ provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null }, url: [] } } }],
    }).compileComponents();

    fixture = TestBed.createComponent(MarketingTeams);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
