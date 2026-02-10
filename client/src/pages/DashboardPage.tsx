import { PositionDisplay } from '../components/RobotControl/PositionDisplay';
import { JogControls } from '../components/RobotControl/JogControls';
import { StatusPanel } from '../components/RobotControl/StatusPanel';
import { EmergencyStopButton } from '../components/RobotControl/EmergencyStopButton';
import { useRobotControl } from '../hooks/useRobotControl';
import './DashboardPage.css';

export function DashboardPage() {
  const { home, isExecuting } = useRobotControl();

  const handleHome = async () => {
    await home();
  };

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1 className="page-title">Robot Control Dashboard</h1>
        <button
          className="btn btn-primary"
          onClick={handleHome}
          disabled={isExecuting}
        >
          🏠 Home Robot
        </button>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-col-left">
          <PositionDisplay />
          <StatusPanel />
        </div>

        <div className="dashboard-col-right">
          <JogControls />
          <EmergencyStopButton />
        </div>
      </div>
    </div>
  );
}
