import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Salesmen } from './salesmen';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { of } from 'rxjs';

describe('Salesmen', () => {
  let component: Salesmen;
  let fixture: ComponentFixture<Salesmen>;

  beforeEach(async () => {
    const api = {
      getSalesmen: () => of({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
      getSalesman: () => of(null)
    };
    const auth = { hasRole: () => true, user: () => ({ username: 'test', role: 'SalesManagerAdmin' }) };

    await TestBed.configureTestingModule({
      imports: [Salesmen],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null }, url: [] } } },
        { provide: ApiService, useValue: api },
        { provide: AuthService, useValue: auth }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Salesmen);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
