import React from 'react';
import { translateStatus } from '/src/statusHelper';

// Đồng bộ kích thước với ServiceStatusRow.jsx
const BAR_W = 6;   // px
const BAR_H = 32;  // px (≈ 2rem khi root 16px)

const formatDateTime = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
};

const StatusBar = ({ status, checkedAt }) => {
  const getColor = () => {
    if (status === 'Healthy') return '#22c55e';               // xanh lá
    if (status === 'Unreachable' || status === 'Error') return '#ef4444'; // đỏ
    return '#374151';                                        // xám
  };

  const tooltipText = checkedAt
    ? `Trạng thái: ${translateStatus(status)}\nThời gian: ${formatDateTime(checkedAt)}`
    : 'Chưa có dữ liệu';

  return (
    <div
      title={tooltipText}
      style={{
        width: `${BAR_W}px`,
        height: `${BAR_H}px`,
        borderRadius: '2px',
        backgroundColor: getColor(),
        flex: '0 0 auto',
        flexShrink: 0,
        cursor: 'pointer',
      }}
    />
  );
};

export default StatusBar;
