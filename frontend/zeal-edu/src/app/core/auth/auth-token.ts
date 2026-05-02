import { Injectable, computed, signal } from '@angular/core';

const TOKEN_STORAGE_KEY = 'zeal-edu.auth-token';

interface StoredToken {
  token: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthToken {
  private readonly _data = signal<StoredToken | null>(this.readFromStorage());

  readonly token = computed(() => this._data()?.token ?? null);
  readonly expiresAt = computed(() => {
    const raw = this._data()?.expiresAt;
    return raw ? new Date(raw) : null;
  });

  set(token: string, expiresAt: string): void {
    const data: StoredToken = { token, expiresAt };
    localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(data));
    this._data.set(data);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this._data.set(null);
  }

  private readFromStorage(): StoredToken | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw) as StoredToken;
      if (parsed && typeof parsed.token === 'string' && typeof parsed.expiresAt === 'string') {
        return parsed;
      }
      return null;
    } catch {
      return null;
    }
  }
}
