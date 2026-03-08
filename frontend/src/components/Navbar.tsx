import { NavLink } from 'react-router-dom';
import { useAuthState } from '../auth/useAuthState';

const navLinkClassName = ({ isActive }: { isActive: boolean }): string =>
  isActive ? 'nav-link nav-link-active' : 'nav-link';

const navItems = [
  { to: '/users/add', label: 'Add' },
  { to: '/users', label: 'List' }
] as const;

export function Navbar(): JSX.Element {
  const auth = useAuthState();

  const loginAction = auth.isAuthConfigured ? (
    <button className="primary-button" onClick={() => void auth.login()} type="button">
      Login
    </button>
  ) : (
    <NavLink className={navLinkClassName} to="/login">
      Login
    </NavLink>
  );

  return (
    <header className="top-nav">
      <div className="brand-block">
        <span className="brand-title">User Directory</span>
      </div>

      <nav className="nav-links" aria-label="Main navigation">
        {navItems.map((item) => (
          <NavLink key={item.to} className={navLinkClassName} to={item.to}>
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="auth-actions">
        {auth.isAuthenticated ? (
          <button className="secondary-button" onClick={() => void auth.logout()} type="button">
            Logout
          </button>
        ) : (
          loginAction
        )}
      </div>
    </header>
  );
}
