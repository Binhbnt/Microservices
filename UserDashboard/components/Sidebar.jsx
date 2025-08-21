import React, { useState, useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { menuItems } from '/config/menuItems';
import '/src/Sidebar.css';
import { useTooltipContext } from '/src/TooltipContext';
import AnalogClock from '/components/AnalogClock';
import FlipClock from '/components/FlipClock';
import ModernClock from '/components/ModernClock';
import SessionInfo from '/components/SessionInfo';

function Sidebar({ collapsed, currentUser }) {
    const [currentTime, setCurrentTime] = useState(new Date());
    const { showTooltip, hideTooltip } = useTooltipContext();
    const userRole = currentUser?.role;

    // --- LOGIC LƯU SKIN ĐỒNG HỒ ---

    // 1. Danh sách các skin
    const skins = ['digital-modern', 'analog', 'htc-flip-clock', 'modern-gradient'];

    // 2. State để quản lý skin, đọc từ localStorage khi khởi tạo
    const [currentSkinIndex, setCurrentSkinIndex] = useState(() => {
        const savedSkinIndex = localStorage.getItem('clockSkinIndex');
        return savedSkinIndex !== null ? Number(savedSkinIndex) : 0;
    });

    // 3. State để quản lý thông báo lưu
    const [showSaveNotice, setShowSaveNotice] = useState(false);

    // Cập nhật thời gian mỗi giây
    useEffect(() => {
        const timer = setInterval(() => setCurrentTime(new Date()), 1000);
        return () => clearInterval(timer);
    }, []);

    // 4. Hàm chuyển đổi skin và lưu vào localStorage
    const cycleClockSkin = () => {
        const nextIndex = (currentSkinIndex + 1) % skins.length;

        // Lưu lựa chọn mới
        localStorage.setItem('clockSkinIndex', nextIndex);
        setCurrentSkinIndex(nextIndex);

        // Hiển thị thông báo và tự ẩn sau 2 giây
        setShowSaveNotice(true);
        setTimeout(() => setShowSaveNotice(false), 2000);
    };

    // --- KẾT THÚC LOGIC LƯU SKIN ---


    // Hàm render đồng hồ dựa trên skin được chọn
    const renderClock = () => {
        const skin = skins[currentSkinIndex];

        switch (skin) {
            case 'analog':
                return <AnalogClock time={currentTime} />;
            case 'htc-flip-clock':
                return <FlipClock />;
            case 'modern-gradient':
                return <ModernClock />;
            default: // digital-modern
                return (
                    <div className="digital-clock">
                        <h3>{currentTime.toLocaleDateString('vi-VN', {
                            weekday: 'long',
                            day: '2-digit',
                            month: '2-digit',
                            year: 'numeric'
                        })}</h3>
                        <h5>{currentTime.toLocaleTimeString('vi-VN')}</h5>
                    </div>
                );
        }
    };

    return (
        <div className={`sidebar ${collapsed ? 'collapsed' : ''}`}>
            <div
                className="sidebar-header"
                onClick={cycleClockSkin}
                onMouseEnter={(e) => showTooltip("Đổi đồng hồ", e)}
                onMouseLeave={hideTooltip}
            >
                {!collapsed && renderClock()}

                {/* Phần hiển thị thông báo lưu */}
                {showSaveNotice && (
                    <div className="skin-save-notice">
                        Đã lưu giao diện (trên trình duyệt này)
                    </div>
                )}
            </div>

            <div className="sidebar-menu">
                {menuItems
                    .filter(item => !item.roles || item.roles.includes(userRole))
                    .map((item, index) => (
                        <NavLink
                            key={index}
                            to={item.path}
                            className="menu-item"
                            onMouseEnter={(e) => showTooltip(item.title, e)}
                            onMouseLeave={hideTooltip}
                        >
                            <i className={item.icon}></i>
                            <span className="menu-text">{item.title}</span>
                        </NavLink>
                    ))
                }
            </div>
            {!collapsed && <SessionInfo currentUser={currentUser} />}
        </div>
    );
}

export default Sidebar;