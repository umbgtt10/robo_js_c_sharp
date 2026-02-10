import { useEffect, useState } from 'react';
import { apiClient } from '../services/apiClient';
import type { WorkEnvelope } from '../types';
import './SettingsPage.css';

export function SettingsPage() {
  const [workEnvelope, setWorkEnvelope] = useState<WorkEnvelope | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        const envelope = await apiClient.getWorkEnvelope();
        setWorkEnvelope(envelope);
      } catch (error) {
        console.error('Failed to load settings:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchSettings();
  }, []);

  if (isLoading) {
    return (
      <div className="page">
        <h1 className="page-title">Settings</h1>
        <div className="card">
          <p className="placeholder-text">Loading settings...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <h1 className="page-title">System Settings</h1>

      <div className="settings-grid">
        <div className="card">
          <h2 className="card-title">Work Envelope</h2>
          {workEnvelope && (
            <div className="settings-section">
              <div className="settings-row">
                <span className="settings-label">X Range:</span>
                <span className="settings-value">
                  {workEnvelope.xMin} mm to {workEnvelope.xMax} mm
                </span>
              </div>
              <div className="settings-row">
                <span className="settings-label">Y Range:</span>
                <span className="settings-value">
                  {workEnvelope.yMin} mm to {workEnvelope.yMax} mm
                </span>
              </div>
              <div className="settings-row">
                <span className="settings-label">Z Range:</span>
                <span className="settings-value">
                  {workEnvelope.zMin} mm to {workEnvelope.zMax} mm
                </span>
              </div>
            </div>
          )}
        </div>

        <div className="card">
          <h2 className="card-title">Connection Settings</h2>
          <p className="placeholder-text">
            ⚙️ Connection configuration will be available here.
          </p>
          <p className="placeholder-description">
            Features to be implemented:
          </p>
          <ul className="feature-list">
            <li>Hardware IP address and port configuration</li>
            <li>Connection timeout settings</li>
            <li>Auto-reconnect preferences</li>
            <li>Communication protocol options</li>
          </ul>
        </div>

        <div className="card">
          <h2 className="card-title">UI Preferences</h2>
          <p className="placeholder-text">
            🎨 User interface customization options.
          </p>
          <p className="placeholder-description">
            Features to be implemented:
          </p>
          <ul className="feature-list">
            <li>Dark mode toggle</li>
            <li>Unit system (metric/imperial)</li>
            <li>Position update refresh rate</li>
            <li>Number of decimal places</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
