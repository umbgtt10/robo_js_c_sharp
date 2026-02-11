import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { RobotProvider } from './contexts/RobotContext';
import { MainLayout } from './components/Layout/MainLayout';
import { DashboardPage } from './pages/DashboardPage';
import { HistoryPage } from './pages/HistoryPage';
import { SettingsPage } from './pages/SettingsPage';
import { LoginPage } from './pages/LoginPage';
import { apiClient } from './services/apiClient';
import './styles/global.css';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  if (!apiClient.isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route
          path="/"
          element={
            <ProtectedRoute>
              <RobotProvider>
                <MainLayout />
              </RobotProvider>
            </ProtectedRoute>
          }
        >
          <Route index element={<DashboardPage />} />
          <Route path="history" element={<HistoryPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
