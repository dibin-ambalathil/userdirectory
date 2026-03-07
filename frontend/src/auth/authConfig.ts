const domain = import.meta.env.VITE_AUTH_DOMAIN ?? '';
const clientId = import.meta.env.VITE_AUTH_CLIENT_ID ?? '';
const audience = import.meta.env.VITE_AUTH_AUDIENCE ?? '';

export const authConfig = {
  domain,
  clientId,
  audience,
  redirectUri: import.meta.env.VITE_AUTH_REDIRECT_URI ?? window.location.origin
};

export const isOidcConfigured =
  authConfig.domain.trim().length > 0 &&
  authConfig.clientId.trim().length > 0 &&
  authConfig.audience.trim().length > 0;
