import axios, { AxiosHeaders, type InternalAxiosRequestConfig } from 'axios';

type AccessTokenProvider = () => Promise<string | undefined>;

let accessTokenProvider: AccessTokenProvider | undefined;

export function setAccessTokenProvider(provider?: AccessTokenProvider): void {
  accessTokenProvider = provider;
}

function getHeaderValue(headers: InternalAxiosRequestConfig['headers'] | undefined, name: string): string | undefined {
  if (!headers) {
    return undefined;
  }

  if (headers instanceof AxiosHeaders) {
    const value = headers.get(name);
    return typeof value === 'string' ? value : undefined;
  }

  const recordHeaders = headers as Record<string, string | undefined>;
  const matchingKey = Object.keys(recordHeaders).find((key) => key.toLowerCase() === name.toLowerCase());
  return matchingKey ? recordHeaders[matchingKey] : undefined;
}

function setHeaderValue(
  headers: InternalAxiosRequestConfig['headers'] | undefined,
  name: string,
  value: string
): InternalAxiosRequestConfig['headers'] {
  if (!headers) {
    return new AxiosHeaders({ [name]: value });
  }

  if (headers instanceof AxiosHeaders) {
    headers.set(name, value);
    return headers;
  }

  const axiosHeaders = AxiosHeaders.from(headers);
  axiosHeaders.set(name, value);
  return axiosHeaders;
}

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7253',
  timeout: 10000
});

httpClient.interceptors.request.use(async (config) => {
  if (!accessTokenProvider) {
    return config;
  }

  if (getHeaderValue(config.headers, 'Authorization')) {
    return config;
  }

  const accessToken = await accessTokenProvider();
  if (!accessToken) {
    return config;
  }

  config.headers = setHeaderValue(config.headers, 'Authorization', `Bearer ${accessToken}`);
  return config;
});
