import { useState, useCallback } from 'react';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;

async function handleApiResponse(response) {
    if (response.ok) {
        if (response.status === 204) return { success: true, message: 'Thao tác thành công.' };
        try {
            const data = await response.json();
            return { success: true, message: data.message || 'Thành công.', data };
        } catch (e) {
            return { success: true, message: 'Thành công.' };
        }
    } else {
        const errorData = await response.json().catch(() => ({ message: 'Lỗi không xác định từ server.' }));
        return { error: errorData.message || `Lỗi HTTP: ${response.status}` };
    }
}

export function useLeaveRequestData(getAuthHeaders, handleSessionExpired) {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const fetchLeaveRequests = useCallback(async (searchTerm = '', status = '') => {
        setLoading(true);
        setError(null);
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (status) queryParams.append('status', status);

            // ✅ ĐÃ SỬA: Luôn luôn gọi đến endpoint chung này
            const url = `${API_GATEWAY_URL}/api/LeaveRequests?${queryParams.toString()}`;
            
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) {
                handleSessionExpired();
                return;
            }
            if (!response.ok) {
                // Ném ra lỗi để component có thể bắt và hiển thị
                const errorBody = await response.json().catch(() => ({ message: `Lỗi khi tải danh sách đơn: ${response.status}` }));
                throw new Error(errorBody.message || `Lỗi HTTP: ${response.status}`);
            }
            const data = await response.json();
            setRequests(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [getAuthHeaders, handleSessionExpired]);

    const createLeaveRequest = async (formData) => {
        const response = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(formData),
        });
        return handleApiResponse(response);
    };

    const sendForApproval = async (id) => {
        if (!window.confirm('Bạn có chắc muốn gửi đơn này đi duyệt?')) return null;
        const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/send-for-approval`, {
            method: 'POST',
            headers: getAuthHeaders(),
        });
        return handleApiResponse(res);
    };

    const cancelLeaveRequest = async (id) => {
        // Sửa lại: API này dùng phương thức DELETE hoặc PUT/PATCH, không phải PUT
        if (!window.confirm('Bạn có chắc muốn hủy đơn này?')) return null;
        const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/cancel`, {
            method: 'DELETE', // Chuẩn RESTful hơn
            headers: getAuthHeaders(),
        });
        return handleApiResponse(res);
    };

    const requestRevocation = async (id) => {
        if (!window.confirm('Bạn có chắc muốn yêu cầu thu hồi đơn đã duyệt này?')) return null;
        const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/request-revocation`, {
            method: 'POST',
            headers: getAuthHeaders(),
        });
        return handleApiResponse(res);
    };

    return {
        requests,
        setRequests,
        loading,
        error,
        fetchLeaveRequests,
        createLeaveRequest,
        sendForApproval,
        cancelLeaveRequest,
        requestRevocation,
    };
}