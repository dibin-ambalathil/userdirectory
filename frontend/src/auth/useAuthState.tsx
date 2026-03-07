import { createContext, useContext } from 'react';

export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  isAuthConfigured: boolean;
  userDisplayName?: string;
  login: () => Promise<void>;
  loginWithPassword: (email: string, password: string) => Promise<string | undefined>;
  logout: () => void;
  getAccessToken: () => Promise<string | undefined>;
}

const defaultAuthState: AuthState = {
  isAuthenticated: true,
  isLoading: false,
  isAuthConfigured: false,
  userDisplayName: undefined,
  login: async () => Promise.resolve(),
  loginWithPassword: async () => 'Email/password login is unavailable.',
  logout: () => undefined,
  getAccessToken: async () => undefined
};

const AuthStateContext = createContext<AuthState>(defaultAuthState);

export const AuthStateProvider = AuthStateContext.Provider;

export function useAuthState(): AuthState {
  return useContext(AuthStateContext);
}
