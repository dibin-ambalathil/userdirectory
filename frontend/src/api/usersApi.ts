import { AxiosError } from 'axios';
import { httpClient } from './httpClient';
import { CreateUserPayload, UpdateUserPayload, User } from '../types/user';

export async function getUsers(): Promise<User[]> {
  const response = await httpClient.get<User[]>('/api/users');
  return response.data;
}

export async function getUserById(id: string): Promise<User> {
  const response = await httpClient.get<User>(`/api/users/${id}`);
  return response.data;
}

export async function createUser(payload: CreateUserPayload): Promise<User> {
  const response = await httpClient.post<User>('/api/users', payload);
  return response.data;
}

export async function updateUser(id: string, payload: UpdateUserPayload): Promise<void> {
  await httpClient.put(`/api/users/${id}`, payload);
}

export async function deleteUser(id: string): Promise<void> {
  await httpClient.delete(`/api/users/${id}`);
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
