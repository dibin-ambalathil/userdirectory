import { AxiosError } from 'axios';
import { httpClient } from './httpClient';

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

export async function loginWithCredentials(email: string, password: string): Promise<LoginResponse> {
  const response = await httpClient.post<LoginResponse>('/api/auth/login', {
    email,
    password
  });

  return response.data;
}

export function getLoginErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    return (
      error.response?.data?.message ??
      error.response?.data?.title ??
      error.response?.data?.error ??
      'Invalid email or password.'
    );
  }

  return 'Unable to sign in right now. Please try again.';
}
