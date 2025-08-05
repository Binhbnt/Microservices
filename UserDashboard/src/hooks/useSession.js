import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;

export function useSession() {
  const [currentUser, setCurrentUser] = useState(null);
  const [lastActiveUser, setLastActiveUser] = useState(null);
  const [isRenewalModalOpen, setIsRenewalModalOpen] = useState(false);
  const [isReLoginModalOpen, setIsReLoginModalOpen] = useState(false);
  const [minutesRemaining, setMinutesRemaining] = useState(0);
  const [toastMessage, setToastMessage] = useState(null);

  const sessionTimers = useRef({ warning: null, logout: null });
  const expiryTimeRef = useRef(null);
  const navigate = useNavigate();

  const getAuthHeaders = useCallback(() => {
    const token = localStorage.getItem('jwtToken');
    return token
      ? { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }
      : { 'Content-Type': 'application/json' };
  }, []);

  const clearSessionTimers = useCallback(() => {
    clearTimeout(sessionTimers.current.warning);
    clearTimeout(sessionTimers.current.logout);
  }, []);

  const handleLogout = useCallback((message) => {
    const finalMessage = typeof message === 'string' ? message : 'Bạn đã đăng xuất.';
    clearSessionTimers();
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('lastActiveUser');
    setCurrentUser(null);
    setLastActiveUser(null);
    setIsRenewalModalOpen(false);
    setIsReLoginModalOpen(false);
    expiryTimeRef.current = null;
    setToastMessage({ type: 'success', message: finalMessage });
    navigate('/login');
  }, [navigate, clearSessionTimers]);

  const handleSessionExpired = useCallback(() => {
    clearSessionTimers();
    setIsRenewalModalOpen(false);
    const storedUser = localStorage.getItem('lastActiveUser');
    if (storedUser) {
      setLastActiveUser(JSON.parse(storedUser));
      setIsReLoginModalOpen(true);
    } else {
      handleLogout('Phiên làm việc đã hết hạn.');
    }
  }, [clearSessionTimers, handleLogout]);

  const handleLoginSuccess = useCallback((token, userDetails) => {
    const userToStore = {
      id: userDetails.id,
      maSoNhanVien: userDetails.maSoNhanVien || userDetails.MaSoNhanVien,
      hoTen: userDetails.hoTen || userDetails.HoTen,
      email: userDetails.email,
      role: userDetails.role,
      chucVu: userDetails.chucVu,
      boPhan: userDetails.boPhan,
      avatarUrl: userDetails.avatarUrl,
    };
    localStorage.setItem('jwtToken', token);
    localStorage.setItem('lastActiveUser', JSON.stringify(userToStore));
    setCurrentUser({ ...userDetails, token });
    setLastActiveUser(userToStore);
    setToastMessage({ type: 'success', message: `Chào mừng, ${userToStore.hoTen}!` });
    navigate('/');
  }, [navigate]);

  const setupSessionTimers = useCallback((token) => {
    clearSessionTimers();
    const parseJwt = (t) => { try { return JSON.parse(atob(t.split('.')[1])); } catch (e) { return null; } };
    const decodedToken = parseJwt(token);
    if (!decodedToken || !decodedToken.exp) {
      handleLogout('Token không hợp lệ.');
      return;
    }

    const expiryTime = decodedToken.exp * 1000;
    expiryTimeRef.current = expiryTime;
    const warningTime = 5 * 60 * 1000;
    const timeUntilExpiry = expiryTime - Date.now();

    if (timeUntilExpiry <= 0) { handleSessionExpired(); return; }

    sessionTimers.current.logout = setTimeout(handleSessionExpired, timeUntilExpiry);

    if (timeUntilExpiry > warningTime) {
      sessionTimers.current.warning = setTimeout(() => {
        setIsRenewalModalOpen(true);
      }, timeUntilExpiry - warningTime);
    } else {
      setIsRenewalModalOpen(true);
    }
  }, [clearSessionTimers, handleSessionExpired, handleLogout]);

  const handleRenewSession = async () => {
    setIsRenewalModalOpen(false);
    try {
      const response = await fetch(`${API_GATEWAY_URL}/api/Auth/renew-token`, {
        method: 'POST',
        headers: getAuthHeaders()
      });
      if (!response.ok) throw new Error('Renew failed');
      const data = await response.json();

      localStorage.setItem('jwtToken', data.token);
      setCurrentUser(prev => ({ ...prev, token: data.token }));
      setToastMessage({ type: 'success', message: 'Gia hạn thành công!' });
      setupSessionTimers(data.token);
    } catch (error) {
      handleLogout('Không thể gia hạn phiên. Vui lòng đăng nhập lại.');
    }
  };

  const handleDeclineRenewal = () => {
    setIsRenewalModalOpen(false);
    handleLogout('Bạn đã chọn không gia hạn phiên làm việc.');
  };

  const handleReLoginAttempt = async (password) => {
    const storedUser = JSON.parse(localStorage.getItem('lastActiveUser'));
    if (!storedUser) return false;
    try {
      const response = await fetch(`${API_GATEWAY_URL}/api/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: storedUser.maSoNhanVien, password })
      });
      if (!response.ok) return false;
      const loginResponse = await response.json();
      handleLoginSuccess(loginResponse.token, loginResponse);
      setIsReLoginModalOpen(false);
      return true;
    } catch (error) {
      return false;
    }
  };

  useEffect(() => {
    const storedToken = localStorage.getItem('jwtToken');
    if (storedToken) {
      const storedUser = localStorage.getItem('lastActiveUser');
      try {
        const user = JSON.parse(storedUser);
        setCurrentUser({ ...user, token: storedToken });
        setLastActiveUser(user);
      } catch (e) { localStorage.clear(); }
    }
  }, []);

  useEffect(() => {
    if (currentUser?.token) {
      setupSessionTimers(currentUser.token);
    }
    return () => clearSessionTimers();
  }, [currentUser?.token, setupSessionTimers, clearSessionTimers]);

  useEffect(() => {
    if (isRenewalModalOpen) {
      const timerId = setInterval(() => {
        if (expiryTimeRef.current) {
          const now = Date.now();
          const timeLeft = expiryTimeRef.current - now;
          setMinutesRemaining(timeLeft / (1000 * 60));
        }
      }, 1000);
      return () => clearInterval(timerId);
    }
  }, [isRenewalModalOpen]);

  return {
    currentUser, lastActiveUser, isRenewalModalOpen, isReLoginModalOpen,
    minutesRemaining, toastMessage, setToastMessage,
    API_GATEWAY_URL, // Nếu các component cần dùng đến
    handleLoginSuccess, handleLogout, handleSessionExpired, handleRenewSession,
    handleDeclineRenewal, handleReLoginAttempt, getAuthHeaders, setCurrentUser
  };
}
