import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, timeout, throwError, TimeoutError } from 'rxjs';

const TIMEOUT_MS      = 15_000;
const LONG_TIMEOUT_MS = 120_000;

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router  = inject(Router);
  const snack   = inject(MatSnackBar);

  const show = (msg: string) =>
    snack.open(msg, 'Dismiss', { duration: 5000, panelClass: ['snack-error'] });

  const isLongTimeout = req.headers.has('X-Long-Timeout');
  const cleanReq = isLongTimeout
    ? req.clone({ headers: req.headers.delete('X-Long-Timeout') })
    : req;

  return next(cleanReq).pipe(
    timeout(isLongTimeout ? LONG_TIMEOUT_MS : TIMEOUT_MS),
    catchError(err => {
      if (err instanceof TimeoutError) {
        show('Request timed out. Please try again.');
        return throwError(() => err);
      }

      if (err instanceof HttpErrorResponse) {
        switch (err.status) {
          case 0:
            show('Cannot reach the server. Check your network connection.');
            break;
          case 400:
            show(err.error?.detail ?? err.error?.message ?? err.error?.title ?? 'Invalid request.');
            break;
          case 401:
            if (!req.url.includes('/auth/login')) {
              show('Session expired. Please log in again.');
              router.navigate(['/login']);
            }
            break;
          case 403:
            show('You do not have permission to perform this action.');
            break;
          case 404:
            show(err.error?.detail ?? err.error?.message ?? 'Resource not found.');
            break;
          case 409:
            show(err.error?.detail ?? err.error?.message ?? 'Conflict: resource already exists.');
            break;
          case 422:
            show(err.error?.message ?? 'Validation failed.');
            break;
          case 500:
            show('Internal server error. Please try again later.');
            break;
          default:
            show(`Unexpected error (${err.status}). Please try again.`);
        }
      }

      return throwError(() => err);
    })
  );
};
