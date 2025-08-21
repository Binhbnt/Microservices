import React, { useState, useEffect } from 'react';
import '/src/AnalogClock.css'; // Import file CSS

function AnalogClock() {
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timerId = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(timerId);
  }, []);

  const hourDeg = time.getHours() * 30 + time.getMinutes() * 0.5;
  const minuteDeg = time.getMinutes() * 6;
  const secondDeg = time.getSeconds() * 6;

  return (
    // Thêm một div bọc ngoài để dễ dàng scale
    <div className="analog-clock-wrapper">
      <div className="clock">
        <div className="hour_hand" style={{ transform: `rotateZ(${hourDeg}deg)` }} />
        <div className="min_hand" style={{ transform: `rotateZ(${minuteDeg}deg)` }} />
        <div className="sec_hand" style={{ transform: `rotateZ(${secondDeg}deg)` }} />
        
        <span className="twelve">12</span>
        <span className="one">1</span>
        <span className="two">2</span>
        <span className="three">3</span>
        <span className="four">4</span>
        <span className="five">5</span>
        <span className="six">6</span>
        <span className="seven">7</span>
        <span className="eight">8</span>
        <span className="nine">9</span>
        <span className="ten">10</span>
        <span className="eleven">11</span>
      </div>
    </div>
  );
}

export default AnalogClock;