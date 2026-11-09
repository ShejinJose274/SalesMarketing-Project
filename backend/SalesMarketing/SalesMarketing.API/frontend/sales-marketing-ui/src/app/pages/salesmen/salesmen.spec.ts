import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Salesmen } from './salesmen';
import { ActivatedRoute } from '@angular/router';

describe('Salesmen', () => {
  let component: Salesmen;
  let fixture: ComponentFixture<Salesmen>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Salesmen],
      providers: [{ provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null }, url: [] } } }],
    }).compileComponents();

    fixture = TestBed.createComponent(Salesmen);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
