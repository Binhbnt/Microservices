import React, { useState, useMemo, useContext, useEffect } from 'react';
import { PageTitleContext } from '/components/PageTitleContext';
import ControlPanel from '/components/ControlPanel'; // Giả sử bạn có component này
import SearchFilter from '/components/SearchFilter';
import UserTable from '/components/UserTable';

function EmployeePage({
    currentUser, users, loading, error,
    onAddUserClick, onEditUser, onDeleteUser,
    onExportClick, onToastMessage,
    localSearchTerm, setLocalSearchTerm,
    localSelectedRole, setLocalSelectedRole,
    handleFilterClick
}) {
    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    useEffect(() => {
        setPageTitle('Quản lý nhân viên');
        setPageIcon('fa-solid fa-users');
    }, [setPageTitle, setPageIcon]);

    const [sortConfig, setSortConfig] = useState({ key: 'hoTen', direction: 'ascending' });

    const sortedUsers = useMemo(() => {
        let sortableUsers = [...users];
        if (sortConfig.key !== null) {
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

    // Định nghĩa các lựa chọn cho dropdown vai trò
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
    
    return (
        <div className="container-fluid">
            <ControlPanel
                onAddUserClick={onAddUserClick}
                onRefreshClick={handleFilterClick}
            />
            {/* 👇 CẬP NHẬT LẠI CÁCH GỌI SearchFilter ĐỂ DÙNG PROPS GENERIC */}
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

            {loading && <div className="text-center p-5">Đang tải...</div>}
            {error && <div className="alert alert-danger">Lỗi: {error}</div>}
            {!loading && !error && <UserTable
                users={sortedUsers}
                onDeleteUser={onDeleteUser}
                onEditUser={onEditUser}
                currentUser={currentUser}
                requestSort={requestSort}
                sortConfig={sortConfig}
            />}
        </div>
    );
}

export default EmployeePage;