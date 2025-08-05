import React, { useState, useEffect, useContext } from 'react';
import { PageTitleContext } from '/components/PageTitleContext';
import LeaveRequestViewModal from '/components/LeaveRequestViewModal';
import { useNotifications } from '/components/NotificationContext';
import { useSession } from '/src/hooks/useSession';
import { API_GATEWAY_URL } from '/src/config';

function AllNotificationsPage() {
    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    const {
        notifications,
        fetchNotifications,
        markAsRead
    } = useNotifications();

    const { getAuthHeaders } = useSession();

    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState(null);

    useEffect(() => {
        setPageTitle('Lịch sử thông báo');
        setPageIcon('fa-solid fa-bell');

        const load = async () => {
            setLoading(true);
            await fetchNotifications();
            setLoading(false);
        };

        load();
    }, [setPageTitle, setPageIcon, fetchNotifications]);

    const formatDate = (dateStr) => new Date(dateStr).toLocaleString('vi-VN');

    const handleRowClick = async (notification) => {
        if (!notification.url) return;

        if (!notification.isRead) {
            await markAsRead(notification.id);
        }

        const requestId = notification.url.split('=').pop();
        if (requestId && !isNaN(requestId)) {
            try {
                const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${requestId}`, {
                    headers: getAuthHeaders()
                });
                if (!res.ok) throw new Error("Không thể tải chi tiết đơn.");
                const data = await res.json();
                setSelectedRequest(data);
                setIsModalOpen(true);
            } catch (err) {
                console.error("❌ Lỗi khi tải đơn nghỉ phép:", err.message);
            }
        }
    };

    return (
        <>
            <div className="container-fluid">
                <div className="card shadow-sm">
                    <div className="card-header">
                        <h5 className="mb-0">Lịch sử thông báo</h5>
                    </div>
                    <div className="card-body p-0">
                        {loading ? (
                            <div className="text-center p-5">Đang tải...</div>
                        ) : (
                            <div className="table-responsive">
                                <table className="table table-hover mb-0 align-middle">
                                    <thead>
                                        <tr>
                                            <th style={{ width: '55%' }}>Nội dung</th>
                                            <th>Người gửi</th>
                                            <th>Thời gian</th>
                                            <th>Trạng thái</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {notifications.length > 0 ? (
                                            notifications.map(n => (
                                                <tr
                                                    key={n.id}
                                                    className={n.isRead ? '' : 'table-primary'}
                                                    onClick={() => handleRowClick(n)}
                                                    style={{ cursor: n.url ? 'pointer' : 'default' }}
                                                >
                                                    <td>{n.message}</td>
                                                    <td>{n.triggeredByUsername || 'Hệ thống'}</td>
                                                    <td>{formatDate(n.createdAt)}</td>
                                                    <td>
                                                        <span className={`badge ${n.isRead ? 'bg-secondary' : 'bg-primary'}`}>
                                                            {n.isRead ? 'Đã đọc' : 'Chưa đọc'}
                                                        </span>
                                                    </td>
                                                </tr>
                                            ))
                                        ) : (
                                            <tr>
                                                <td colSpan="4" className="text-center p-5 text-muted">
                                                    Không có thông báo.
                                                </td>
                                            </tr>
                                        )}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <LeaveRequestViewModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                request={selectedRequest}
            />
        </>
    );
}

export default AllNotificationsPage;
