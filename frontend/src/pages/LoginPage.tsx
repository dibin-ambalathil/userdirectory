import { FormEvent, useEffect, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuthState } from '../auth/useAuthState';
import { LoadingSpinner } from '../components/LoadingSpinner';

interface LoginLocationState {
  from?: string;
}

export function LoginPage(): JSX.Element {
  const auth = useAuthState();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (auth.isAuthConfigured && !auth.isAuthenticated) {
      void auth.login();
    }
  }, [auth.isAuthConfigured, auth.isAuthenticated, auth.login]);

  if (auth.isLoading) {
    return <LoadingSpinner label="Preparing sign in..." />;
  }

  if (auth.isAuthenticated) {
    const locationState = (location.state as LoginLocationState | null) ?? null;
    return <Navigate to={locationState?.from ?? '/users'} replace />;
  }

  if (auth.isAuthConfigured) {
    return <LoadingSpinner label="Redirecting to sign in..." />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setError(null);

    const normalizedEmail = email.trim();
    if (!normalizedEmail || !password) {
      setError('Email and password are required.');
      return;
    }

    setIsSubmitting(true);

    const loginError = await auth.loginWithPassword(normalizedEmail, password);
    setIsSubmitting(false);

    if (loginError) {
      setError(loginError);
      return;
    }

    const locationState = (location.state as LoginLocationState | null) ?? null;
    navigate(locationState?.from ?? '/users', { replace: true });
  }

  return (
    <section className="page-card login-card">
      <header className="page-header">
        <h1>Login</h1>
      </header>

      <form className="form-grid" onSubmit={(event) => void handleSubmit(event)} noValidate>
        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="Enter your email"
            autoComplete="email"
          />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Enter your password"
            autoComplete="current-password"
          />
        </label>

        {error ? <p className="status-message error">{error}</p> : null}

        <div className="button-row">
          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>
        </div>
      </form>
    </section>
  );
}
