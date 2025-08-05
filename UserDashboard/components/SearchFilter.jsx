import React from 'react';
import { useTooltipContext } from '/src/TooltipContext';

// Component giờ đây nhận vào các props chung chung hơn
function SearchFilter({
  searchTerm,
  onSearchChange,
  onSearchKeyDown,

  filterLabel,          // Nhãn cho dropdown (ví dụ: "Lọc theo trạng thái")
  selectedFilterValue,  // Giá trị đang được chọn
  onFilterValueChange,  // Hàm xử lý khi chọn giá trị mới
  filterOptions,        // Mảng các lựa chọn cho dropdown

  onFilterClick,
  onExportClick
}) {
  const { showTooltip, hideTooltip } = useTooltipContext();

  return (
    <div className="card shadow-sm mb-4">
      <div className="card-body">
        <div className="row g-2 align-items-center">
          {/* Ô tìm kiếm */}
          <div className="col-md-5">
            <label htmlFor="search-term" className="form-label visually-hidden">Tìm kiếm</label>
            <input
              type="text"
              id="search-term"
              className="form-control"
              placeholder="Nhập từ khóa..."
              value={searchTerm}
              onChange={onSearchChange}
              onKeyDown={onSearchKeyDown}
            />
          </div>

          {/* Dropdown lọc */}
          <div className="col-md-3">
            <label htmlFor="select-filter" className="form-label visually-hidden">{filterLabel}</label>
            <select
              id="select-filter"
              className="form-select"
              value={selectedFilterValue}
              onChange={onFilterValueChange}
            >
              <option value="">Tất cả</option>
              {filterOptions.map(option => (
                <option key={option.key} value={option.key}>{option.displayName}</option>
              ))}
            </select>
          </div>

          {/* Nút lọc + export */}
          <div className="col-md-4 d-flex gap-2 justify-content-end">
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
          </div>
        </div>
      </div>
    </div>
  );
}

export default SearchFilter;