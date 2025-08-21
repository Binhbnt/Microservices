import React, { useState, useMemo, useContext, useEffect } from 'react';
import { PageTitleContext } from '/components/PageTitleContext';
import SearchFilter from '/components/SearchFilter';
import UserTable from '/components/UserTable';
import UserTableSkeleton from '/components/UserTableSkeleton';
import Pagination from '/components/Pagination';
import { useNavigate } from 'react-router-dom'; // BƯỚC 1

function EmployeePage({
    currentUser, 
    users, 
    loading, 
    error,
    onAddUserClick, 
    onEditUser, 
    onDeleteUser,
    onExportClick, 
    onToastMessage,
    localSearchTerm, 
    setLocalSearchTerm,
    localSelectedRole, 
    setLocalSelectedRole,
    handleFilterClick,
    currentPage, 
    totalPages, 
    setCurrentPage, 
    totalUsers
}) {
    const navigate = useNavigate(); // BƯỚC 2
    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    
    useEffect(() => {
        setPageTitle('Quản lý nhân viên');
        setPageIcon('fa-solid fa-users');
    }, [setPageTitle, setPageIcon]);

    const [sortConfig, setSortConfig] = useState({ key: 'hoTen', direction: 'ascending' });

    const sortedUsers = useMemo(() => {
        if (!users || users.length === 0) return [];
        let sortableUsers = [...users];
        if (sortConfig.key) {
            sortableUsers.sort((a, b) => {
                if (a[sortConfig.key] < b[sortConfig.key]) {
                    return sortConfig.direction === 'ascending' ? -1 : 1;
                }
                if (a[sortConfig.key] > b[sortConfig.key]) {
                    return sortConfig.direction === 'ascending' ? 1 : -1;
                }
                return 0;
            });
        }
        return sortableUsers;
    }, [users, sortConfig]);

    const requestSort = (key) => {
        let direction = 'ascending';
        if (sortConfig.key === key && sortConfig.direction === 'ascending') {
            direction = 'descending';
        }
        setSortConfig({ key, direction });
    };

    const roleOptions = [
        { key: 'Admin', displayName: 'Quản trị viên' },
        { key: 'SuperUser', displayName: 'Quản lý cấp cao' },
        { key: 'User', displayName: 'Người dùng' }
    ];

    const handleExport = async () => {
        if (onToastMessage) {
            onToastMessage({ type: 'info', message: 'Đang chuẩn bị file Excel, vui lòng chờ...' });
        }
        const result = await onExportClick(localSearchTerm, localSelectedRole);
        if (result?.error && onToastMessage) {
            onToastMessage({ type: 'error', message: result.error });
        }
    };
    
    const handlePageChange = (page) => {
        setCurrentPage(page);
    };

    const PAGE_SIZE = 10;
    const startItem = (currentPage - 1) * PAGE_SIZE + 1;
    const endItem = Math.min(currentPage * PAGE_SIZE, totalUsers);

    return (
        <div className="container-fluid">
            {/* BƯỚC 3: THÊM NÚT THÙNG RÁC */}
            <div className="d-flex justify-content-between align-items-center mb-3">
                <div className="d-flex gap-2">
                    <button className="btn btn-primary" onClick={onAddUserClick}>
                        <i className="fas fa-plus me-2"></i>Thêm người dùng
                    </button>
                    <button className="btn btn-secondary" onClick={() => navigate('/trash')}>
                        <i className="fas fa-trash-alt me-2"></i>Thùng rác
                    </button>
                </div>
            </div>
            
            <SearchFilter
                searchTerm={localSearchTerm}
                onSearchChange={(e) => setLocalSearchTerm(e.target.value)}
                onSearchKeyDown={(e) => { if (e.key === 'Enter') handleFilterClick(); }}
                filterLabel="Lọc theo vai trò"
                selectedFilterValue={localSelectedRole}
                onFilterValueChange={(e) => setLocalSelectedRole(e.target.value)}
                filterOptions={roleOptions}
                onFilterClick={handleFilterClick}
                onExportClick={handleExport}
            />

            {loading && <UserTableSkeleton rows={PAGE_SIZE} />}
            {error && <div className="alert alert-danger">{`Lỗi: ${error}`}</div>}
            
            {!loading && !error && (
                <>
                    <UserTable
                        users={sortedUsers}
                        onDeleteUser={onDeleteUser}
                        onEditUser={onEditUser}
                        currentUser={currentUser}
                        requestSort={requestSort}
                        sortConfig={sortConfig}
                    />
                    <div className="d-flex justify-content-between align-items-center mt-3">
                        <span>
                            {totalUsers > 0 
                                ? `Hiển thị ${startItem} - ${endItem} trên tổng số ${totalUsers} mục`
                                : "Không có người dùng nào."
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
        </div>
    );
}

export default EmployeePage;