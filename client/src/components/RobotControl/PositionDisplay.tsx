import { useRobot } from '../../contexts/RobotContext';
import './PositionDisplay.css';

export function PositionDisplay() {
  const { position } = useRobot();

  if (!position) {
    return (
      <div className="card">
        <h2 className="card-title">Position</h2>
        <div className="position-loading">Loading position data...</div>
      </div>
    );
  }

  const formatValue = (value: number | undefined) => value?.toFixed(2) ?? 'N/A';

  return (
    <div className="card">
      <h2 className="card-title">Current Position</h2>
      <div className="position-grid">
        <div className="position-item">
          <span className="position-label">X</span>
          <span className="position-value">{formatValue(position.x)} mm</span>
        </div>
        <div className="position-item">
          <span className="position-label">Y</span>
          <span className="position-value">{formatValue(position.y)} mm</span>
        </div>
        <div className="position-item">
          <span className="position-label">Z</span>
          <span className="position-value">{formatValue(position.z)} mm</span>
        </div>
        <div className="position-item">
          <span className="position-label">Rot X</span>
          <span className="position-value">{formatValue(position.rotationX)}°</span>
        </div>
        <div className="position-item">
          <span className="position-label">Rot Y</span>
          <span className="position-value">{formatValue(position.rotationY)}°</span>
        </div>
        <div className="position-item">
          <span className="position-label">Rot Z</span>
          <span className="position-value">{formatValue(position.rotationZ)}°</span>
        </div>
      </div>
      <div className="position-timestamp">
        Last update: {new Date(position.timestamp).toLocaleTimeString()}
      </div>
    </div>
  );
}
