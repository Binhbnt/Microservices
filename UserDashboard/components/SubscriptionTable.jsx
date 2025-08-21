import React from 'react';
import { useTooltipContext } from '/src/TooltipContext';
import { useEnums } from '/src/hooks/useEnums';

function SubscriptionTable({ subscriptions, onEdit, onDelete,requestSort, sortConfig }) {
  const { showTooltip, hideTooltip } = useTooltipContext();
  const { serviceTypes } = useEnums();

  // XÓA BỎ HOÀN TOÀN HÀM formatDate VÌ KHÔNG CẦN THIẾT NỮA

  const getRemainingDaysBadge = (days) => {
    if (days === null || days === undefined) {
      return <span className="badge bg-light text-dark">N/A</span>;
    }
    if (days < 0) {
      return <span className="badge bg-secondary">Đã hết hạn</span>;
    }
    if (days <= 30) {
      return <span className="badge bg-danger">{days} ngày</span>;
    }
    if (days <= 60) {
      return <span className="badge bg-warning text-dark">{days} ngày</span>;
    }
    return <span className="badge bg-success">{days} ngày</span>;
  };

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
        <thead>
          <tr>
                        <th onClick={() => requestSort('name')}>Tên Dịch Vụ {getSortIcon('name')}</th>
                        <th onClick={() => requestSort('type')}>Loại {getSortIcon('type')}</th>
                        <th onClick={() => requestSort('provider')}>Nhà Cung Cấp {getSortIcon('provider')}</th>
                        <th onClick={() => requestSort('expiryDate')}>Ngày Hết Hạn {getSortIcon('expiryDate')}</th>
                        <th onClick={() => requestSort('daysRemaining')}>Còn Lại {getSortIcon('daysRemaining')}</th>
            <th style={{ minWidth: '120px' }}>Ghi Chú</th>
            <th style={{ minWidth: '120px' }}>Hành Động</th>
          </tr>
        </thead>
        <tbody>
          {subscriptions.length > 0 ? (
            subscriptions.map((sub) => (
              <tr key={sub.id}>
                <td>{sub.name}</td>
                <td>{serviceTypes(sub.type)}</td>
                <td>{sub.provider}</td>
                {/* HIỂN THỊ TRỰC TIẾP GIÁ TRỊ TỪ API */}
                <td>{sub.expiryDate || 'N/A'}</td>
                <td>{getRemainingDaysBadge(sub.daysRemaining)}</td>
                <td>{sub.note}</td>
                <td>
                  <div className="d-flex gap-2 justify-content-center">
                    <button
                      className="btn btn-sm btn-outline-primary"
                      onMouseEnter={(e) => showTooltip("Chỉnh sửa", e)}
                      onMouseLeave={hideTooltip}
                      onClick={() => onEdit(sub)}
                    >
                      <i className="fas fa-pencil-alt"></i>
                    </button>
                    <button
                      className="btn btn-sm btn-outline-danger"
                      onMouseEnter={(e) => showTooltip("Xóa", e)}
                      onMouseLeave={hideTooltip}
                      onClick={() => onDelete(sub.id)}
                    >
                      <i className="fas fa-trash-alt"></i>
                    </button>
                  </div>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="7" className="text-center p-4">
                Không có gói dịch vụ nào.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default SubscriptionTable;