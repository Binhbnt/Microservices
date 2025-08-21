import React from 'react';
import { useTooltipContext } from '/src/TooltipContext';

// Component giờ sẽ nhận thêm 2 props mới: addNewButton và extraButtons
function SearchFilter({
  searchTerm,
  onSearchChange,
  onSearchKeyDown,
  filterLabel,
  selectedFilterValue,
  onFilterValueChange,
  filterOptions,
  onFilterClick,
  onExportClick,
  addNewButton,   // Prop cho nút "Thêm mới"
  extraButtons,   // Prop cho các nút phụ (Nhập Excel, Tải mẫu)
}) {
  const { showTooltip, hideTooltip } = useTooltipContext();

  return (
    <div className="card shadow-sm mb-4">
      <div className="card-body">
        <div className="row g-3 align-items-center">
          
          {/* Cột bên trái cho nút Thêm mới */}
          <div className="col-12 col-md-auto">
            {addNewButton}
          </div>

          {/* Cột bên phải cho các bộ lọc và nút khác */}
          <div className="col-12 col-md">
            <div className="row g-2 align-items-center justify-content-end">
              <div className="col-md-5">
                <input
                  type="text"
                  className="form-control"
                  placeholder="Nhập từ khóa..."
                  value={searchTerm}
                  onChange={onSearchChange}
                  onKeyDown={onSearchKeyDown}
                />
              </div>
              <div className="col-md-3">
                <select
                  className="form-select"
                  value={selectedFilterValue}
                  onChange={onFilterValueChange}
                >
                  <option value="">{filterLabel || 'Tất cả'}</option>
                  {filterOptions.map(option => (
                    <option key={option.key} value={option.key}>{option.displayName}</option>
                  ))}
                </select>
              </div>
              <div className="col-md-auto d-flex gap-2">
                <button
                  className="btn btn-primary"
                  onClick={onFilterClick}
                  onMouseEnter={(e) => showTooltip("Áp dụng bộ lọc", e)}
                  onMouseLeave={hideTooltip}
                >
                  <i className="fas fa-filter me-1"></i>Lọc
                </button>
                <button
                  className="btn btn-success"
                  onClick={onExportClick}
                  onMouseEnter={(e) => showTooltip("Xuất danh sách ra file Excel", e)}
                  onMouseLeave={hideTooltip}
                >
                  <i className="fas fa-file-excel me-1"></i>Xuất Excel
                </button>
                
                {/* Hiển thị các nút phụ ở đây */}
                {extraButtons}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default SearchFilter;