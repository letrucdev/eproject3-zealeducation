import { UserAccount } from '@core/models/user-account';

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserAccount;
}
