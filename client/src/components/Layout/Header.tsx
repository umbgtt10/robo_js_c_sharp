import { useRobot } from '../../contexts/RobotContext';
import './Header.css';

export function Header() {
  const { status, connectionStatus } = useRobot();

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
        </div>
      </div>
    </header>
  );
}
