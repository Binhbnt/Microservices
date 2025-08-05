import React, { useState, useEffect } from 'react';
import { UAParser } from 'ua-parser-js';
import countries from 'i18n-iso-countries';
import viLocale from 'i18n-iso-countries/langs/vi.json';
import '/src/SessionInfo.css';

countries.registerLocale(viLocale);

function SessionInfo() {
  const [sessionData, setSessionData] = useState({
    ipAddress: 'Đang tải...',
    city: '',
    country: '',
    browser: 'Không xác định',
    os: '',
    deviceName: '',
    isMobile: false,
  });

  useEffect(() => {
    const parser = new UAParser();
    const result = parser.getResult();

    const getBrowserName = () => result.browser.name || 'Không xác định';
    const detectOS   = () => result.os.name || 'Không rõ';
    const getDeviceName = () => {
      const { device } = result;
      return (device.vendor && device.model)
        ? `${device.vendor} ${device.model}`
        : result.os.name || 'Unknown';
    };

    const fetchIpAddress = async () => {
      try {
        const res  = await fetch('https://ipwho.is/');
        const json = await res.json();
        const code  = json.country_code.toLowerCase();  
        const name  = json.country;
        if (!json.success) throw new Error('API lỗi');

        setSessionData({
          ipAddress: json.ip,
          city: json.city,
          country: name,
          flagCode: code,
          browser: getBrowserName(),
          os: detectOS(),
          deviceName: getDeviceName(),
          isMobile: result.device.type === 'mobile' || result.device.type === 'tablet',
        });
      } catch (_) {
        setSessionData({
          ipAddress: 'Không thể lấy',
          city: '',
          country: '',
          browser: getBrowserName(),
          os: detectOS(),
          deviceName: getDeviceName(),
          isMobile: result.device.type === 'mobile' || result.device.type === 'tablet',
        });
      }
    };

    fetchIpAddress();
  }, []);

  /* ---------- Icon helpers ---------- */
  const getBrowserIconClass = (browserName) => {
    const n = browserName?.toLowerCase() || '';
    if (n.includes('chrome')) return 'fa-brands fa-chrome';
    if (n.includes('firefox')) return 'fa-brands fa-firefox-browser';
    if (n.includes('edge')) return 'fa-brands fa-edge';
    if (n.includes('safari')) return 'fa-brands fa-safari';
    if (n.includes('brave')) return 'fa-brands fa-brave';
    if (n.includes('opera')) return 'fa-brands fa-opera';
    return 'fa-solid fa-globe';
  };

  const getOSIconClass = (osName) => {
    const n = osName?.toLowerCase() || '';
    if (n.includes('windows')) return 'fa-brands fa-windows';
    if (n.includes('mac')) return 'fa-brands fa-apple';
    if (n.includes('ios')) return 'fa-brands fa-apple';
    if (n.includes('android')) return 'fa-brands fa-android';
    if (n.includes('linux')) return 'fa-brands fa-linux';
    return 'fa-solid fa-display';
  };

  const getDeviceIconClass = (isMobile) =>
    isMobile ? 'fa-solid fa-mobile-screen-button' : 'fa-solid fa-desktop';

  /* ---------- Render ---------- */
  return (
    <div className="session-info">
      <div className="info-item">
        <i className={getBrowserIconClass(sessionData.browser)}></i>
        <span>Trình duyệt: {sessionData.browser}</span>
      </div>

      <div className="info-item">
        <i className="fa-solid fa-location-dot"></i>
        <span>IP: {sessionData.ipAddress}</span>
      </div>

      <div className="info-item">
        <i className="fa-solid fa-city"></i>
        <span>Thành Phố: {sessionData.city}</span>
      </div>

      <div className="info-item" style={{ display: 'flex', alignItems: 'center' }}>
           {sessionData.flagCode && (
                    <img
                        src={`https://flagcdn.com/16x12/${sessionData.flagCode}.png`}
                        alt="flag"
                        className="flag-icon"
                        style={{ marginRight: 17, borderRadius: 2, width: 16, height: 12 }}            
                     />
             )}           
            <span><strong>Quốc gia:</strong> {sessionData.country}</span>
      </div>

      <div className="info-item">
        <i className={getOSIconClass(sessionData.os)}></i>
        <span>Hệ Điều Hành: {sessionData.os}</span>
      </div>

      <div className="info-item">
        <i className={getDeviceIconClass(sessionData.isMobile)}></i>
        <span>Thiết Bị: {sessionData.deviceName}</span>
      </div>
    </div>
  );
}

export default SessionInfo;