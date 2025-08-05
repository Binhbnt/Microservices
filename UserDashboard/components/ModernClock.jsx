import React, { useState, useEffect } from 'react';
import '/src/ModernClock.css'; // File CSS riêng cho đồng hồ

function ModernClock() {
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timerId = setInterval(() => {
      setTime(new Date());
    }, 1000);

    return () => clearInterval(timerId);
  }, []);

  const day = time.toLocaleDateString('vi-VN', { weekday: 'long' });
  const date = time.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const timeString = time.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });

  return (
    <div className="modern-clock">
      <div className="time-display">{timeString}</div>
      <div className="date-display">{`${day}, ${date}`}</div>
    </div>
  );
}

export default ModernClock;