import { Injectable, computed, signal } from '@angular/core';
import { UserAccount } from '../models/user-account';

const USER_STORAGE_KEY = 'zeal-edu.current-user';

@Injectable({ providedIn: 'root' })
export class CurrentUser {
  private readonly _user = signal<UserAccount | null>(this.readFromStorage());

  readonly user = this._user.asReadonly();
  readonly role = computed(() => this._user()?.role);

  set(user: UserAccount): void {
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  clear(): void {
    localStorage.removeItem(USER_STORAGE_KEY);
    this._user.set(null);
  }

  private readFromStorage(): UserAccount | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserAccount;
    } catch {
      return null;
    }
  }
}
