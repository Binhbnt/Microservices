import React from 'react';
import { useTooltipContext } from '/src/TooltipContext';
import { useEnums } from '/src/hooks/useEnums';

// CSS for the leave details tooltip
const leaveTooltipStyle = {
  textAlign: 'left',
  lineHeight: 1.6,
  padding: '2px 4px'
};

function UserTable({ users, onDeleteUser, onEditUser, currentUser, requestSort, sortConfig }) {
  const { showTooltip, hideTooltip } = useTooltipContext();
  const { roles } = useEnums();

  // --- CORRECTED DATE FORMATTING FUNCTION ---
   const formatDate = (dateString) => {
    // Trực tiếp nhận chuỗi ngày đầy đủ từ backend (ví dụ: "2025-06-27T07:57:35.7807993")
    if (!dateString) return 'N/A';
    
    // new Date() của JavaScript có thể đọc trực tiếp định dạng chuẩn này
    const date = new Date(dateString);
    
    // Kiểm tra xem ngày có hợp lệ không
    if (isNaN(date.getTime())) {
      return 'N/A'; // Nếu không hợp lệ, trả về N/A
    }
    
    // Trả về định dạng dd/MM/yyyy
    return date.toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  };

  const getSortIcon = (key) => {
    if (sortConfig.key !== key) {
      return null;
    }
    const isAscending = sortConfig.direction === 'ascending';
    const tooltipText = isAscending ? "Sắp xếp giảm dần" : "Sắp xếp tăng dần";
    const icon = isAscending ? '▲' : '▼';
    return (
      <span
        className="sort-icon"
        onMouseEnter={(e) => showTooltip(tooltipText, e)}
        onMouseLeave={hideTooltip}
      >
        {icon}
      </span>
    );
  };

  const renderRoleBadge = (role) => {
    let badgeClass = 'bg-secondary';
    switch (role) {
      case 'Admin': badgeClass = 'bg-primary'; break;
      case 'SuperUser': badgeClass = 'bg-success'; break;
      case 'User': badgeClass = 'bg-info'; break;
    }
    return <span className={`badge ${badgeClass}`}>{roles(role) || role}</span>;
  };

  const getDeleteButtonState = (userInRow) => {
    if (!currentUser) {
      return { isDisabled: true, title: "Đang tải quyền..." };
    }
    if (userInRow.role === 'Admin') {
      return { isDisabled: true, title: "Không thể xóa tài khoản Admin" };
    }
    if (userInRow.id === currentUser.id) {
      return { isDisabled: true, title: "Bạn không thể tự xóa chính mình" };
    }
    if (currentUser.role === 'SuperUser') {
      if (userInRow.role === 'User' && userInRow.boPhan === currentUser.boPhan) {
        return { isDisabled: false, title: "Xóa người dùng" };
      } else {
        return { isDisabled: true, title: "Chỉ có thể xóa 'User' trong cùng bộ phận" };
      }
    }
    if (currentUser.role === 'Admin') {
      return { isDisabled: false, title: "Xóa người dùng" };
    }
    return { isDisabled: true, title: "Bạn không có quyền này" };
  };

  return (
    <div className="responsive-table-wrapper">
      <table className="app-table">
        <thead>
          <tr>
            <th onClick={() => requestSort('maSoNhanVien')}>Mã số NV {getSortIcon('maSoNhanVien')}</th>
            <th onClick={() => requestSort('hoTen')}>Họ tên {getSortIcon('hoTen')}</th>
            <th onClick={() => requestSort('email')}>Email {getSortIcon('email')}</th>
            <th>Chức vụ</th>
            <th onClick={() => requestSort('boPhan')}>Bộ phận {getSortIcon('boPhan')}</th>
            <th>Vai trò</th>
            <th onClick={() => requestSort('ngayTao')}>Ngày tạo {getSortIcon('ngayTao')}</th>
            <th>Phép Được Hưởng</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {users.length > 0 ? (
            users.map(user => {
              const deleteButtonState = getDeleteButtonState(user);
              return (
                <tr key={user.id}>
                  <td>{user.maSoNhanVien}</td>
                  <td>{user.hoTen}</td>
                  <td>{user.email}</td>
                  <td>{user.chucVu}</td>
                  <td>{user.boPhan}</td>
                  <td>{renderRoleBadge(user.role)}</td>
                  {/* Use the new, more reliable formatDate function */}
                  <td>{formatDate(user.ngayTao)}</td>
                  <td className="text-center">
                    <span
                      className="license-points"
                      onMouseEnter={(e) => {
                        const tooltipContent = (
                          <div style={leaveTooltipStyle}>
                            <div className="fw-bold mb-1">Chi tiết ngày phép</div>
                            <div>Được hưởng: {user.totalLeaveEntitlement} ngày</div>
                            <div>Đã nghỉ: {user.daysTaken?.toFixed(1) || '0.0'} ngày</div>
                          </div>
                        );
                        showTooltip(tooltipContent, e);
                      }}
                      onMouseLeave={hideTooltip}
                    >
                      {(user.remainingLeaveDays ?? 0).toFixed(1)}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn btn-warning btn-sm"
                        onMouseEnter={(e) => showTooltip("Chỉnh sửa thông tin người dùng này", e)}
                        onMouseLeave={hideTooltip}
                        onClick={() => onEditUser(user.id)}
                      >
                        Sửa
                      </button>
                      <button
                        className="btn btn-danger btn-sm"
                        onMouseEnter={(e) => showTooltip("Xóa người dùng này", e)}
                        onMouseLeave={hideTooltip}
                        onClick={() => onDeleteUser(user.id)}
                        disabled={deleteButtonState.isDisabled}
                        title={deleteButtonState.title}
                      >
                        Xóa
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })
          ) : (
            <tr>
              <td colSpan="9" style={{ textAlign: 'center', padding: '20px' }}>Không có người dùng nào.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default UserTable;