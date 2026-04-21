import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { injectMutation } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { LoginRequest } from '../../features/auth/models/login-request';
import { LoginResponse } from '../../features/auth/models/login-response';
import { ApiResponse } from '../http/api-response';
import { AuthToken } from './auth-token';
import { CurrentUser } from './current-user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _http = inject(HttpClient);
  private readonly _authToken = inject(AuthToken);
  private readonly _currentUser = inject(CurrentUser);
  private readonly _router = inject(Router);

  readonly loginMutation = injectMutation<
    ApiResponse<LoginResponse>,
    HttpErrorResponse,
    LoginRequest
  >(() => ({
    mutationFn: (payload) =>
      firstValueFrom(
        this._http.post<ApiResponse<LoginResponse>>('/auth/login', payload),
      ),
  }));

  signOut(): void {
    this._authToken.clear();
    this._currentUser.clear();
    void this._router.navigateByUrl('/login');
  }
}
