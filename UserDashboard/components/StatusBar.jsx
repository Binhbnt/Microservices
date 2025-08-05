import React from 'react';
import { translateStatus } from '/src/statusHelper';
// Hàm helper để định dạng ngày giờ theo kiểu dd/MM/yyyy HH:mm:ss
const formatDateTime = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng trong JS bắt đầu từ 0
  const year = date.getFullYear();

  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');

  return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
};

const StatusBar = ({ status, checkedAt }) => {
  const getColor = () => {
    if (status === 'Healthy') return '#22c55e'; // Xanh lá
    if (status === 'Unreachable' || status === 'Error') return '#ef4444'; // Đỏ
    return '#374151'; // Xám cho các thanh trống
  };

  // Sử dụng hàm formatDateTime mới
  const tooltipText = checkedAt
    ? `Trạng thái: ${translateStatus(status)}\nThời gian: ${formatDateTime(checkedAt)}`
    : 'Chưa có dữ liệu';

  return (
    <div
      title={tooltipText}
      style={{
        width: '0.375rem',
        height: '2rem',
        borderRadius: '0.125rem',
        backgroundColor: getColor(),
        flexShrink: 0,
        cursor: 'pointer',
      }}
    />
  );
};

export default StatusBar;