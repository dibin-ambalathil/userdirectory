import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { UserListPage } from '../UserListPage';
import { getUsers } from '../../api/usersApi';

jest.mock('../../api/usersApi', () => ({
  getUsers: jest.fn(),
  getApiErrorMessage: jest.fn(() => 'Unable to fetch users.')
}));

const mockedGetUsers = getUsers as jest.MockedFunction<typeof getUsers>;

describe('UserListPage', () => {
  it('renders users in table after loading', async () => {
    mockedGetUsers.mockResolvedValueOnce([
      {
        id: 'f6c3bb98-a95e-4bb5-989f-0d4f8f7780f0',
        name: 'Priya Sharma',
        age: 34,
        city: 'Pune',
        state: 'Maharashtra',
        pincode: '411001',
        createdAt: '2024-01-01T00:00:00Z'
      }
    ]);

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
});
