import { useState, useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;

// THAY ĐỔI 1: Thêm `currentUser` làm tham số đầu vào cho hook
export function useUserData(currentUser, getAuthHeaders, handleSessionExpired, setCurrentUser) {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [userToEdit, setUserToEdit] = useState(null);
    const location = useLocation();
    const [localSearchTerm, setLocalSearchTerm] = useState('');
    const [localSelectedRole, setLocalSelectedRole] = useState('');
    const [apiSearchTerm, setApiSearchTerm] = useState('');
    const [apiSelectedRole, setApiSelectedRole] = useState('');

    const fetchUsers = useCallback(async (isDeleted = false) => {
        const token = localStorage.getItem('jwtToken');
        if (!token) return;

        setLoading(true);
        setError(null);
        try {
            const queryParams = new URLSearchParams({ isDeleted: isDeleted.toString() });
            if (apiSearchTerm) queryParams.append('searchTerm', apiSearchTerm);
            if (apiSelectedRole) queryParams.append('role', apiSelectedRole);

            const url = `${API_GATEWAY_URL}/api/LeaveRequests/users-with-leave-details?${queryParams.toString()}`;
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) { handleSessionExpired(); return; }

            const responseText = await response.text();
            if (!response.ok) throw new Error(`Lỗi từ server (mã ${response.status}): ${responseText}`);
            const data = responseText ? JSON.parse(responseText) : [];
            setUsers(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [apiSearchTerm, apiSelectedRole, getAuthHeaders, handleSessionExpired]);

    // THAY ĐỔI 2: Sửa lại useEffect để kiểm tra vai trò trước khi gọi API
    useEffect(() => {
        const handler = setTimeout(() => {
            // KIỂM TRA VAI TRÒ TRƯỚC KHI GỌI API
            if (currentUser && (currentUser.role === 'Admin' || currentUser.role === 'SuperUser')) {
                // Nếu đúng là Admin/SuperUser, thì mới fetch dữ liệu
                fetchUsers(location.pathname === '/trash');
            } else {
                // Nếu là user thường hoặc chưa có thông tin, không gọi API.
                // Xóa danh sách cũ và tắt loading để đảm bảo giao diện sạch sẽ.
                setUsers([]);
                setLoading(false);
            }
        }, 300);

        return () => clearTimeout(handler);
    }, [currentUser, apiSearchTerm, apiSelectedRole, fetchUsers, location.pathname]); // Thêm currentUser vào dependency array

    const handleDeleteUser = async (userId) => {
        if (window.confirm(`Bạn có chắc muốn xóa người dùng có ID ${userId}?`)) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}`, {
                    method: 'DELETE',
                    headers: getAuthHeaders()
                });
                if (response.status === 401) { handleSessionExpired(); return { error: 'Phiên làm việc đã hết hạn' }; }
                if (!response.ok) throw new Error('Xóa người dùng thất bại');
                fetchUsers(false);
                return { success: true, message: 'Xóa người dùng thành công!' };
            } catch (err) {
                return { error: err.message };
            }
        }
        return null;
    };

    const handleRestoreUser = async (userId) => {
        if (window.confirm(`Bạn có chắc muốn khôi phục người dùng có ID ${userId} không?`)) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}/restore`, {
                    method: 'PUT',
                    headers: getAuthHeaders()
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                fetchUsers(true);
                return { success: true, message: 'Khôi phục người dùng thành công!' };
            } catch (err) {
                return { error: err.message };
            }
        }
        return null;
    };

    const handlePermanentDeleteUser = async (userId) => {
        if (window.confirm(`BẠN CÓ CHẮC CHẮN MUỐN XÓA VĨNH VIỄN người dùng ID ${userId} không? Hành động này không thể hoàn tác!`)) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}/permanent`, {
                    method: 'DELETE',
                    headers: getAuthHeaders()
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                fetchUsers(true);
                return { success: true, message: 'Xóa vĩnh viễn người dùng thành công!' };
            } catch (err) {
                return { error: err.message };
            }
        }
        return null;
    };

    const openAddModal = () => {
        setUserToEdit(null);
        setIsModalOpen(true);
    };

    const openEditModal = async (userId) => {
        setLoading(true);
        setError(null);
        try {
            const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}`, { headers: getAuthHeaders() });
            if (response.status === 401) { handleSessionExpired(); return; }
            if (!response.ok) throw new Error('Không thể tải thông tin người dùng.');
            const userData = await response.json();
            setUserToEdit(userData);
            setIsModalOpen(true);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleSubmitUserForm = async (userData) => {
        try {
            const method = userToEdit ? 'PUT' : 'POST';
            const url = userToEdit
                ? `${API_GATEWAY_URL}/api/Users/${userToEdit.id}`
                : `${API_GATEWAY_URL}/api/Users`;
            const response = await fetch(url, {
                method,
                headers: getAuthHeaders(),
                body: JSON.stringify(userData)
            });

            if (response.status === 401) { handleSessionExpired(); return { error: 'Phiên làm việc đã hết hạn' }; }

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Đã có lỗi xảy ra.');
            }

            setIsModalOpen(false);
            setUserToEdit(null);
            fetchUsers(false);
            return { success: true, message: `Cập nhật thành công!` };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleFilterClick = () => {
        setApiSearchTerm(localSearchTerm);
        setApiSelectedRole(localSelectedRole);
    };

    const handleExportToExcel = async (searchTerm, selectedRole) => {
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (selectedRole) queryParams.append('role', selectedRole);
            queryParams.append('isDeleted', false);

            const url = `${API_GATEWAY_URL}/api/Users/export?${queryParams.toString()}`;

            const response = await fetch(url, {
                method: 'GET',
                headers: getAuthHeaders()
            });

            if (response.status === 401) {
                handleSessionExpired();
                return { error: 'Phiên làm việc đã hết hạn' };
            }

            if (!response.ok) {
                throw new Error(`Lỗi khi xuất file: ${response.status}`);
            }

            const blob = await response.blob();
            const contentDisposition = response.headers.get('Content-Disposition');
            let filename = 'Danh_sach_nhan_vien.xlsx';
            if (contentDisposition && contentDisposition.includes('filename=')) {
                filename = decodeURIComponent(contentDisposition.split('filename=')[1].split(';')[0].replace(/"/g, ''));
            }

            const downloadUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = downloadUrl;
            link.setAttribute('download', filename);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(downloadUrl);

            return { success: true };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleChangePassword = async ({ oldPassword, newPassword }) => {
        try {
            const response = await fetch(`${API_GATEWAY_URL}/api/Users/change-password`, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify({ oldPassword, newPassword })
            });

            if (response.status === 401) return { error: 'Phiên làm việc đã hết hạn' };

            const responseData = await response.json();
            if (!response.ok) throw new Error(responseData.message || 'Mật khẩu cũ không đúng hoặc đã có lỗi xảy ra.');
            return { success: true, message: 'Đổi mật khẩu thành công! Vui lòng đăng nhập lại.' };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleUpdateAvatar = async (file) => {
        const formData = new FormData();
        formData.append('file', file);
        try {
            const headers = getAuthHeaders();
            delete headers['Content-Type'];

            const response = await fetch(`${API_GATEWAY_URL}/api/Users/avatar`, {
                method: 'POST',
                headers,
                body: formData
            });

            if (response.status === 401) { handleSessionExpired(); return null; }

            if (!response.ok) throw new Error("Lỗi khi tải ảnh lên.");
            const data = await response.json();

            if (data?.avatarUrl) {
                setCurrentUser(prevUser => {
                    const updatedUser = { ...prevUser, avatarUrl: data.avatarUrl };
                    localStorage.setItem('lastActiveUser', JSON.stringify(updatedUser));
                    return updatedUser;
                });
            }

            return data;
        } catch (err) {
            return null;
        }
    };

    return {
        users, loading, error, fetchUsers,
        handleDeleteUser, handleRestoreUser, handlePermanentDeleteUser,
        handleFilterClick, setApiSearchTerm, setApiSelectedRole,
        handleExportToExcel, handleChangePassword, handleUpdateAvatar,
        isModalOpen, setIsModalOpen, userToEdit, setUserToEdit,
        openAddModal, openEditModal, handleSubmitUserForm,
        localSearchTerm, setLocalSearchTerm,
        localSelectedRole, setLocalSelectedRole
    };
}