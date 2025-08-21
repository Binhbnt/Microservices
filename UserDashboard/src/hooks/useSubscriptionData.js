import { useState, useEffect, useCallback } from 'react';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;
const PAGE_SIZE = 10; // Cấu hình số mục hiển thị trên mỗi trang

function parseDMY(dmyString) {
    if (!dmyString || typeof dmyString !== 'string') { return null; }
    const parts = dmyString.split('/');
    if (parts.length === 3) {
        const day = parseInt(parts[0], 10);
        const month = parseInt(parts[1], 10) - 1;
        const year = parseInt(parts[2], 10);
        if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
            return new Date(Date.UTC(year, month, day));
        }
    }
    return null;
}

export function useSubscriptionData(currentUser, getAuthHeaders, handleSessionExpired) {
    const [subscriptions, setSubscriptions] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterType, setFilterType] = useState('');
    const [totalSubscriptions, setTotalSubscriptions] = useState(0);

    const fetchSubscriptions = useCallback(async (pageNumber = 1) => {
        // Guard clause này vẫn quan trọng để đảm bảo không gọi API khi không cần thiết
        if (!currentUser) return;
        
        setLoading(true);
        setError(null);
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (filterType) queryParams.append('type', filterType);
            queryParams.append('pageNumber', pageNumber.toString());
            queryParams.append('pageSize', PAGE_SIZE.toString());
            
            const url = `${API_GATEWAY_URL}/api/subscriptions?${queryParams.toString()}`;
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) {
                handleSessionExpired();
                return;
            }
            if (!response.ok) {
                throw new Error(`Lỗi từ server (mã ${response.status})`);
            }
            
            const data = await response.json();
            const processedSubscriptions = data.items.map(sub => {
                const dateObject = parseDMY(sub.expiryDate);
                let daysRemaining = null;
                if (dateObject) {
                    const today = new Date();
                    today.setUTCHours(0, 0, 0, 0);
                    const diffTime = dateObject - today;
                    daysRemaining = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                }
                return { ...sub, daysRemaining };
            });
            
            setSubscriptions(processedSubscriptions);
            setCurrentPage(pageNumber);
            setTotalPages(Math.ceil(data.totalCount / PAGE_SIZE));
            setTotalSubscriptions(data.totalCount);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [currentUser, getAuthHeaders, handleSessionExpired, searchTerm, filterType]);

    // SỬA LẠI useEffect ĐỂ THEO DÕI `currentUser`
    useEffect(() => {
        // Chỉ chạy logic fetch khi đã có thông tin người dùng
        if (currentUser) {
            const handler = setTimeout(() => {
                fetchSubscriptions(currentPage);
            }, 300); // Debounce để tránh gọi API liên tục khi gõ tìm kiếm

            return () => clearTimeout(handler);
        } else {
            // Nếu người dùng đăng xuất (currentUser là null), xóa dữ liệu cũ
            setSubscriptions([]);
            setTotalPages(0);
            setTotalSubscriptions(0);
        }
    // THÊM `fetchSubscriptions` VÀO DEPENDENCY ARRAY
    }, [currentPage, searchTerm, filterType, currentUser, fetchSubscriptions]);

    const handleSubmitSubscription = async (subscriptionData, subscriptionId) => {
        const isEditing = !!subscriptionId;
        const method = isEditing ? 'PUT' : 'POST';
        const url = isEditing
            ? `${API_GATEWAY_URL}/api/subscriptions/${subscriptionId}`
            : `${API_GATEWAY_URL}/api/subscriptions`;

        try {
            const response = await fetch(url, {
                method,
                headers: getAuthHeaders(),
                body: JSON.stringify(subscriptionData),
            });

            if (response.status === 401) {
                handleSessionExpired();
                return { error: 'Phiên làm việc đã hết hạn' };
            }
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.title || 'Đã có lỗi xảy ra khi lưu dữ liệu.');
            }
            return { success: true, message: `Lưu thông tin thành công!` };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleDeleteSubscription = async (subscriptionId) => {
        try {
            const url = `${API_GATEWAY_URL}/api/subscriptions/${subscriptionId}`;
            const response = await fetch(url, {
                method: 'DELETE',
                headers: getAuthHeaders(),
            });

            if (response.status === 401) {
                handleSessionExpired();
                return { error: 'Phiên làm việc đã hết hạn' };
            }
            if (!response.ok) throw new Error('Xóa thất bại');
            return { success: true, message: 'Xóa thành công!' };
        } catch (err) {
            return { error: err.message };
        }
    };

    const handleExportExcel = async (searchTerm, filterType) => {
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (filterType) queryParams.append('type', filterType);

            const url = `${API_GATEWAY_URL}/api/subscriptions/export?${queryParams.toString()}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: getAuthHeaders(),
            });

            if (!response.ok) {
                throw new Error('Xuất file Excel thất bại.');
            }

            const disposition = response.headers.get('content-disposition');
            let filename = 'Subscriptions_Export.xlsx';
            if (disposition && disposition.indexOf('attachment') !== -1) {
                const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, '');
                }
            }

            const blob = await response.blob();
            const downloadUrl = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = downloadUrl;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(downloadUrl);

            return { success: true, message: 'Xuất file Excel thành công!' };
        } catch (error) {
            console.error('Lỗi khi xuất Excel:', error);
            return { error: error.message };
        }
    };

    return {
        subscriptions, loading, error, fetchSubscriptions,
        handleSubmitSubscription, handleDeleteSubscription,
        searchTerm, setSearchTerm,
        filterType, setFilterType,
        handleExportExcel,totalSubscriptions,
        currentPage, totalPages, setCurrentPage
    };
}