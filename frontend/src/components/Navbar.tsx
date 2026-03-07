import { NavLink } from 'react-router-dom';
import { useAuthState } from '../auth/useAuthState';

export function Navbar(): JSX.Element {
  const auth = useAuthState();

  return (
    <header className="top-nav">
      <div className="brand-block">
        <span className="brand-title">User Directory</span>
      </div>

      <nav className="nav-links" aria-label="Main navigation">
        <NavLink className={({ isActive }) => (isActive ? 'nav-link nav-link-active' : 'nav-link')} to="/users/add">
          Add
        </NavLink>
        <NavLink className={({ isActive }) => (isActive ? 'nav-link nav-link-active' : 'nav-link')} to="/users">
          List
        </NavLink>
      </nav>

      <div className="auth-actions">
        {auth.isAuthenticated ? (
          <button className="secondary-button" onClick={auth.logout} type="button">
            Logout
          </button>
        ) : auth.isAuthConfigured ? (
          <button className="primary-button" onClick={() => void auth.login()} type="button">
            Login
          </button>
        ) : (
          <>
            <NavLink className={({ isActive }) => (isActive ? 'nav-link nav-link-active' : 'nav-link')} to="/login">
              Login
            </NavLink>
            <span className="dev-badge">Local auth mode</span>
          </>
        )}
      </div>
    </header>
  );
}
