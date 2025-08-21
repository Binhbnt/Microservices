// components/status/StatusFooter.jsx

import React, { useState, useEffect } from 'react';

const REFRESH_INTERVAL = 15; // Thời gian đếm ngược (giây), khớp với BE

const StatusFooter = ({ lastUpdated }) => {
  const [countdown, setCountdown] = useState(REFRESH_INTERVAL);

  useEffect(() => {
    // Reset bộ đếm mỗi khi có dữ liệu mới (lastUpdated thay đổi)
    setCountdown(REFRESH_INTERVAL);
  }, [lastUpdated]);

  useEffect(() => {
    // Chạy bộ đếm thời gian mỗi giây
    const timer = setInterval(() => {
      setCountdown(prev => (prev > 0 ? prev - 1 : REFRESH_INTERVAL));
    }, 1000);

    // Dọn dẹp timer khi component bị hủy
    return () => clearInterval(timer);
  }, []);

  const formatTime = (date) => {
    if (!date) return '';
    return new Date(date).toLocaleString('vi-VN');
  };

  const formatCountdown = (seconds) => {
    const mins = Math.floor(seconds / 60).toString().padStart(2, '0');
    const secs = (seconds % 60).toString().padStart(2, '0');
    return `${mins}:${secs}`;
  };

  return (
    <div style={{ 
        marginTop: '2rem', 
        textAlign: 'center', 
        fontSize: '0.875rem', 
        color: '#6b7280' 
    }}>
      <p>Powered by BinhBNT</p>
      <p>Last Updated: {formatTime(lastUpdated)}</p>
      <p>Refresh in: {formatCountdown(countdown)}</p>
    </div>
  );
};

export default StatusFooter;