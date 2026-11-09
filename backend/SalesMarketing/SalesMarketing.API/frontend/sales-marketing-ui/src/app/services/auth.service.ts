import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, of, tap } from 'rxjs';
import { ApiService } from './api.service';
import { CurrentUser, UserRole } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  readonly user = signal<CurrentUser | null>(null);
  readonly loading = signal(false);

  loadSession() {
    return this.api.me().pipe(
      map((user) => ({ username: user.username, role: user.role as UserRole })),
      tap((user) => this.user.set(user)),
      catchError(() => {
        this.user.set(null);
        return of(null);
      }),
    );
  }

  login(username: string, password: string) {
    this.loading.set(true);
    return this.api.login(username, password).pipe(
      tap((response) => this.user.set({ username: response.username, role: response.role as UserRole })),
      tap(() => this.loading.set(false)),
      catchError((error) => {
        this.loading.set(false);
        throw error;
      }),
    );
  }

  logout() {
    return this.api.logout().pipe(
      tap(() => {
        this.user.set(null);
        void this.router.navigate(['/login']);
      }),
      catchError(() => {
        this.user.set(null);
        void this.router.navigate(['/login']);
        return of(null);
      }),
    );
  }

  hasRole(role: UserRole) {
    return this.user()?.role === role;
  }
}
