import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const apiBase = environment.apiUrl;

  return next(request.clone({ withCredentials: true })).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && request.url.startsWith(apiBase) && !request.url.endsWith('/auth/login')) {
        void router.navigate(['/login'], { queryParams: { expired: '1' } });
      }

      return throwError(() => error);
    })
  );
};
