import React from 'react';
import { Link } from 'react-router-dom';
import NotificationBell from '/components/NotificationBell';
import { API_GATEWAY_URL } from '/src/config'; // Tối ưu: Import từ file config

// Định nghĩa các vai trò để code sạch hơn
const ROLES = {
  ADMIN: 'Admin',
  SUPER_USER: 'SuperUser',
};

function Navbar({ currentUser, onLogout, pageTitle, pageIcon }) {
  // === LOGIC KIỂM TRA VAI TRÒ ===
  const isAdmin = currentUser?.role === ROLES.ADMIN;
  const isSuperUser = currentUser?.role === ROLES.SUPER_USER;
  
  const relativeAvatarUrl = currentUser?.avatarUrl;

  // === LOGIC CHỌN LINK HƯỚNG DẪN ĐỘNG ===
  const helpLink = isAdmin
    ? '/Help/guide-admin.html'       // Nếu là Admin
    : isSuperUser
      ? '/Help/guide-superuser.html' // Nếu là Super User
      : '/Help/guide-user.html';       // Còn lại là User

  return (
    <nav className="navbar navbar-expand-sm navbar-light px-3 mb-4">
      <div className="container rounded-3 px-3 py-2 shadow-sm bg-white">
        <div className="d-flex align-items-center">
          <i className={`${pageIcon} fs-4 me-3 text-primary`}></i>
          <h2 className="navbar-brand fs-4 fw-bold mb-0 text-primary">
            {pageTitle}
          </h2>
        </div>
        <div className="collapse navbar-collapse justify-content-end">
          <ul className="navbar-nav align-items-center">
            {currentUser ? (
              <>
                <NotificationBell />
                <li className="nav-item dropdown">
                  <a className="nav-link dropdown-toggle d-flex align-items-center" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                    {relativeAvatarUrl ? (
                      <img
                        src={`${API_GATEWAY_URL}/${relativeAvatarUrl}`}
                        alt="Avatar"
                        className="rounded-circle me-2"
                        style={{ width: '32px', height: '32px', objectFit: 'cover' }}
                      />
                    ) : (
                      <i className="fa-solid fa-circle-user fs-4 me-2"></i>
                    )}
                    Chào mừng, {'\u00A0'}<strong className="text-success">{currentUser.hoTen || currentUser.HoTen || currentUser.email || 'User'}</strong>
                  </a>
                  <ul className="dropdown-menu dropdown-menu-end">
                    <li>
                      <Link className="dropdown-item" to="/profile">
                        <i className="fa-solid fa-address-card me-2"></i>Thông tin tài khoản
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/change-password">
                        <i className="fa-solid fa-key me-2"></i>Đổi mật khẩu
                      </Link>
                    </li>
                    <li>
                      <a className="dropdown-item" href={helpLink} target="_blank" rel="noopener noreferrer">
                        <i className="fa-solid fa-circle-question me-2"></i>Hướng dẫn sử dụng
                      </a>
                    </li>
                    {isAdmin && (
                      <>
                        <li><hr className="dropdown-divider" /></li>
                        <li>
                          <Link className="dropdown-item" to="/audit-log">
                            <i className="fa-solid fa-clipboard-list me-2"></i>Nhật ký hệ thống
                          </Link>
                        </li>
                      </>
                    )}
                    <li><hr className="dropdown-divider" /></li>
                    <li>
                      <button className="dropdown-item" onClick={onLogout}>
                        <i className="fa-solid fa-right-from-bracket me-2"></i>Đăng xuất
                      </button>
                    </li>
                  </ul>
                </li>
              </>
            ) : (
              <li className="nav-item">
                <Link to="/login" className="nav-link">
                  <i className="fa-solid fa-right-to-bracket"></i> Đăng nhập
                </Link>
              </li>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;