import { useState, useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;
const PAGE_SIZE = 10;

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

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [totalUsers, setTotalUsers] = useState(0);

    const fetchUsers = useCallback(async (pageToFetch) => {
        if (!currentUser || !(currentUser.role === 'Admin' || currentUser.role === 'SuperUser')) {
            setUsers([]);
            setTotalUsers(0);
            setTotalPages(0);
            setLoading(false);
            return;
        }

        setLoading(true);
        setError(null);
        try {
            const isTrashPage = location.pathname === '/trash';
            const queryParams = new URLSearchParams({
                isDeleted: isTrashPage.toString(),
                pageNumber: pageToFetch.toString(),
                pageSize: PAGE_SIZE.toString()
            });
            if (apiSearchTerm) queryParams.append('searchTerm', apiSearchTerm);
            if (apiSelectedRole) queryParams.append('role', apiSelectedRole);

            //const url = `${API_GATEWAY_URL}/api/Users?${queryParams.toString()}`;
            const url = `${API_GATEWAY_URL}/api/LeaveRequests/users-with-leave-details?${queryParams.toString()}`;
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) {
                handleSessionExpired();
                return;
            }
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Lỗi từ server (mã ${response.status}): ${errorText || 'Không có phản hồi'}`);
            }
            
            const data = await response.json();

            setUsers(data.items || []);
            setCurrentPage(pageToFetch);
            setTotalUsers(data.totalCount || 0);
            setTotalPages(Math.ceil((data.totalCount || 0) / PAGE_SIZE));

        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [apiSearchTerm, apiSelectedRole, getAuthHeaders, handleSessionExpired, currentUser, location.pathname]);

    useEffect(() => {
        const handler = setTimeout(() => {
            fetchUsers(currentPage);
        }, 300); // Debounce time

        return () => {
            clearTimeout(handler);
        };
    }, [currentPage, apiSearchTerm, apiSelectedRole, fetchUsers]);

    const handleFilterClick = () => {
        // Nếu trang hiện tại là 1, việc set state sẽ không trigger useEffect.
        // Do đó, ta cần check và gọi fetch trực tiếp nếu cần.
        if (currentPage === 1) {
            fetchUsers(1);
        } else {
            setCurrentPage(1);
        }
        setApiSearchTerm(localSearchTerm);
        setApiSelectedRole(localSelectedRole);
    };

    const handleDeleteUser = async (userId) => {
        const result = await Swal.fire({
            title: 'Chuyển vào thùng rác?',
            text: "Người dùng này sẽ được chuyển vào thùng rác và có thể khôi phục sau.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Vâng, chuyển đi!',
            cancelButtonText: 'Hủy bỏ'
        });

        if (result.isConfirmed) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}`, {
                    method: 'DELETE',
                    headers: getAuthHeaders()
                });
                if (response.status === 401) { handleSessionExpired(); return { error: 'Phiên làm việc đã hết hạn' }; }
                if (!response.ok) throw new Error('Xóa người dùng thất bại');
                
                Swal.fire('Thành công!', 'Người dùng đã được chuyển vào thùng rác.', 'success');
                
                if (users.length === 1 && currentPage > 1) {
                    setCurrentPage(prev => prev - 1);
                } else {
                    fetchUsers(currentPage);
                }
                return { success: true };
            } catch (err) {
                Swal.fire('Lỗi!', err.message, 'error');
                return { error: err.message };
            }
        }
        return { success: false, cancelled: true };
    };

    const handleRestoreUser = async (userId) => {
        const result = await Swal.fire({
            title: 'Khôi phục người dùng?',
            text: "Người dùng này sẽ được khôi phục và có thể đăng nhập lại.",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#aaa',
            confirmButtonText: 'Đúng, khôi phục!',
            cancelButtonText: 'Hủy bỏ'
        });

        if (result.isConfirmed) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}/restore`, {
                    method: 'PUT',
                    headers: getAuthHeaders()
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                Swal.fire('Đã khôi phục!', 'Người dùng đã được khôi phục thành công.', 'success');
                fetchUsers(currentPage, true);
                return { success: true };
            } catch (err) {
                Swal.fire('Lỗi!', err.message, 'error');
                return { error: err.message };
            }
        }
        return { success: false, cancelled: true };
    };

    const handlePermanentDeleteUser = async (userId) => {
        const result = await Swal.fire({
            title: 'XÓA VĨNH VIỄN?',
            html: "Bạn có chắc chắn không? Hành động này <b>KHÔNG THỂ</b> hoàn tác. Tất cả dữ liệu liên quan sẽ bị xóa vĩnh viễn.",
            icon: 'error',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Tôi hiểu, xóa vĩnh viễn!',
            cancelButtonText: 'Hủy bỏ'
        });

        if (result.isConfirmed) {
            try {
                const response = await fetch(`${API_GATEWAY_URL}/api/Users/${userId}/permanent`, {
                    method: 'DELETE',
                    headers: getAuthHeaders()
                });
                if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                Swal.fire('Đã xóa vĩnh viễn!', 'Người dùng đã bị xóa hoàn toàn khỏi hệ thống.', 'success');
                fetchUsers(currentPage, true);
                return { success: true };
            } catch (err) {
                Swal.fire('Lỗi!', err.message, 'error');
                return { error: err.message };
            }
        }
        return { success: false, cancelled: true };
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
            fetchUsers(currentPage);
            return { success: true, message: `Lưu thông tin thành công!` };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleExportToExcel = async (searchTerm, selectedRole) => {
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (selectedRole) queryParams.append('role', selectedRole);
            queryParams.append('isDeleted', 'false');

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
        handleFilterClick,
        handleExportToExcel, handleChangePassword, handleUpdateAvatar,
        isModalOpen, setIsModalOpen, userToEdit, setUserToEdit,
        openAddModal, openEditModal, handleSubmitUserForm,
        localSearchTerm, setLocalSearchTerm,
        localSelectedRole, setLocalSelectedRole,
        currentPage, totalPages, totalUsers, setCurrentPage
    };
}