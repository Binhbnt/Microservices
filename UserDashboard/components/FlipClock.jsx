import React, { useState, useEffect, useRef } from 'react';
import '/src/FlipClock.css';

// Component cho một cặp số (giờ, phút, hoặc giây)
function FlipUnit({ digit, unit }) {
  const [currentDigit, setCurrentDigit] = useState('00');
  const [previousDigit, setPreviousDigit] = useState('00');
  const [shuffle, setShuffle] = useState(false);

  useEffect(() => {
    const formattedDigit = digit < 10 ? `0${digit}` : digit.toString();
    if (formattedDigit !== currentDigit) {
      setPreviousDigit(currentDigit);
      setCurrentDigit(formattedDigit);
      setShuffle(prev => !prev);
    }
  }, [digit, currentDigit]);

  const digit1 = shuffle ? previousDigit : currentDigit;
  const digit2 = !shuffle ? previousDigit : currentDigit;
  
  const animation1 = shuffle ? 'fold' : 'unfold';
  const animation2 = !shuffle ? 'fold' : 'unfold';

  return (
    <div className="flipUnitContainer">
      <div className="upperCard"><span>{currentDigit}</span></div>
      <div className="lowerCard"><span>{previousDigit}</span></div>
      <div className={`flipCard fold ${animation1} play`}><span>{digit1}</span></div>
      <div className={`flipCard unfold ${animation2} play`}><span>{digit2}</span></div>
    </div>
  );
}

// Component chính của đồng hồ
function FlipClock() {
  const [time, setTime] = useState({
    hours: 0,
    minutes: 0,
    seconds: 0,
  });

  useEffect(() => {
    const timerID = setInterval(() => {
      const now = new Date();
      setTime({
        hours: now.getHours(),
        minutes: now.getMinutes(),
        seconds: now.getSeconds(),
      });
    }, 1000);
    return () => clearInterval(timerID);
  }, []);

  return (
    <div className="flipClock">
      <FlipUnit unit="hours" digit={time.hours} />
      <FlipUnit unit="minutes" digit={time.minutes} />
      <FlipUnit unit="seconds" digit={time.seconds} />
    </div>
  );
}

export default FlipClock;