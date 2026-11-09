import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  MarketingTeam,
  MarketingTeamListItem,
  MarketingTeamRequest,
  PagedResponse,
  Salesman,
  SalesmanRequest,
  TeamMemberRequest,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  login(username: string, password: string) {
    return this.http.post<{ username: string; role: string; dashboardUrl: string }>(
      `${this.baseUrl}/auth/login`,
      { username, password },
    );
  }

  getSalesman(id: number): Observable<Salesman> {
    return this.http.get<Salesman>(`${this.baseUrl}/salesmen/${id}`);
  }

  me() {
    return this.http.get<{ username: string; role: string }>(`${this.baseUrl}/auth/me`);
  }

  logout() {
    return this.http.post(`${this.baseUrl}/auth/logout`, {});
  }

  getSalesmen(query: { search?: string; status?: number; page?: number; pageSize?: number } = {}): Observable<PagedResponse<Salesman>> {
    return this.http.get<PagedResponse<Salesman>>(`${this.baseUrl}/salesmen`, { params: this.queryParams(query) });
  }

  getAvailableSalesmen(): Observable<Salesman[]> {
    return this.http.get<Salesman[]>(`${this.baseUrl}/salesmen/available`);
  }

  getSalesmenByStatus(status: number): Observable<Salesman[]> {
    return this.http.get<Salesman[]>(`${this.baseUrl}/salesmen/status/${status}`);
  }

  createSalesman(request: SalesmanRequest) {
    return this.http.post<Salesman>(`${this.baseUrl}/salesmen`, request);
  }

  updateSalesman(id: number, request: SalesmanRequest) {
    return this.http.put<{ message: string }>(`${this.baseUrl}/salesmen/${id}`, request);
  }

  changeSalesmanStatus(id: number, status: number) {
    const params = new HttpParams().set('status', status);
    return this.http.patch<{ message: string }>(`${this.baseUrl}/salesmen/${id}/status`, {}, { params });
  }

  deleteSalesman(id: number) {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/salesmen/${id}`);
  }

  getTeams(query: { search?: string; status?: number; page?: number; pageSize?: number } = {}): Observable<PagedResponse<MarketingTeamListItem>> {
    return this.http.get<PagedResponse<MarketingTeamListItem>>(`${this.baseUrl}/marketingteams`, { params: this.queryParams(query) });
  }

  getTeam(id: number): Observable<MarketingTeam> {
    return this.http.get<MarketingTeam>(`${this.baseUrl}/marketingteams/${id}`);
  }

  createTeam(request: MarketingTeamRequest) {
    return this.http.post<MarketingTeam>(`${this.baseUrl}/marketingteams`, request);
  }

  updateTeam(id: number, request: MarketingTeamRequest) {
    return this.http.put<{ message: string }>(`${this.baseUrl}/marketingteams/${id}`, request);
  }

  addTeamMember(teamId: number, request: TeamMemberRequest) {
    return this.http.post<{ message: string }>(`${this.baseUrl}/marketingteams/${teamId}/members`, request);
  }

  changeTeamStatus(id: number, status: number) {
    const params = new HttpParams().set('status', status);
    return this.http.patch<{ message: string }>(`${this.baseUrl}/marketingteams/${id}/status`, {}, { params });
  }

  deleteTeam(id: number) {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/marketingteams/${id}`);
  }

  errorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.message ?? error.message ?? 'The request could not be completed.';
    }

    return 'The request could not be completed.';
  }

  private queryParams(query: Record<string, string | number | undefined>): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== '') params = params.set(key, value);
    }
    return params;
  }
}
