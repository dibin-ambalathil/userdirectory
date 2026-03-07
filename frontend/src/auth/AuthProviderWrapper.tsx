import { Auth0Provider, useAuth0 } from '@auth0/auth0-react';
import { ReactNode, useEffect, useState } from 'react';
import { getLoginErrorMessage, loginWithCredentials } from '../api/authApi';
import { setAccessTokenProvider } from '../api/httpClient';
import { authConfig, isOidcConfigured } from './authConfig';
import { AuthStateProvider } from './useAuthState';

interface AuthProviderWrapperProps {
  children: ReactNode;
}

const localTokenStorageKey = 'userdirectory.local.accessToken';

function getStoredToken(): string | undefined {
  if (typeof window === 'undefined') {
    return undefined;
  }

  return window.localStorage.getItem(localTokenStorageKey) ?? undefined;
}

function saveToken(token: string): void {
  if (typeof window !== 'undefined') {
    window.localStorage.setItem(localTokenStorageKey, token);
  }
}

function clearToken(): void {
  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(localTokenStorageKey);
  }
}

function Auth0Bridge({ children }: AuthProviderWrapperProps): JSX.Element {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, getAccessTokenSilently, user } = useAuth0();

  useEffect(() => {
    setAccessTokenProvider(async () => {
      try {
        return await getAccessTokenSilently();
      } catch {
        return undefined;
      }
    });

    return () => {
      setAccessTokenProvider(undefined);
    };
  }, [getAccessTokenSilently]);

  return (
    <AuthStateProvider
      value={{
        isAuthenticated,
        isLoading,
        isAuthConfigured: true,
        userDisplayName: user?.name,
        login: async () => loginWithRedirect(),
        loginWithPassword: async () => 'Use SSO login in this environment.',
        logout: () => logout({ logoutParams: { returnTo: window.location.origin } }),
        getAccessToken: async () => {
          try {
            return await getAccessTokenSilently();
          } catch {
            return undefined;
          }
        }
      }}
    >
      {children}
    </AuthStateProvider>
  );
}

function DevAuthProvider({ children }: AuthProviderWrapperProps): JSX.Element {
  const [accessToken, setAccessToken] = useState<string | undefined>(() => getStoredToken());

  const isAuthenticated = !!accessToken;

  useEffect(() => {
    setAccessTokenProvider(async () => accessToken);

    return () => {
      setAccessTokenProvider(undefined);
    };
  }, [accessToken]);

  return (
    <AuthStateProvider
      value={{
        isAuthenticated,
        isLoading: false,
        isAuthConfigured: false,
        userDisplayName: isAuthenticated ? 'test@mail.com' : undefined,
        login: async () => Promise.resolve(),
        loginWithPassword: async (email: string, password: string) => {
          try {
            const response = await loginWithCredentials(email, password);
            setAccessToken(response.accessToken);
            saveToken(response.accessToken);
            return undefined;
          } catch (error) {
            return getLoginErrorMessage(error);
          }
        },
        logout: () => {
          setAccessToken(undefined);
          clearToken();
        },
        getAccessToken: async () => accessToken
      }}
    >
      {children}
    </AuthStateProvider>
  );
}

export function AuthProviderWrapper({ children }: AuthProviderWrapperProps): JSX.Element {
  if (!isOidcConfigured) {
    return <DevAuthProvider>{children}</DevAuthProvider>;
  }

  return (
    <Auth0Provider
      domain={authConfig.domain}
      clientId={authConfig.clientId}
      authorizationParams={{
        redirect_uri: authConfig.redirectUri,
        audience: authConfig.audience,
        scope: 'openid profile email'
      }}
      cacheLocation="localstorage"
      useRefreshTokens
    >
      <Auth0Bridge>{children}</Auth0Bridge>
    </Auth0Provider>
  );
}
