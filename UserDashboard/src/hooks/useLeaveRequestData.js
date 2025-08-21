import { useState, useEffect, useCallback } from 'react';
import Swal from 'sweetalert2';
import 'sweetalert2/dist/sweetalert2.min.css';

const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;
const PAGE_SIZE = 10; // Cấu hình số mục mỗi trang

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

    // State cho phân trang
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [totalRequests, setTotalRequests] = useState(0);

    const fetchLeaveRequests = useCallback(async (searchTerm = '', status = '', pageNumber = 1) => {
        setLoading(true);
        setError(null);
        try {
            const queryParams = new URLSearchParams();
            if (searchTerm) queryParams.append('searchTerm', searchTerm);
            if (status) queryParams.append('status', status);
            queryParams.append('pageNumber', pageNumber.toString());
            queryParams.append('pageSize', PAGE_SIZE.toString());

            const url = `${API_GATEWAY_URL}/api/LeaveRequests?${queryParams.toString()}`;
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) {
                handleSessionExpired();
                return;
            }
            if (!response.ok) {
                const errorBody = await response.json().catch(() => ({ message: `Lỗi khi tải danh sách đơn: ${response.status}` }));
                throw new Error(errorBody.message || `Lỗi HTTP: ${response.status}`);
            }
            const data = await response.json(); // Data giờ là { items: [], totalCount: ... }

            setRequests(data.items || []);
            setCurrentPage(pageNumber);
            setTotalRequests(data.totalCount || 0);
            setTotalPages(Math.ceil((data.totalCount || 0) / PAGE_SIZE));

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
        const result = await Swal.fire({
            title: 'Gửi đơn đi duyệt?',
            text: 'Sau khi gửi, bạn sẽ không thể chỉnh sửa đơn này nữa.',
            icon: 'info',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#aaa',
            confirmButtonText: 'Vâng, gửi đi!',
            cancelButtonText: 'Hủy'
        });

        if (result.isConfirmed) {
            const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/send-for-approval`, {
                method: 'POST',
                headers: getAuthHeaders(),
            });
            return handleApiResponse(res);
        }
        return null;
    };

    const cancelLeaveRequest = async (id) => {
        const result = await Swal.fire({
            title: 'Bạn chắc chắn muốn hủy?',
            text: 'Đơn nghỉ phép này sẽ bị hủy.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Đúng, hủy đơn!',
            cancelButtonText: 'Không'
        });

        if (result.isConfirmed) {
            const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/cancel`, {
                method: 'DELETE',
                headers: getAuthHeaders(),
            });
            return handleApiResponse(res);
        }
        return null;
    };

    const requestRevocation = async (id) => {
        const result = await Swal.fire({
            title: 'Yêu cầu thu hồi đơn?',
            text: 'Một yêu cầu thu hồi sẽ được gửi đến quản lý của bạn.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#ff7f50',
            cancelButtonColor: '#aaa',
            confirmButtonText: 'Gửi yêu cầu!',
            cancelButtonText: 'Hủy'
        });

        if (result.isConfirmed) {
            const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${id}/request-revocation`, {
                method: 'POST',
                headers: getAuthHeaders(),
            });
            return handleApiResponse(res);
        }
        return null;
    };

    return {
        requests,
        loading,
        error,
        fetchLeaveRequests,
        createLeaveRequest,
        sendForApproval,
        cancelLeaveRequest,
        requestRevocation,
        // Trả về state và hàm setter cho phân trang
        currentPage,
        totalPages,
        totalRequests,
        setCurrentPage
    };
}