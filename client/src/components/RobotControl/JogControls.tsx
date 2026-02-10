import { useState } from 'react';
import { useRobotControl } from '../../hooks/useRobotControl';
import './JogControls.css';

export function JogControls() {
  const { jog, isExecuting } = useRobotControl();
  const [stepSize, setStepSize] = useState(10);

  const handleJog = async (deltaX: number, deltaY: number, deltaZ: number) => {
    await jog({ deltaX, deltaY, deltaZ });
  };

  return (
    <div className="card">
      <h2 className="card-title">Jog Controls</h2>

      <div className="jog-step-selector">
        <span className="step-label">Step Size:</span>
        <div className="step-buttons">
          {[1, 10, 100].map((size) => (
            <button
              key={size}
              className={`btn btn-secondary ${stepSize === size ? 'active' : ''}`}
              onClick={() => setStepSize(size)}
            >
              {size} mm
            </button>
          ))}
        </div>
      </div>

      <div className="jog-controls-container">
        {/* XY Plane Controls */}
        <div className="jog-section">
          <h3 className="jog-section-title">XY Plane</h3>
          <div className="jog-grid-xy">
            <div></div>
            <button
              className="btn btn-primary jog-btn"
              onClick={() => handleJog(0, stepSize, 0)}
              disabled={isExecuting}
            >
              +Y
            </button>
            <div></div>
            <button
              className="btn btn-primary jog-btn"
              onClick={() => handleJog(-stepSize, 0, 0)}
              disabled={isExecuting}
            >
              -X
            </button>
            <div className="jog-center">⊕</div>
            <button
              className="btn btn-primary jog-btn"
              onClick={() => handleJog(stepSize, 0, 0)}
              disabled={isExecuting}
            >
              +X
            </button>
            <div></div>
            <button
              className="btn btn-primary jog-btn"
              onClick={() => handleJog(0, -stepSize, 0)}
              disabled={isExecuting}
            >
              -Y
            </button>
            <div></div>
          </div>
        </div>

        {/* Z Axis Controls */}
        <div className="jog-section">
          <h3 className="jog-section-title">Z Axis</h3>
          <div className="jog-z-controls">
            <button
              className="btn btn-primary jog-btn-tall"
              onClick={() => handleJog(0, 0, stepSize)}
              disabled={isExecuting}
            >
              +Z ↑
            </button>
            <button
              className="btn btn-primary jog-btn-tall"
              onClick={() => handleJog(0, 0, -stepSize)}
              disabled={isExecuting}
            >
              -Z ↓
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
