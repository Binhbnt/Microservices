import React from 'react';
import StatusIndicator from './StatusIndicator';
import StatusBar from './StatusBar';

// Thông tin hiển thị cho từng service
const serviceInfo = {
  ApiGateway: ["API Gateway", "Port 5081"],
  UserService: ["User", "Port 5080"],
  LeaveRequestService: ["LeaveRequests", "Port 5083"],
  NotificationService: ["Notification", "Port 5084"],
  AuditLogService: ["AuditLog", "Port 5082"],
};

// Hàm tính thời gian
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
  const TOTAL_BARS = 60;
  const labelLines = serviceInfo[serviceName] || [serviceName];

  // 1. Tính toán tỷ lệ uptime
  const healthyCount = logs.filter(log => log.status === 'Healthy').length;
  const uptimePercentage = logs.length > 0 ? ((healthyCount / logs.length) * 100).toFixed(0) : 100;

  // 2. Lấy log cũ nhất và mới nhất
  const latestLog = logs.length > 0 ? logs[logs.length - 1] : null;
  const oldestLog = logs.length > 0 ? logs[0] : null;

  // 3. Tạo mảng 60 thanh trạng thái để render
  const bars = Array.from({ length: TOTAL_BARS }).map((_, index) => {
    const log = logs[index];
    return { status: log ? log.status : 'empty', checkedAt: log ? log.checkedAt : null };
  });

  return (
    <div style={{
      backgroundColor: '#1f2937', padding: '1rem', borderRadius: '0.5rem',
      display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '1rem'
    }}>
      {/* Cụm thông tin service và % uptime */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexBasis: '250px', flexShrink: 0 }}>
        <span style={{
          padding: '0.25rem 0.75rem', fontSize: '0.875rem', fontWeight: 600, borderRadius: '9999px',
          backgroundColor: uptimePercentage > 95 ? 'rgba(34, 197, 94, 0.2)' : 'rgba(239, 68, 68, 0.2)',
          color: uptimePercentage > 95 ? '#4ade80' : '#f87171',
        }}>
          {uptimePercentage}%
        </span>
        <div>
          <span style={{ color: '#e5e7eb', fontWeight: 500 }}>{labelLines[0]}</span>
          {labelLines[1] && <span style={{ color: '#9ca3af', fontSize: '0.875rem', display: 'block' }}>{labelLines[1]}</span>}
        </div>
      </div>

      {/* Trạng thái hiện tại */}
      {latestLog && <StatusIndicator status={latestLog.status} />}

      {/* --- CỤM THANH TRẠNG THÁI VÀ THỜI GIAN (ĐÃ SỬA) --- */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        {/* Hàng chứa các thanh trạng thái */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '4px' }}>
          {bars.map((bar, index) => (
            <StatusBar key={index} status={bar.status} checkedAt={bar.checkedAt} />
          ))}
        </div>
        {/* Hàng chứa thời gian bên dưới */}
        <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            marginTop: '4px', 
            fontSize: '0.75rem', 
            color: '#6b7280' 
        }}>
          <span>{timeAgoText(oldestLog?.checkedAt)}</span>
          <span>{timeAgoText(latestLog?.checkedAt)}</span>
        </div>
      </div>
    </div>
  );
};

export default ServiceStatusRow;