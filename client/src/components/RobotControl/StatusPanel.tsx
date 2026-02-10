import { useRobot } from '../../contexts/RobotContext';
import './StatusPanel.css';

export function StatusPanel() {
  const { status } = useRobot();

  if (!status) {
    return (
      <div className="card">
        <h2 className="card-title">System Status</h2>
        <div className="status-loading">Loading status...</div>
      </div>
    );
  }

  const getStateClass = () => {
    switch (status.state) {
      case 'Idle':
        return 'state-idle';
      case 'Moving':
      case 'Homing':
        return 'state-moving';
      case 'EmergencyStopped':
      case 'Error':
        return 'state-error';
      default:
        return '';
    }
  };

  return (
    <div className="card">
      <h2 className="card-title">System Status</h2>

      <div className="status-grid">
        <div className="status-item">
          <span className="status-label">Robot State</span>
          <span className={`status-value state-badge ${getStateClass()}`}>
            {status.state}
          </span>
        </div>

        <div className="status-item">
          <span className="status-label">Temperature</span>
          <span className="status-value">
            {status.temperature.toFixed(1)}°C
          </span>
        </div>

        <div className="status-item">
          <span className="status-label">Load</span>
          <span className="status-value">
            {status.loadPercentage.toFixed(0)}%
          </span>
        </div>

        <div className="status-item">
          <span className="status-label">Connection</span>
          <span className="status-value">
            {status.isConnected ? '✓ Connected' : '✗ Disconnected'}
          </span>
        </div>

        {status.errorCode !== 0 && (
          <div className="status-item status-error">
            <span className="status-label">Error</span>
            <span className="status-value">
              Code {status.errorCode}: {status.errorMessage || 'Unknown error'}
            </span>
          </div>
        )}
      </div>
    </div>
  );
}
