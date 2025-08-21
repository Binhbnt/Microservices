import React, { useEffect, useState, useContext, useCallback, useMemo  } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { PageTitleContext } from '/components/PageTitleContext';
import { useLeaveRequestData } from '/src/hooks/useLeaveRequestData';
import { useSession } from '/src/hooks/useSession';
import LeaveRequestTable from '/components/LeaveRequestTable';
import LeaveRequestFormModal from '/components/LeaveRequestFormModal';
import LeaveRequestViewModal from '/components/LeaveRequestViewModal';
import SearchFilter from '/components/SearchFilter';
import { useTooltipContext } from '/src/TooltipContext';
import { API_GATEWAY_URL } from '/src/config';
import LeaveRequestTableSkeleton from '/components/LeaveRequestTableSkeleton';
import Pagination from '/components/Pagination'; // Import Pagination

const PAGE_SIZE = 10; // Phải khớp với PAGE_SIZE trong hook

function LeaveRequestPage({ onToastMessage }) {
    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    const { currentUser, getAuthHeaders, handleSessionExpired } = useSession();
    const { showTooltip, hideTooltip } = useTooltipContext();

    const [searchTerm, setSearchTerm] = useState('');
    const [selectedStatus, setSelectedStatus] = useState('');
    const [sortConfig, setSortConfig] = useState({ key: 'ngayTao', direction: 'descending' });
    const {
        requests, loading, error, fetchLeaveRequests,
        createLeaveRequest, sendForApproval, cancelLeaveRequest, requestRevocation,
        // Lấy state phân trang từ hook
        currentPage, totalPages, totalRequests, setCurrentPage
    } = useLeaveRequestData(getAuthHeaders, handleSessionExpired);

    const [isFormModalOpen, setIsFormModalOpen] = useState(false);
    const [isViewModalOpen, setIsViewModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState(null);

    const location = useLocation();
    const navigate = useNavigate();

    const statusOptions = [
        { key: 'Pending', displayName: 'Chờ duyệt' },
        { key: 'Approved', displayName: 'Đã duyệt' },
        { key: 'Rejected', displayName: 'Bị từ chối' },
        { key: 'Cancelled', displayName: 'Đã hủy' },
        { key: 'PendingRevocation', displayName: 'Chờ thu hồi' }
    ];

    useEffect(() => {
        setPageTitle('Quản lý Đơn xin phép');
        setPageIcon('fas fa-file-alt');
    }, [setPageTitle, setPageIcon]);

    useEffect(() => {
        const handler = setTimeout(() => {
            fetchLeaveRequests(searchTerm, selectedStatus, currentPage);
        }, 300);
        return () => clearTimeout(handler);
    }, [searchTerm, selectedStatus, currentPage, fetchLeaveRequests]);

    const handleViewDetails = useCallback(async (requestId) => {
        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${requestId}`, {
                headers: getAuthHeaders()
            });
            if (!res.ok) throw new Error("Không thể tải chi tiết đơn.");
            const data = await res.json();
            setSelectedRequest(data);
            setIsViewModalOpen(true);
        } catch (err) {
            onToastMessage?.({ type: 'error', message: err.message });
        }
    }, [getAuthHeaders, onToastMessage]);

    useEffect(() => {
        const queryParams = new URLSearchParams(location.search);
        const requestId = queryParams.get('requestId');
        if (requestId && !isNaN(requestId)) {
            handleViewDetails(parseInt(requestId, 10));
            navigate(location.pathname, { replace: true });
        }
    }, [location.search, handleViewDetails, navigate]);

    const handleApiResponse = (result) => {
        if (!result) return; // Người dùng nhấn Hủy
        if (result.success) {
            // Xử lý logic phân trang sau khi thao tác thành công
            if (requests.length === 1 && currentPage > 1) {
                setCurrentPage(currentPage - 1);
            } else {
                fetchLeaveRequests(searchTerm, selectedStatus, currentPage);
            }
        }
        onToastMessage?.(
            result.error ? { type: 'error', message: result.error } : { type: 'success', message: result.message }
        );
    };

    const handleCreateSubmit = async (formData) => {
        const result = await createLeaveRequest(formData);
        if (result.success) {
            setIsFormModalOpen(false);
            onToastMessage?.({ type: 'success', 'message': 'Tạo đơn thành công!' });
            // Quay về trang 1 để thấy đơn mới nhất
            setCurrentPage(1);
        } else {
            onToastMessage?.({ type: 'error', message: result.error });
        }
    };

    const handleSendForApproval = async (id) => handleApiResponse(await sendForApproval(id));
    const handleCancel = async (id) => handleApiResponse(await cancelLeaveRequest(id));
    const handleRevoke = async (id) => handleApiResponse(await requestRevocation(id));
    
    const handleFilterClick = () => {
        setCurrentPage(1);
        // useEffect sẽ tự động gọi fetchLeaveRequests vì currentPage thay đổi
    };
    
    const handleSearchKeyDown = (e) => { if (e.key === 'Enter') handleFilterClick(); };

    const handleExportClick = async () => {
        try {
            onToastMessage?.({ type: 'info', message: 'Đang chuẩn bị file Excel, vui lòng chờ...' });
            const query = new URLSearchParams();
            if (searchTerm) query.append('searchTerm', searchTerm);
            if (selectedStatus) query.append('status', selectedStatus);

            const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/export?${query.toString()}`, {
                headers: getAuthHeaders()
            });

            if (!res.ok) throw new Error('Lỗi khi xuất Excel');

            const blob = await res.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'Danh_sach_don_xin_phep.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            onToastMessage?.({ type: 'success', message: 'Xuất file Excel thành công!' });
        } catch (err) {
            onToastMessage?.({ type: 'error', message: err.message || 'Xuất Excel thất bại' });
        }
    };

    const startItem = (currentPage - 1) * PAGE_SIZE + 1;
    const endItem = Math.min(currentPage * PAGE_SIZE, totalRequests);

    const sortedRequests = useMemo(() => {
        if (!requests || requests.length === 0) return [];
        let sortableItems = [...requests];
        if (sortConfig.key) {
            sortableItems.sort((a, b) => {
                if (a[sortConfig.key] < b[sortConfig.key]) {
                    return sortConfig.direction === 'ascending' ? -1 : 1;
                }
                if (a[sortConfig.key] > b[sortConfig.key]) {
                    return sortConfig.direction === 'ascending' ? 1 : -1;
                }
                return 0;
            });
        }
        return sortableItems;
    }, [requests, sortConfig]);

    const requestSort = (key) => {
        let direction = 'ascending';
        if (sortConfig.key === key && sortConfig.direction === 'ascending') {
            direction = 'descending';
        }
        setSortConfig({ key, direction });
    };

    return (
        <div className="container-fluid">
            <SearchFilter
                searchTerm={searchTerm}
                onSearchChange={(e) => setSearchTerm(e.target.value)}
                onSearchKeyDown={handleSearchKeyDown}
                filterLabel="Lọc theo trạng thái"
                selectedFilterValue={selectedStatus}
                onFilterValueChange={(e) => setSelectedStatus(e.target.value)}
                filterOptions={statusOptions}
                onFilterClick={handleFilterClick}
                onExportClick={handleExportClick}
                addNewButton={
                    <button
                        className="btn btn-primary"
                        onClick={() => setIsFormModalOpen(true)}
                        onMouseEnter={(e) => showTooltip("Tạo đơn xin phép mới", e)}
                        onMouseLeave={hideTooltip}
                    >
                        <i className="fas fa-plus me-2"></i>Tạo đơn mới
                    </button>
                }
            />

            {loading && <LeaveRequestTableSkeleton rows={PAGE_SIZE} currentUser={currentUser} />}
            {error && <div className="alert alert-danger m-3">{`Lỗi: ${error}`}</div>}
            
            {!loading && !error && (
                <>
                    <LeaveRequestTable
                        requests={sortedRequests}
                        currentUser={currentUser}
                        onSendForApproval={handleSendForApproval}
                        onCancel={handleCancel}
                        onRevoke={handleRevoke}
                        onViewDetails={handleViewDetails}
                        requestSort={requestSort}
                        sortConfig={sortConfig}
                    />
                    <div className="d-flex justify-content-between align-items-center mt-3">
                        <span>
                            {totalRequests > 0 
                                ? `Hiển thị ${startItem} - ${endItem} trên tổng số ${totalRequests} mục`
                                : "Không có đơn xin phép nào."
                            }
                        </span>
                        <Pagination 
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={(page) => setCurrentPage(page)}
                        />
                    </div>
                </>
            )}

            <LeaveRequestFormModal
                isOpen={isFormModalOpen}
                onClose={() => setIsFormModalOpen(false)}
                onSubmit={handleCreateSubmit}
            />

            <LeaveRequestViewModal
                isOpen={isViewModalOpen}
                onClose={() => {
                    setIsViewModalOpen(false);
                    setSelectedRequest(null);
                }}
                request={selectedRequest}
            />
        </div>
    );
}

export default LeaveRequestPage;