import React from 'react';
import { Link } from 'react-router-dom';
import { useEnums } from '/src/hooks/useEnums';
import { useTooltipContext } from '/src/TooltipContext';
function TrashUserList({
    users,
    loading,
    error,
    currentUser,
    onRestoreUser,
    onPermanentDelete,
    onToastMessage
}) {
    const formatSqliteDateTime = (sqlDateTimeString) => {
        if (!sqlDateTimeString) return 'N/A';
        const isoString = sqlDateTimeString.replace(' ', 'T') + 'Z';
        const date = new Date(isoString);
        if (isNaN(date.getTime())) return 'N/A';
        return date.toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });
    };
    const { showTooltip, hideTooltip } = useTooltipContext();
    const { roles } = useEnums();

    const renderRoleBadge = (role) => {
        let badgeClass = 'bg-secondary';
        switch (role) {
            case 'Admin': badgeClass = 'bg-primary'; break;
            case 'SuperUser': badgeClass = 'bg-success'; break;
            case 'User': badgeClass = 'bg-info'; break;
        }
        return <span className={`badge ${badgeClass}`}>{roles(role) || role}</span>;
    };

    const determineTrashButtonState = (userInRow) => {
        if (currentUser?.role !== 'Admin') {
            return { isDisabled: true, title: "Chỉ Admin có quyền thực hiện" };
        }
        if (userInRow.id === currentUser.id) {
            return { isDisabled: true, title: "Không thể xóa vĩnh viễn chính mình" };
        }
        return { isDisabled: false };
    };

    if (loading) return <div className="text-center p-5">Đang tải...</div>;
    if (error) return <div className="alert alert-danger">Lỗi: {error}</div>;

    return (
        <div className="trash-wrapper">
            <div className="control-panel mb-4">
                <Link to="/employees"
                    className="btn btn-primary"
                    onMouseEnter={(e) => showTooltip("Quay lại trang danh sách người dùng", e)}
                    onMouseLeave={hideTooltip}>
                    <i className="fas fa-arrow-left"></i> Quay lại Danh sách
                </Link>
            </div>

            <div className="table-responsive">
                <table className="user-table table-hover align-middle audit-log-table">
                    <thead>
                        <tr>
                            <th>Mã số NV</th>
                            <th>Họ tên</th>
                            <th>Email</th>
                            <th>Chức vụ</th>
                            <th>Bộ phận</th>
                            <th>Vai trò</th>
                            <th>Ngày xóa</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.length > 0 ? (
                            users.map(user => {
                                const buttonState = determineTrashButtonState(user);
                                return (
                                    <tr key={user.id}>
                                        <td>{user.maSoNhanVien}</td>
                                        <td>{user.hoTen}</td>
                                        <td>{user.email}</td>
                                        <td>{user.chucVu}</td>
                                        <td>{user.boPhan}</td>
                                        <td>{renderRoleBadge(user.role)}</td>
                                        <td>{formatSqliteDateTime(user.lastUpdatedAt)}</td>
                                        <td>
                                            <div className="d-flex gap-1 flex-wrap">
                                                <button
                                                    className="btn btn-info btn-sm"
                                                    onMouseEnter={(e) => showTooltip("Khôi phục người dùng này", e)}
                                                    onMouseLeave={hideTooltip}
                                                    onClick={() => onRestoreUser(user.id)}
                                                    disabled={buttonState.isDisabled}
                                                    title={buttonState.title || "Khôi phục người dùng"}
                                                >
                                                    Khôi phục
                                                </button>
                                                <button
                                                    className="btn btn-danger btn-sm"
                                                    onMouseEnter={(e) => showTooltip("Xóa VĨNH VIỄN người dùng này", e)}
                                                    onMouseLeave={hideTooltip}
                                                    onClick={() => onPermanentDelete(user.id)}
                                                    disabled={buttonState.isDisabled}
                                                    title={buttonState.title || "Xóa vĩnh viễn người dùng"}
                                                >
                                                    Xóa
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })
                        ) : (
                            <tr>
                                <td colSpan="8" style={{ textAlign: 'center', padding: '20px' }}>Thùng rác trống.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default TrashUserList;
