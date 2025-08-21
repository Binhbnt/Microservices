// src/components/LoginPage.jsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom'; // useNavigate ở đây là đúng
import axios from 'axios';
import '/src/LoginPage.css';
import { useTooltipContext } from '/src/TooltipContext';
function LoginPage({ API_GATEWAY_URL, onLoginSuccess, onToastMessage }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
   const { showTooltip, hideTooltip } = useTooltipContext();
  const handleLoginSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    try {
      const response = await axios.post(`${API_GATEWAY_URL}/api/Auth/login`, { username, password });
      const data = response.data;
      onLoginSuccess(data.token, data, navigate);
    } catch (err) {
      // ... (Phần xử lý lỗi giữ nguyên)
      let errorMessage = "Đã xảy ra lỗi không xác định.";
      if (axios.isAxiosError(err) && err.response) {
        errorMessage = err.response.data?.message || "Tên đăng nhập hoặc mật khẩu không đúng.";
      }
      setError(errorMessage);
    }
  };

  return (
    <div className="login-page-container">
      <div className="login-box">
        {/* THAY ĐỔI: Cập nhật tiêu đề */}
        <h2 className="login-title">Đăng Nhập Hệ Thống</h2>
        
        <form onSubmit={handleLoginSubmit}>
          <div className="form-group">
            {/* THAY ĐỔI: Cập nhật label */}
            <label htmlFor="username">Mã số nhân viên</label>
            <input
              type="text"
              id="username"              
              onMouseEnter={(e) => showTooltip("Nhập Email hoặc Mã nhân viên", e)}
              onMouseLeave={hideTooltip}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              // THAY ĐỔI: Thêm placeholder
              placeholder="Nhập mã NV..."
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Mật khẩu</label>
            <input
              type="password"
              id="password"
              onMouseEnter={(e) => showTooltip("Nhập Mật Khẩu", e)}
              onMouseLeave={hideTooltip}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              // THAY ĐỔI: Thêm placeholder
              placeholder="Nhập mật khẩu..."
              required
            />
          </div>
          {error && <p className="error-text">{error}</p>}
          <button type="submit" className="login-button">Đăng nhập</button>
        </form>
      </div>
    </div>
  );
}

export default LoginPage;