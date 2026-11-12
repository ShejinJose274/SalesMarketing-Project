import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { Dashboard } from './dashboard';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideRouter([]),
        { provide: ApiService, useValue: { getSalesmen: () => of([]), getTeams: () => of([]) } },
        { provide: AuthService, useValue: { user: signal({ username: 'test', role: 'MasterAdmin' }), hasRole: () => true } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
