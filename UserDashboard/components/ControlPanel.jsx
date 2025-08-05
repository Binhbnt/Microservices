// src/components/ControlPanel.jsx
import React from 'react';
import { useNavigate } from 'react-router-dom'; // Thêm useNavigate
import { Link } from 'react-router-dom'; // Thêm Link nếu cần
import { useTooltipContext } from '/src/TooltipContext';
// Nhận props onAddUserClick
function ControlPanel({ onAddUserClick }) {
  const navigate = useNavigate(); // useNavigate ở đây là đúng
  const { showTooltip, hideTooltip } = useTooltipContext();
  const handleGoToTrash = () => {
    navigate('/trash');
  };

  return (
    <div className="control-panel">
      <button
        className="btn btn-primary"
        data-tooltip="Thêm người dùng mới vào hệ thống"
        onClick={onAddUserClick}
        onMouseEnter={(e) => showTooltip("Thêm người dùng mới vào hệ thống", e)}
        onMouseLeave={hideTooltip}
      >
        <i className="fas fa-plus"></i> Thêm người dùng
      </button>
      <button
        className="btn btn-secondary "
        data-tooltip="Xem danh sách người dùng đã xóa mềm"
        onClick={handleGoToTrash}
        onMouseEnter={(e) => showTooltip("Xem nhân viên đã xóa", e)}
        onMouseLeave={hideTooltip}
      >
        <i className="fas fa-trash"></i> Thùng rác
      </button>
    </div>
  );
}

export default ControlPanel;