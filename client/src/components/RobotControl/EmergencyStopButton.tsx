import { useState } from 'react';
import { useRobotControl } from '../../hooks/useRobotControl';
import './EmergencyStopButton.css';

export function EmergencyStopButton() {
  const { emergencyStop, resetError } = useRobotControl();
  const [isStopped, setIsStopped] = useState(false);

  const handleEmergencyStop = async () => {
    const success = await emergencyStop();
    if (success) {
      setIsStopped(true);
    }
  };

  const handleReset = async () => {
    const success = await resetError();
    if (success) {
      setIsStopped(false);
    }
  };

  return (
    <div className="card emergency-stop-card">
      <h2 className="card-title">Emergency Controls</h2>

      {!isStopped ? (
        <div className="emergency-content">
          <button
            className="btn-emergency-stop"
            onClick={handleEmergencyStop}
          >
            <span className="emergency-icon">⚠️</span>
            <span className="emergency-text">EMERGENCY STOP</span>
          </button>
          <p className="emergency-description">
            Immediately halts all robot operations
          </p>
        </div>
      ) : (
        <div className="emergency-content">
          <div className="emergency-status">
            <span className="emergency-icon">🛑</span>
            <span>Emergency Stop Active</span>
          </div>
          <button
            className="btn btn-secondary"
            onClick={handleReset}
          >
            Reset Error State
          </button>
          <p className="emergency-description">
           Reset the error state to resume normal operations
          </p>
        </div>
      )}
    </div>
  );
}
