import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '/src/ProfilePage.css';
import { useEnums } from '/src/hooks/useEnums';
import { useTooltipContext } from '/src/TooltipContext';
import { API_GATEWAY_URL } from '/src/config';

function ProfilePage({ currentUser, onOpenEditModal, onUpdateAvatar, onToastMessage }) {
    const [profileData, setProfileData] = useState(currentUser);
    const fileInputRef = useRef(null);
    const navigate = useNavigate();
    const { showTooltip, hideTooltip } = useTooltipContext();

    useEffect(() => {
        setProfileData(currentUser);
    }, [currentUser]);

    if (!profileData) {
        return <div className="container mt-4 text-center">Đang tải thông tin...</div>;
    }

    const handleAvatarClick = () => {
        fileInputRef.current.click();
    };

    const handleFileChange = async (event) => {
        const file = event.target.files[0];
        if (!file) return;

        if (onToastMessage) onToastMessage({ type: 'info', message: 'Đang tải ảnh lên...' });

        const result = await onUpdateAvatar(file);

        if (result && result.avatarUrl) {
            setProfileData(prev => ({ ...prev, avatarUrl: result.avatarUrl }));
            if (onToastMessage) onToastMessage({ type: 'success', message: 'Cập nhật avatar thành công!' });
        } else {
            if (onToastMessage) onToastMessage({ type: 'error', message: 'Tải ảnh lên thất bại.' });
        }
    };

    const { roles } = useEnums();
    const renderRoleBadge = (role) => {
        let badgeClass = 'bg-secondary';
        switch (role) {
            case 'Admin': badgeClass = 'bg-primary'; break;
            case 'SuperUser': badgeClass = 'bg-success'; break;
            case 'User': badgeClass = 'bg-info'; break;
        }
        return <span className={`badge ${badgeClass}`}>{roles(role) || role}</span>;
    };

    return (
        <div className="container mt-3">
            <div className="profile-card">
                <div className="row mx-0">
                    <div className="col-lg-8">
                        <div className="profile-header">
                            <h3>Thông tin tài khoản</h3>
                        </div>
                        <div className="profile-details">
                            <dl>
                                <div className="detail-row"><dt>Mã số nhân viên</dt><dd>{profileData.maSoNhanVien}</dd></div>
                                <div className="detail-row"><dt>Họ và Tên</dt><dd>{profileData.hoTen || profileData.HoTen}</dd></div>
                                <div className="detail-row"><dt>Email</dt><dd>{profileData.email}</dd></div>
                                <div className="detail-row"><dt>Chức vụ</dt><dd>{profileData.chucVu || 'Chưa cập nhật'}</dd></div>
                                <div className="detail-row"><dt>Bộ phận</dt><dd>{profileData.boPhan || 'Chưa cập nhật'}</dd></div>
                                <div className="detail-row"><dt>Vai trò hệ thống</dt><dd>{renderRoleBadge(profileData.role)}</dd></div>
                            </dl>
                        </div>
                    </div>
                    <div className="col-lg-4 d-none d-lg-flex">
                        <div className="profile-logo-container">
                            <div onClick={handleAvatarClick} style={{ cursor: 'pointer' }} title="Bấm để đổi avatar">
                                {profileData.avatarUrl ? (
                                    <img src={`${API_GATEWAY_URL}/${profileData.avatarUrl}`} alt="User Avatar" className="profile-logo" />
                                ) : (
                                    <i className="fa-solid fa-circle-user" style={{ fontSize: '150px', color: '#dee2e6' }}></i>
                                )}
                            </div>
                            <input
                                type="file"
                                ref={fileInputRef}
                                style={{ display: 'none' }}
                                onChange={handleFileChange}
                                accept="image/png, image/jpeg, image/gif"
                            />
                        </div>
                    </div>
                </div>
                <div className="profile-actions">
                    <button className="btn btn-secondary" 
                        onMouseEnter={(e) => showTooltip("Quay về", e)}
                        onMouseLeave={hideTooltip}
                        onClick={() => navigate('/')}
                    >
                        <i className="fa-solid fa-arrow-left me-2"></i>Quay về
                    </button>
                    <button className="btn btn-warning" 
                        onMouseEnter={(e) => showTooltip("Chỉnh sửa", e)}
                        onMouseLeave={hideTooltip}
                        onClick={() => onOpenEditModal(currentUser.id)}
                    >
                        <i className="fa-solid fa-pencil me-2"></i>Chỉnh sửa
                    </button>
                    <Link to="/change-password" className="btn btn-outline-danger"
                        onMouseEnter={(e) => showTooltip("Đổi mật khẩu", e)}
                        onMouseLeave={hideTooltip}
                    >
                        <i className="fa-solid fa-key me-2"></i>Đổi mật khẩu
                    </Link>
                </div>
            </div>
        </div>
    );
}

export default ProfilePage;
