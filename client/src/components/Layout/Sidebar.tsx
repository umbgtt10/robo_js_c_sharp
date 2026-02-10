import { Link, useLocation } from 'react-router-dom';
import './Sidebar.css';

export function Sidebar() {
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <aside className="sidebar">
      <nav className="sidebar-nav">
        <Link
          to="/"
          className={`sidebar-link ${isActive('/') ? 'active' : ''}`}
        >
          <span>🏠</span>
          <span>Dashboard</span>
        </Link>
        <Link
          to="/history"
          className={`sidebar-link ${isActive('/history') ? 'active' : ''}`}
        >
          <span>📊</span>
          <span>History</span>
        </Link>
        <Link
          to="/settings"
          className={`sidebar-link ${isActive('/settings') ? 'active' : ''}`}
        >
          <span>⚙️</span>
          <span>Settings</span>
        </Link>
      </nav>
    </aside>
  );
}
