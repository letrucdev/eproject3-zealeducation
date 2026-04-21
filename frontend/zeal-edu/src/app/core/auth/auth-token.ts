import { Injectable, signal } from '@angular/core';

const TOKEN_STORAGE_KEY = 'zeal-edu.auth-token';

@Injectable({ providedIn: 'root' })
export class AuthToken {
  private readonly _token = signal<string | null>(this.readFromStorage());

  readonly token = this._token.asReadonly();

  set(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
    this._token.set(token);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this._token.set(null);
  }

  private readFromStorage(): string | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }
}
