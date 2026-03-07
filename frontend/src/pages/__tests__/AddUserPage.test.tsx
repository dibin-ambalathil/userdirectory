import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AddUserPage } from '../AddUserPage';
import { createUser } from '../../api/usersApi';

jest.mock('../../api/usersApi', () => ({
  createUser: jest.fn(),
  getApiErrorMessage: jest.fn(() => 'Request failed.')
}));

jest.mock('../../auth/useAuthState', () => ({
  useAuthState: () => ({
    isAuthenticated: true,
    isLoading: false,
    isAuthConfigured: false,
    login: async () => Promise.resolve(),
    loginWithPassword: async () => undefined,
    logout: () => undefined,
    getAccessToken: async () => undefined
  })
}));

const mockedCreateUser = createUser as jest.MockedFunction<typeof createUser>;

describe('AddUserPage', () => {
  it('shows inline validation errors when submitting empty form', () => {
    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <AddUserPage />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: /save user/i }));

    expect(screen.getByText('Name is required.')).toBeInTheDocument();
    expect(screen.getByText('Age is required.')).toBeInTheDocument();
    expect(screen.getByText('City is required.')).toBeInTheDocument();
    expect(screen.getByText('State is required.')).toBeInTheDocument();
    expect(screen.getByText('Pincode is required.')).toBeInTheDocument();
  });

  it('submits valid form payload', async () => {
    mockedCreateUser.mockResolvedValueOnce({
      id: '16eb1129-9925-4f53-a8e0-5dd36346f2cc',
      name: 'Aarav Menon',
      age: 29,
      city: 'Bengaluru',
      state: 'Karnataka',
      pincode: '560001',
      createdAt: '2024-01-01T00:00:00Z'
    });

    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <AddUserPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByPlaceholderText('Enter name'), { target: { value: 'Aarav Menon' } });
    fireEvent.change(screen.getByPlaceholderText('Enter age'), { target: { value: '29' } });
    fireEvent.change(screen.getByPlaceholderText('Enter city'), { target: { value: 'Bengaluru' } });
    fireEvent.change(screen.getByPlaceholderText('Enter state'), { target: { value: 'Karnataka' } });
    fireEvent.change(screen.getByPlaceholderText('Enter pincode'), { target: { value: '560001' } });

    fireEvent.click(screen.getByRole('button', { name: /save user/i }));

    await waitFor(() => {
      expect(mockedCreateUser).toHaveBeenCalledWith({
        name: 'Aarav Menon',
        age: 29,
        city: 'Bengaluru',
        state: 'Karnataka',
        pincode: '560001'
      });
    });
  });
});
