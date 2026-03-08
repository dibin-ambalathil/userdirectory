import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { UserListPage } from '../UserListPage';
import { getUsers, deleteUser } from '../../api/usersApi';

jest.mock('../../api/usersApi', () => ({
  getUsers: jest.fn(),
  deleteUser: jest.fn(),
  getApiErrorMessage: jest.fn(() => 'Unable to fetch users.')
}));

const stableAuthState = {
  isAuthenticated: true,
  isLoading: false,
  isAuthConfigured: false,
  login: async () => Promise.resolve(),
  loginWithPassword: async () => undefined,
  logout: jest.fn(),
  getAccessToken: async () => undefined
};

jest.mock('../../auth/useAuthState', () => ({
  useAuthState: () => stableAuthState
}));

const mockedGetUsers = getUsers as jest.MockedFunction<typeof getUsers>;
const mockedDeleteUser = deleteUser as jest.MockedFunction<typeof deleteUser>;

const sampleUser = {
  id: 'f6c3bb98-a95e-4bb5-989f-0d4f8f7780f0',
  name: 'Priya Sharma',
  age: 34,
  city: 'Pune',
  state: 'Maharashtra',
  pincode: '411001',
  createdAt: '2024-01-01T00:00:00Z'
};

describe('UserListPage', () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('renders users in table after loading', async () => {
    mockedGetUsers.mockResolvedValueOnce([sampleUser]);

    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <UserListPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Priya Sharma')).toBeInTheDocument();
      expect(screen.getByText('Pune')).toBeInTheDocument();
      expect(screen.getByText('411001')).toBeInTheDocument();
    });
  });

  it('renders Edit and Delete buttons for each user', async () => {
    mockedGetUsers.mockResolvedValueOnce([sampleUser]);

    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <UserListPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /delete/i })).toBeInTheDocument();
    });
  });

  it('removes user from list after confirming deletion', async () => {
    mockedGetUsers.mockResolvedValueOnce([sampleUser]);
    mockedDeleteUser.mockResolvedValueOnce();
    jest.spyOn(window, 'confirm').mockReturnValue(true);

    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <UserListPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('Priya Sharma')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /delete/i }));

    await waitFor(() => {
      expect(mockedDeleteUser).toHaveBeenCalledWith(sampleUser.id);
      expect(screen.queryByText('Priya Sharma')).not.toBeInTheDocument();
    });
  });
});
