import React, { useState, useEffect, useContext, useRef, useMemo } from 'react';
import { useSession } from '/src/hooks/useSession';
import { useSubscriptionData } from '/src/hooks/useSubscriptionData';
import { useEnums } from '/src/hooks/useEnums';
import SubscriptionFormModal from '/components/SubscriptionFormModal';
import SubscriptionTable from '/components/SubscriptionTable';
import { PageTitleContext } from '/components/PageTitleContext';
import SearchFilter from '/components/SearchFilter';
import { API_GATEWAY_URL } from '/src/config';
import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';
import { useTooltipContext } from '/src/TooltipContext';
import SubscriptionTableSkeleton from '/components/SubscriptionTableSkeleton';
import Pagination from '/components/Pagination';

const PAGE_SIZE = 10;

const SubscriptionManagementPage = ({ onToastMessage }) => {
    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    const { currentUser, getAuthHeaders, handleSessionExpired } = useSession();
    const [sortConfig, setSortConfig] = useState({ key: 'sortOrder', direction: 'ascending' });
    const { showTooltip, hideTooltip } = useTooltipContext();

    const {
        subscriptions, loading, error, fetchSubscriptions,
        handleSubmitSubscription, handleDeleteSubscription,
        searchTerm, setSearchTerm,
        filterType, setFilterType,
        handleExportExcel, totalSubscriptions,
        currentPage, totalPages, setCurrentPage
    } = useSubscriptionData(currentUser, getAuthHeaders, handleSessionExpired);

    const { serviceTypeList } = useEnums();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [subscriptionToEdit, setSubscriptionToEdit] = useState(null);
    const fileInputRef = useRef(null);

    const sortedSubscriptions = useMemo(() => {
        if (!subscriptions || subscriptions.length === 0) return [];
        let sortableItems = [...subscriptions];
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
    }, [subscriptions, sortConfig]);

    const requestSort = (key) => {
        let direction = 'ascending';
        if (sortConfig.key === key && sortConfig.direction === 'ascending') {
            direction = 'descending';
        }
        setSortConfig({ key, direction });
    };

    useEffect(() => {
        setPageTitle("Quản lý Gói Dịch vụ");
        setPageIcon('fa-solid fa-list-check');
    }, [setPageTitle, setPageIcon]);

    // Hàm xử lý khi người dùng nhấn nút Lọc
    const handleFilterClick = () => {
        // Khi lọc, luôn quay về trang 1
        setCurrentPage(1);
    };

    // Hàm xử lý khi người dùng chuyển trang
    const handlePageChange = (page) => {
        setCurrentPage(page);
    };

    const openAddModal = () => {
        setSubscriptionToEdit(null);
        setIsModalOpen(true);
    };

    const openEditModal = (subscription) => {
        setSubscriptionToEdit(subscription);
        setIsModalOpen(true);
    };

    const handleSubmit = async (formData) => {
        const result = await handleSubmitSubscription(formData, subscriptionToEdit?.id);
        if (result) {
            onToastMessage({
                type: result.error ? 'error' : 'success',
                message: result.error || result.message
            });
            if (result.success) {
                setIsModalOpen(false);
                //Làm mới trang hiện tại, không phải trang 1
                fetchSubscriptions(currentPage);
            }
        }
    };
    const startItem = (currentPage - 1) * PAGE_SIZE + 1;
    const endItem = Math.min(currentPage * PAGE_SIZE, totalSubscriptions);
    const onDeleteClick = async (id) => {
        Swal.fire({
            title: 'Bạn chắc chắn không?',
            text: "Bạn sẽ không thể hoàn tác hành động này!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Vâng, xóa nó!',
            cancelButtonText: 'Hủy'
        }).then(async (result) => {
            if (result.isConfirmed) {
                const deleteResult = await handleDeleteSubscription(id);
                if (deleteResult.success) {
                    Swal.fire(
                        'Đã xóa!',
                        'Gói dịch vụ đã được xóa thành công.',
                        'success'
                    );
                    // Khi xóa xong, nếu trang hiện tại không còn dữ liệu
                    // và không phải là trang 1, thì lùi về trang trước đó.
                    if (subscriptions.length === 1 && currentPage > 1) {
                        setCurrentPage(currentPage - 1);
                    } else {
                        fetchSubscriptions(currentPage);
                    }
                } else {
                    Swal.fire(
                        'Lỗi!',
                        deleteResult.error || 'Đã có lỗi xảy ra khi xóa.',
                        'error'
                    );
                }
            }
        });
    };

    const handleFileImport = async (event) => {
        const file = event.target.files[0];
        if (!file) return;
        const formData = new FormData();
        formData.append('file', file);
        try {
            const headers = { ...getAuthHeaders() };
            delete headers['Content-Type'];
            const response = await fetch(`${API_GATEWAY_URL}/api/subscriptions/import`, {
                method: 'POST',
                headers: headers,
                body: formData,
            });
            const resultData = await response.json();
            if (!response.ok) {
                throw new Error(resultData.message || 'Import thất bại.');
            }
            onToastMessage({ type: 'success', message: `Import thành công: ${resultData.successCount} mục, thất bại: ${resultData.failCount} mục.` });
            fetchSubscriptions(1);
        } catch (error) {
            onToastMessage({ type: 'error', message: error.message });
        } finally {
            event.target.value = null;
        }
    };

    const handleDownloadTemplate = async () => {
        try {
            const response = await fetch(`${API_GATEWAY_URL}/api/subscriptions/download-template`, {
                method: 'GET',
                headers: getAuthHeaders(),
            });
            if (!response.ok) throw new Error('Không thể tải file mẫu.');
            const disposition = response.headers.get('content-disposition');
            let filename = 'Subscription_Import_Template.xlsx';
            if (disposition && disposition.indexOf('attachment') !== -1) {
                const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            onToastMessage({ type: 'error', message: error.message });
        }
    };

    const handleImportClick = () => {
        fileInputRef.current.click();
    };

    const onExportClick = async () => {
        const result = await handleExportExcel(searchTerm, filterType);
        if (result) {
            onToastMessage({
                type: result.error ? 'error' : 'success',
                message: result.error || result.message
            });
        }
    };

    return (
        <div className="container-fluid">
            <SearchFilter
                searchTerm={searchTerm}
                onSearchChange={e => setSearchTerm(e.target.value)}
                filterLabel="Lọc theo loại dịch vụ"
                selectedFilterValue={filterType}
                onFilterValueChange={e => setFilterType(e.target.value)}
                filterOptions={serviceTypeList}
                onFilterClick={handleFilterClick}
                onExportClick={onExportClick}
                addNewButton={
                    <button onClick={openAddModal} className="btn btn-primary"
                        onMouseEnter={(e) => showTooltip("Thêm mới dịch vụ", e)}
                        onMouseLeave={hideTooltip}
                    >
                        <i className="fas fa-plus me-1"></i>Thêm mới
                    </button>
                }
                // BƯỚC 3 & 4: SỬA LẠI HOÀN TOÀN PROP NÀY
                extraButtons={
                    <>
                        <input type="file" ref={fileInputRef} onChange={handleFileImport} className="d-none" accept=".xlsx, .xls" />
                        <button
                            onClick={handleImportClick}
                            className="btn btn-info"
                            onMouseEnter={(e) => showTooltip("Nhập dữ liệu từ file Excel", e)}
                            onMouseLeave={hideTooltip}
                        >
                            <i className="fas fa-file-import me-1"></i>Nhập Excel
                        </button>
                        <button
                            onClick={handleDownloadTemplate}
                            className="btn btn-outline-secondary"
                            onMouseEnter={(e) => showTooltip("Tải file excel mẫu để nhập liệu", e)}
                            onMouseLeave={hideTooltip}
                        >
                            <i className="fas fa-file-download me-1"></i>Tải mẫu
                        </button>
                    </>
                }
            />

            {/* {loading && <p className="p-5 text-center text-muted">Đang tải dữ liệu...</p>} */}
            {error && <p className="alert alert-danger m-3">Lỗi: {error}</p>}
            {loading && <SubscriptionTableSkeleton rows={10} />}

            {!loading && !error && (
                <>
                    <SubscriptionTable
                        subscriptions={sortedSubscriptions}
                        onEdit={openEditModal}
                        onDelete={onDeleteClick}
                        requestSort={requestSort}
                        sortConfig={sortConfig}
                    />
                    {/* Hiển thị component Pagination */}
                    <div className="d-flex justify-content-between align-items-center mt-3">
                        <span>
                            {totalSubscriptions > 0
                                ? `Hiển thị ${startItem} - ${endItem} trên tổng số ${totalSubscriptions} mục`
                                : "Không có gói dịch vụ nào."
                            }
                        </span>
                        <Pagination
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={handlePageChange}
                        />
                    </div>
                </>
            )}

            <SubscriptionFormModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onSubmit={handleSubmit}
                initialData={subscriptionToEdit}
                serviceTypes={serviceTypeList}
            />
        </div>
    );
};

export default SubscriptionManagementPage;