import React from 'react';
import { useEnums } from '/src/hooks/useEnums';
import { useTooltipContext } from '/src/TooltipContext';

function LeaveRequestTable({
  requests,
  currentUser,
  onCancel,
  onSendForApproval,
  onRevoke,
  onViewDetails,
  requestSort,
  sortConfig
}) {

  const getStatusBadge = (statusKey) => {
    const displayName = statuses(statusKey);

    switch (statusKey) {
      case 'Approved':
        return <span className="badge bg-success">{displayName}</span>;
      case 'Pending':
        return <span className="badge bg-warning text-dark">{displayName}</span>;
      case 'Rejected':
        return <span className="badge bg-danger">{displayName}</span>;
      case 'Cancelled':
        return <span className="badge bg-secondary">{displayName}</span>;
      case 'PendingRevocation':
        return <span className="badge bg-info text-dark">{displayName}</span>;
      default:
        return <span className="badge bg-light text-dark">{displayName || statusKey}</span>;
    }
  };
  const { showTooltip, hideTooltip } = useTooltipContext();
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  };

  const formatTime = (timeString) => {
    if (!timeString) return '--:--';
    return timeString.substring(0, 5);
  };
  const { statuses, types, roles } = useEnums();
  const getSortIcon = (key) => {
    if (!sortConfig || sortConfig.key !== key) {
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
        {' '}{icon}
      </span>
    );
  };

  return (
    <div className="responsive-table-wrapper">

      <table className="app-table">
        <thead >
          <tr>
            {/* Thêm checkbox nếu sau này bạn muốn làm hành động hàng loạt */}
            {/* <th><input type="checkbox" /></th> */}

            {currentUser?.role !== 'User' && <th onClick={() => requestSort('hoTen')}>Họ Tên {getSortIcon('hoTen')}</th>}
            {currentUser?.role !== 'User' && <th onClick={() => requestSort('maSoNhanVien')}>Mã NV {getSortIcon('maSoNhanVien')}</th>}
            {currentUser?.role !== 'User' && <th onClick={() => requestSort('boPhan')}>Bộ Phận {getSortIcon('boPhan')}</th>}
            <th onClick={() => requestSort('loaiPhep')}>Loại Phép {getSortIcon('loaiPhep')}</th>
            <th onClick={() => requestSort('ngayTu')}>Từ Ngày {getSortIcon('ngayTu')}</th>
            <th onClick={() => requestSort('ngayDen')}>Đến Ngày {getSortIcon('ngayDen')}</th>
            <th>Từ Giờ</th>
            <th>Đến Giờ</th>
            <th>Lý Do</th>
            <th>Trạng Thái</th>
            <th style={{ minWidth: '220px' }}>Hành Động</th>
          </tr>
        </thead>
        <tbody>
          {requests.length > 0 ? (
            requests.map(req => (
              <tr key={req.id}>
                {/* <td><input type="checkbox" /></td> */}

                {currentUser?.role !== 'User' && <td>{req.hoTen}</td>}
                {currentUser?.role !== 'User' && <td>{req.maSoNhanVien}</td>}
                {currentUser?.role !== 'User' && <td>{req.boPhan}</td>}

                <td>{types(req.loaiPhep)}</td>
                <td>{formatDate(req.ngayTu)}</td>
                <td>{formatDate(req.ngayDen)}</td>
                <td>{formatTime(req.gioTu)}</td>
                <td>{formatTime(req.gioDen)}</td>
                <td style={{ maxWidth: '200px', whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>{req.lyDo}</td>
                <td>{getStatusBadge(req.trangThai)}</td>
                <td>
                  <div className="d-flex gap-2">
                    {/* Luôn có nút Xem chi tiết */}
                    <button className="btn btn-sm btn-outline-info"
                      onMouseEnter={(e) => showTooltip("Xem chi tiết", e)}
                      onMouseLeave={hideTooltip}
                      onClick={() => onViewDetails(req.id)}>
                      <i className="fas fa-eye"></i>
                    </button>

                    {/* Các nút bấm tùy theo trạng thái và quyền */}
                    {req.trangThai === 'Pending' && (
                      <>
                        <button className="btn btn-sm btn-outline-primary"
                          onMouseEnter={(e) => showTooltip("Duyệt", e)}
                          onMouseLeave={hideTooltip}
                          onClick={() => onSendForApproval(req.id)}>
                          <i className="fas fa-paper-plane"></i>
                        </button>
                        <button className="btn btn-sm btn-outline-danger"
                          onMouseEnter={(e) => showTooltip("Hủy đơn", e)}
                          onMouseLeave={hideTooltip}
                          onClick={() => onCancel(req.id)}>
                          <i className="fas fa-times"></i>
                        </button>
                      </>
                    )}

                    {req.trangThai === 'Approved' && currentUser?.role === 'Admin' && (
                      <button className="btn btn-sm btn-outline-warning"
                        onMouseEnter={(e) => showTooltip("Yêu cầu thu hồi", e)}
                        onMouseLeave={hideTooltip}
                        onClick={() => onRevoke(req.id)}>
                        <i className="fas fa-history"></i>
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={currentUser?.role !== 'User' ? 11 : 8} className="text-center p-4">
                Không có đơn xin phép nào.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default LeaveRequestTable;