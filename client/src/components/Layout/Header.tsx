import { useNavigate } from 'react-router-dom';
import { useRobot } from '../../contexts/RobotContext';
import { apiClient } from '../../services/apiClient';
import './Header.css';

export function Header() {
  const { status, connectionStatus } = useRobot();
  const navigate = useNavigate();

  const handleLogout = () => {
    apiClient.logout();
    navigate('/login');
  };

  const getConnectionBadgeClass = () => {
    switch (connectionStatus) {
      case 'Connected':
        return 'status-badge connected';
      case 'Connecting':
      case 'Reconnecting':
        return 'status-badge connecting';
      default:
        return 'status-badge disconnected';
    }
  };

  const getStatusIndicator = () => {
    return status?.isConnected ? '●' : '○';
  };

  const currentUser = apiClient.getCurrentUser();

  return (
    <header className="header">
      <div className="header-content">
        <div className="header-left">
          <h1 className="header-title">Robotic Control System</h1>
        </div>
        <div className="header-right">
          <div className={getConnectionBadgeClass()}>
            <span>{getStatusIndicator()}</span>
            <span>{connectionStatus}</span>
          </div>
          {status && (
            <div className="status-info">
              <span className="status-label">State:</span>
              <span className="status-value">{status.state}</span>
            </div>
          )}
          {currentUser && (
            <div className="status-info">
              <span className="status-label">User:</span>
              <span className="status-value">{currentUser.username} ({currentUser.role})</span>
            </div>
          )}
          <button
            onClick={handleLogout}
            className="logout-button"
            title="Logout"
          >
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}
