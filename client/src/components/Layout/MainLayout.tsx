import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import './MainLayout.css';

export function MainLayout() {
  return (
    <div className="main-layout">
      <Header />
      <div className="content-wrapper">
        <Sidebar />
        <main className="main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
