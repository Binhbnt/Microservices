// components/Tooltip.jsx

import React from 'react';
import ReactDOM from 'react-dom';
import '/src/Tooltip.css'; // Đảm bảo bạn có file CSS này

function Tooltip({ content, position, visible }) { // <-- Đổi prop 'text' thành 'content'
  if (!visible) return null;

  const style = {
    position: 'fixed',
    top: `${position.y}px`,
    left: `${position.x}px`,
    transform: 'translateX(-50%)',
    background: 'rgba(0, 0, 0, 0.85)',
    color: '#fff',
    padding: '8px 12px',
    borderRadius: '6px',
    fontSize: '14px',
    whiteSpace: 'nowrap',
    zIndex: 9999,
    pointerEvents: 'none',
    boxShadow: '0px 2px 8px rgba(0,0,0,0.2)',
  };

  return ReactDOM.createPortal(
    <div style={style}>{content}</div>, // <-- Render 'content'
    document.body
  );
}

export default Tooltip;