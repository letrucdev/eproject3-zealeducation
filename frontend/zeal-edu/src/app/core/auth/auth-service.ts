import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { injectMutation, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { LoginRequest } from '@features/auth/models/login-request';
import { LoginResponse } from '@features/auth/models/login-response';
import { ApiResponse } from '@core/http/api-response';
import { AuthToken } from './auth-token';
import { CurrentUser } from './current-user';
import { toast } from '@spartan-ng/brain/sonner';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _http = inject(HttpClient);
  private readonly _authToken = inject(AuthToken);
  private readonly _currentUser = inject(CurrentUser);
  private readonly _queryClient = inject(QueryClient);
  private readonly _router = inject(Router);

  readonly loginMutation = injectMutation<
    ApiResponse<LoginResponse>,
    HttpErrorResponse,
    LoginRequest
  >(() => ({
    mutationFn: (payload) =>
      firstValueFrom(this._http.post<ApiResponse<LoginResponse>>('/auth/login', payload)),
  }));

  readonly changePasswordMutation = injectMutation<
    ApiResponse<null>,
    HttpErrorResponse,
    { currentPassword: string; newPassword: string }
  >(() => ({
    mutationFn: (payload) =>
      firstValueFrom(this._http.post<ApiResponse<null>>('/auth/change-password', payload)),
    onSuccess: () => {
      toast.success('Password updated. Please sign in again with your new password.');
      this.signOut();
    },
  }));

  signOut(): void {
    this._authToken.clear();
    this._currentUser.clear();
    this._queryClient.clear();
    void this._router.navigateByUrl('/login');
  }
}
