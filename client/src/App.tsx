import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { RobotProvider } from './contexts/RobotContext';
import { MainLayout } from './components/Layout/MainLayout';
import { DashboardPage } from './pages/DashboardPage';
import { HistoryPage } from './pages/HistoryPage';
import { SettingsPage } from './pages/SettingsPage';
import './styles/global.css';

function App() {
  return (
    <BrowserRouter>
      <RobotProvider>
        <Routes>
          <Route path="/" element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="history" element={<HistoryPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
        </Routes>
      </RobotProvider>
    </BrowserRouter>
  );
}

export default App;
