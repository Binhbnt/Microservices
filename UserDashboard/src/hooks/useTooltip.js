// src/hooks/useTooltip.js

import { useState, useRef, useEffect } from 'react';

export function useTooltip() {
  const [tooltip, setTooltip] = useState({
    visible: false,
    content: null, // <-- Đổi 'text' thành 'content'
    position: { x: 0, y: 0 },
  });

  const timeoutRef = useRef(null);

  // Sửa lại hàm showTooltip để nhận 'content'
  const showTooltip = (content, e) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    const rect = e.currentTarget.getBoundingClientRect();
    const tooltipOffset = 45; 
    const posX = rect.left + rect.width / 2;
    let posY;

    // Tự động điều chỉnh vị trí trên/dưới
    if (rect.top < tooltipOffset) {
      posY = rect.bottom + 10;
    } else {
      posY = rect.top - tooltipOffset;
    }
    
    setTooltip({
      visible: true,
      content, // <-- Gán content
      position: { x: posX, y: posY },
    });

    // Auto-hide, có thể bỏ đi nếu không muốn
    timeoutRef.current = setTimeout(() => {
      setTooltip(prev => ({ ...prev, visible: false }));
    }, 3000);
  };

  const hideTooltip = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    setTooltip(prev => ({ ...prev, visible: false }));
  };

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  return { tooltip, showTooltip, hideTooltip };
}