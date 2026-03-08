import { useCallback, useEffect, useMemo, useState } from 'react';
import { AxiosError } from 'axios';
import { useLocation, useNavigate } from 'react-router-dom';
import { getApiErrorMessage, getUsers, deleteUser } from '../api/usersApi';
import { useAuthState } from '../auth/useAuthState';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { Toast } from '../components/Toast';
import { User } from '../types/user';

interface LocationToastState {
  toastMessage?: string;
}

export function UserListPage(): JSX.Element {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);

  const location = useLocation();
  const navigate = useNavigate();
  const auth = useAuthState();

  useEffect(() => {
    const state = (location.state as LocationToastState | null) ?? null;
    if (!state?.toastMessage) {
      return;
    }

    setToastMessage(state.toastMessage);
    navigate(location.pathname, { replace: true, state: {} });
  }, [location, navigate]);

  useEffect(() => {
    let isMounted = true;

    async function loadUsers(): Promise<void> {
      try {
        setIsLoading(true);
        setError(null);

        const fetchedUsers = await getUsers();
        if (isMounted) {
          setUsers(fetchedUsers);
        }
      } catch (requestError) {
        if (isMounted) {
          if (requestError instanceof AxiosError && requestError.response?.status === 401) {
            auth.logout();
            navigate('/login', { replace: true, state: { from: '/users' } });
            return;
          }

          setError(getApiErrorMessage(requestError));
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadUsers();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  useEffect(() => {
    if (!toastMessage) {
      return;
    }

    const timer = setTimeout(() => setToastMessage(null), 3000);
    return () => clearTimeout(timer);
  }, [toastMessage]);

  const handleDelete = useCallback(
    async (userId: string): Promise<void> => {
      if (!window.confirm('Are you sure you want to delete this user?')) {
        return;
      }

      try {
        setDeletingUserId(userId);
        setError(null);

        await deleteUser(userId);
        setUsers((current) => current.filter((user) => user.id !== userId));
        setToastMessage('User deleted successfully.');
      } catch (requestError) {
        if (requestError instanceof AxiosError && requestError.response?.status === 401) {
          auth.logout();
          navigate('/login', { replace: true, state: { from: '/users' } });
          return;
        }
        setError(getApiErrorMessage(requestError));
      } finally {
        setDeletingUserId(null);
      }
    },
    [auth, navigate]
  );

  const content = useMemo(() => {
    if (isLoading) {
      return <LoadingSpinner label="Loading users..." />;
    }

    if (error) {
      return <p className="status-message error">Failed to load users: {error}</p>;
    }

    if (users.length === 0) {
      return <p className="status-message">No users found. Add your first user.</p>;
    }

    return (
      <div className="table-wrapper">
        <table className="user-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Age</th>
              <th>City</th>
              <th>State</th>
              <th>Pincode</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.name}</td>
                <td>{user.age}</td>
                <td>{user.city}</td>
                <td>{user.state}</td>
                <td>{user.pincode}</td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="edit-button"
                      onClick={() => navigate(`/users/${user.id}/edit`)}
                      disabled={deletingUserId !== null}
                    >
                      Edit
                    </button>
                    <button
                      className="delete-button"
                      onClick={() => void handleDelete(user.id)}
                      disabled={deletingUserId !== null}
                    >
                      {deletingUserId === user.id ? 'Deleting...' : 'Delete'}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }, [isLoading, error, users, deletingUserId, navigate, handleDelete]);

  return (
    <section className="page-card">
      <header className="page-header">
        <h1>User List</h1>
        <p>Browse users available in the directory.</p>
      </header>

      {toastMessage ? <Toast message={toastMessage} onClose={() => setToastMessage(null)} /> : null}
      {content}
    </section>
  );
}
