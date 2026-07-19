import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginDto, TokenResponse } from '../models/auth.models';

export interface UserProfile {
  userId: string;
  phoneNumber: string;
  email: string | null;
  firstName: string;
  lastName: string;
  roles: string[];
  privileges: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY   = 'syncro_token';
  private readonly REFRESH_KEY = 'syncro_refresh';
  private readonly PROFILE_KEY = 'syncro_profile';

  isLoggedIn$ = signal(!!localStorage.getItem(this.TOKEN_KEY));
  userProfile = signal<UserProfile | null>(this.loadStoredProfile());

  constructor(private http: HttpClient, private router: Router) {}

  login(dto: LoginDto) {
    return this.http.post<TokenResponse>(`${environment.apiUrl}/auth/login`, dto).pipe(
      tap(res => {
        localStorage.setItem(this.TOKEN_KEY, res.accessToken);
        localStorage.setItem(this.REFRESH_KEY, res.refreshToken);
        this.isLoggedIn$.set(true);
      })
    );
  }

  /** Fetch the current user's profile from the API and cache it. */
  loadProfile() {
    return this.http.get<UserProfile>(`${environment.apiUrl}/profile`).pipe(
      tap(profile => {
        localStorage.setItem(this.PROFILE_KEY, JSON.stringify(profile));
        this.userProfile.set(profile);
      })
    );
  }

  /** Ensure profile is loaded — skips the API call if already cached. */
  ensureProfile(): void {
    if (!this.userProfile() && this.isLoggedIn()) {
      this.loadProfile().subscribe();
    }
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.PROFILE_KEY);
    this.isLoggedIn$.set(false);
    this.userProfile.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['sub'] || payload['nameid'] || null;
    } catch {
      return null;
    }
  }

  /** Full name of the signed-in user, falling back to phone number. */
  displayName(): string {
    const p = this.userProfile();
    if (!p) return '';
    return `${p.firstName} ${p.lastName}`.trim() || p.phoneNumber;
  }

  hasRole(name: string): boolean {
    return this.userProfile()?.roles.includes(name) ?? false;
  }

  hasPrivilege(code: string): boolean {
    return this.userProfile()?.privileges.includes(code) ?? false;
  }

  private loadStoredProfile(): UserProfile | null {
    try {
      const stored = localStorage.getItem(this.PROFILE_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  }
}
