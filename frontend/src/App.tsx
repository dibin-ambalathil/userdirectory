import { Navigate, Route, Routes } from 'react-router-dom';
import { Navbar } from './components/Navbar';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { UserListPage } from './pages/UserListPage';
import { AddUserPage } from './pages/AddUserPage';
import { EditUserPage } from './pages/EditUserPage';
import { LoginPage } from './pages/LoginPage';

export function App(): JSX.Element {
  return (
    <div className="app-shell">
      <Navbar />
      <main className="content-shell">
        <Routes>
          <Route path="/" element={<Navigate to="/users" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/users"
            element={
              <ProtectedRoute>
                <UserListPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/users/add"
            element={
              <ProtectedRoute>
                <AddUserPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/users/:id/edit"
            element={
              <ProtectedRoute>
                <EditUserPage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </main>
    </div>
  );
}
