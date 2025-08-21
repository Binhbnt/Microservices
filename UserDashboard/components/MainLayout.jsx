import React, { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '/components/Sidebar';
import { Navbar } from './';
import { PageTitleContext } from '/components/PageTitleContext';
import '/src/MainLayout.css';
import { useTooltipContext } from '/src/TooltipContext';

function MainLayout({ currentUser, onLogout }) {
  const [pageTitle, setPageTitle] = useState('Trang chủ');
  const [pageIcon, setPageIcon] = useState('fa-solid fa-house');
  const { showTooltip, hideTooltip } = useTooltipContext();
  // 1. Chuyển state `collapsed` lên đây
  const [collapsed, setCollapsed] = useState(false);

  // 2. Chuyển hàm toggle lên đây
  const toggleSidebar = () => {
    setCollapsed(!collapsed);
  };

  return (
    <PageTitleContext.Provider value={{ setPageTitle, setPageIcon }}>
      {/* Thêm class 'sidebar-collapsed' vào div cha khi cần */}
      <div className={`main-layout ${collapsed ? 'sidebar-collapsed' : ''}`}>
        
        {/* 3. Truyền `collapsed` xuống làm prop cho Sidebar */}
        <Sidebar collapsed={collapsed} currentUser={currentUser} />

        {/* 4. Đặt nút bấm ra ngoài, làm anh em với Sidebar */}
        <button onClick={toggleSidebar}
        onMouseEnter={(e) => showTooltip(collapsed ? "Mở rộng" : "Thu gọn", e)}
        onMouseLeave={hideTooltip} 
        //title={collapsed ? "Mở rộng" : "Thu gọn"}
        className="sidebar-toggle-btn">
          <i className={`fa-solid ${collapsed ? 'fa-chevron-right' : 'fa-chevron-left'}`}></i>
        </button>

        <div className="main-content">
          <Navbar 
            currentUser={currentUser} 
            onLogout={onLogout} 
            pageTitle={pageTitle}
            pageIcon={pageIcon}
          />
          <main className="page-content">
            <Outlet />
          </main>
        </div>
      </div>
    </PageTitleContext.Provider>
  );
}

export default MainLayout;