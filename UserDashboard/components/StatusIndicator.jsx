import React from 'react';
import { translateStatus } from '/src/statusHelper';
const StatusIndicator = ({ status }) => {
  const isHealthy = status === "Healthy";
  
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', width: '120px' }}>
      <div style={{
        width: '8px',
        height: '8px',
        borderRadius: '50%',
        backgroundColor: isHealthy ? '#22c55e' : '#ef4444',
      }}></div>
      <span style={{
        color: isHealthy ? '#4ade80' : '#f87171',
        fontWeight: 500
      }}>
        {translateStatus(status)}
      </span>
    </div>
  );
};

export default StatusIndicator;