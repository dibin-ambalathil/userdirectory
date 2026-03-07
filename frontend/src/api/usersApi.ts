import { AxiosError } from 'axios';
import { httpClient } from './httpClient';
import { CreateUserPayload, User } from '../types/user';

export async function getUsers(): Promise<User[]> {
  const response = await httpClient.get<User[]>('/api/users');
  return response.data;
}

export async function createUser(payload: CreateUserPayload): Promise<User> {
  const response = await httpClient.post<User>('/api/users', payload);
  return response.data;
}

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    return (
      error.response?.data?.title ??
      error.response?.data?.message ??
      error.response?.data?.error ??
      error.message
    );
  }

  return 'Unexpected error. Please try again.';
}
