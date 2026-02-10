import './HistoryPage.css';

export function HistoryPage() {
  return (
    <div className="page">
      <h1 className="page-title">Movement History</h1>
      <div className="card">
        <p className="placeholder-text">
          📊 Historical data visualization will be displayed here.
        </p>
        <p className="placeholder-description">
          Features to be implemented:
        </p>
        <ul className="feature-list">
          <li>Movement history log with timestamps</li>
          <li>Position tracking over time (charts)</li>
          <li>Command execution statistics</li>
          <li>Error and event logs</li>
          <li>Export to CSV functionality</li>
        </ul>
      </div>
    </div>
  );
}
