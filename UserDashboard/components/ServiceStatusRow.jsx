import React, { useEffect, useRef, useState } from 'react';
import StatusIndicator from './StatusIndicator';
import StatusBar from './StatusBar';

// ⚠️ Đồng bộ với StatusBar.jsx
const BAR_W = 6;   // px
const GAP   = 6;   // px
const TOTAL_BARS = 60;

const serviceInfo = {
  ApiGateway: ["API Gateway", "Port 5081"],
  UserService: ["User", "Port 5080"],
  LeaveRequestService: ["LeaveRequests", "Port 5083"],
  NotificationService: ["Notification", "Port 5084"],
  AuditLogService: ["AuditLog", "Port 5082"],
  SubscriptionService: ["Subscription", "Port 5085"],
};

const timeAgoText = (date) => {
  if (!date) return '';
  const now = new Date();
  const diffMs = now - new Date(date);
  const diffMins = Math.floor(diffMs / 60000);
  if (diffMins < 1) return "Vừa xong";
  if (diffMins < 60) return `${diffMins} phút`;
  const diffHrs = Math.floor(diffMins / 60);
  return `${diffHrs} giờ`;
};

const ServiceStatusRow = ({ serviceName, logs }) => {
  const labelLines = serviceInfo[serviceName] || [serviceName];

  // 1) Uptime
  const healthyCount = logs.filter(l => l.status === 'Healthy').length;
  const uptimePercentage = logs.length > 0 ? ((healthyCount / logs.length) * 100).toFixed(0) : 100;

  // 2) Log mới nhất / cũ nhất
  const latestLog = logs.length ? logs[logs.length - 1] : null;
  const oldestLog = logs.length ? logs[0] : null;

  // 3) Chuẩn hóa 60 thanh: **MỚI NHẤT BÊN TRÁI**
  // - Lấy tối đa 60 log gần nhất (thứ tự: cũ -> mới)
  const recentLogs = logs.slice(Math.max(0, logs.length - TOTAL_BARS));
  // - Đảo để MỚI -> CŨ (trái sang phải)
  const recentReversed = [...recentLogs].reverse();
  // - Đệm ô trống ở CUỐI để đủ 60
  const padCount = TOTAL_BARS - recentReversed.length;
  const bars = [
    ...recentReversed.map(l => ({ status: l.status, checkedAt: l.checkedAt })),
    ...Array.from({ length: padCount }, () => ({ status: 'empty', checkedAt: null })),
  ];

  // 4) Tính số vạch tối đa theo bề rộng
  const trackRef = useRef(null);
  const [maxBars, setMaxBars] = useState(TOTAL_BARS);

  useEffect(() => {
    const el = trackRef.current;
    if (!el) return;

    const measure = () => {
      const w = el.clientWidth;
      const n = Math.max(1, Math.floor((w + GAP) / (BAR_W + GAP)));
      setMaxBars(Math.min(n, TOTAL_BARS));
    };

    measure();
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    window.addEventListener('resize', measure);
    window.addEventListener('orientationchange', measure);
    return () => {
      ro.disconnect();
      window.removeEventListener('resize', measure);
      window.removeEventListener('orientationchange', measure);
    };
  }, []);

  // 5) Hiển thị vừa khít (lấy từ BÊN TRÁI vì trái là "mới nhất")
  const barsToShow = bars.slice(0, maxBars);

  // Thời gian hiển thị: trái = mới nhất nhìn thấy, phải = cũ nhất nhìn thấy
  const realShownCount = Math.min(maxBars, recentReversed.length);
  const latestVisibleLog = latestLog || null;
  const oldestVisibleLog =
    realShownCount > 0
      ? recentReversed[realShownCount - 1]   // phần tử cuối trong đoạn đang thấy (cũ hơn)
      : oldestLog;

  return (
    <div
      style={{
        backgroundColor: '#1f2937',
        padding: '1rem',
        borderRadius: '0.5rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '1rem'
      }}
    >
      {/* Cụm thông tin service và % uptime */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexBasis: '250px', flexShrink: 0 }}>
        <span
          style={{
            padding: '0.25rem 0.75rem',
            fontSize: '0.875rem',
            fontWeight: 600,
            borderRadius: '9999px',
            backgroundColor: uptimePercentage > 95 ? 'rgba(34, 197, 94, 0.2)' : 'rgba(239, 68, 68, 0.2)',
            color: uptimePercentage > 95 ? '#4ade80' : '#f87171',
          }}
        >
          {uptimePercentage}%
        </span>
        <div>
          <span style={{ color: '#e5e7eb', fontWeight: 500 }}>{labelLines[0]}</span>
          {labelLines[1] && <span style={{ color: '#9ca3af', fontSize: '0.875rem', display: 'block' }}>{labelLines[1]}</span>}
        </div>
      </div>

      {/* Trạng thái hiện tại */}
      {latestLog && <StatusIndicator status={latestLog.status} />}

      {/* Track + thời gian */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        {/* Dãy vạch (mới bên trái) */}
        <div
          ref={trackRef}
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-start',
            gap: `${GAP}px`,
            overflow: 'hidden',
            minWidth: 0,
            boxSizing: 'border-box'
          }}
        >
          {barsToShow.map((bar, i) => (
            <StatusBar key={i} status={bar.status} checkedAt={bar.checkedAt} />
          ))}
        </div>

        {/* Thời gian: trái = Vừa xong, phải = cũ hơn */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            marginTop: '4px',
            fontSize: '0.75rem',
            color: '#6b7280'
          }}
        >
          <span>{timeAgoText(latestVisibleLog?.checkedAt)}</span>
          <span>{timeAgoText(oldestVisibleLog?.checkedAt)}</span>
        </div>
      </div>
    </div>
  );
};

export default ServiceStatusRow;
