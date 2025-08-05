import React, { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useSession } from '/src/hooks/useSession';
import * as signalR from '@microsoft/signalr';
import '/src/NotificationBell.css';
import { useNotifications } from '/components/NotificationContext';
import LeaveRequestViewModal from '/components/LeaveRequestViewModal';
import { API_GATEWAY_URL } from '/src/config'; // ✅ Thêm dòng này

function NotificationBell() {
    const { currentUser, getAuthHeaders } = useSession();
    const {
        notifications,
        unreadCount,
        markAsRead,
        markAllAsRead,
        addNotification,
        fetchNotifications,
    } = useNotifications();

    const [isOpen, setIsOpen] = useState(false);
    const [connection, setConnection] = useState(null);
    const [jitterKey, setJitterKey] = useState('nojitter');
    const [modalOpen, setModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState(null);
    const dropdownRef = useRef(null);
    const hasStartedRef = useRef(false);

    useEffect(() => {
        if (currentUser && !connection) {
            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(`${API_GATEWAY_URL}/notificationHub`, {
                    accessTokenFactory: () => localStorage.getItem('jwtToken')
                })
                .withAutomaticReconnect()
                .build();
            setConnection(newConnection);
        }
    }, [currentUser, connection]);

    useEffect(() => {
        if (!connection) return;

        const startConnection = async () => {
            try {
                if (connection.state === 'Disconnected') {
                    console.log('🔌 Starting SignalR connection...');
                    await connection.start();
                    console.log('✅ SignalR Connected!');
                    hasStartedRef.current = true;

                    connection.on("ReceiveNotification", (newNotification) => {
                        setJitterKey(prev => (prev === 'jitter' ? 'nojitter' : 'jitter'));
                        addNotification(newNotification);
                    });

                    connection.onclose(error => {
                        console.warn('⚠️ SignalR disconnected:', error?.message);
                        hasStartedRef.current = false;
                    });
                }
            } catch (error) {
                console.error('SignalR Connection Error: ', error);
            }
        };

        startConnection();

        return () => {
            if (connection && hasStartedRef.current) {
                connection.off("ReceiveNotification");
                connection.stop();
                hasStartedRef.current = false;
            }
        };
    }, [connection, addNotification]);

    useEffect(() => {
        if (currentUser) {
            fetchNotifications();
        }
    }, [currentUser, fetchNotifications]);

    useEffect(() => {
        let interval;
        if (unreadCount > 0) {
            interval = setInterval(() => {
                setJitterKey(prev => (prev === 'jitter' ? 'nojitter' : 'jitter'));
            }, 5000);
        }
        return () => clearInterval(interval);
    }, [unreadCount]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setIsOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const handleNotificationClick = async (notification) => {
        setIsOpen(false);

        if (!notification.isRead) {
            await markAsRead(notification.id);
        }

        const requestId = notification.url?.split('=').pop();
        if (requestId && !isNaN(requestId)) {
            try {
                const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/${requestId}`, {
                    headers: getAuthHeaders()
                });
                if (!res.ok) throw new Error("Không thể tải đơn nghỉ phép");

                const data = await res.json();
                setSelectedRequest({
                    ...data,
                    hoTen: data.user?.name ?? '---',
                    maSoNhanVien: data.user?.username ?? '',
                    boPhan: data.user?.deptName ?? '',
                });
                setTimeout(() => setModalOpen(true), 0);
            } catch (err) {
                console.error("Lỗi tải chi tiết:", err);
            }
        }
    };

    const handleMarkAllRead = async () => {
        if (unreadCount === 0) {
            setIsOpen(false);
            return;
        }
        await markAllAsRead();
        setIsOpen(false);
    };

    return (
        <>
            <li className="nav-item notification-bell-wrapper" ref={dropdownRef}>
                <button
                    className="nav-link notification-bell-button"
                    onClick={() => setIsOpen(prev => !prev)}
                    aria-label="Thông báo"
                >
                    <i key={jitterKey} className="fa-solid fa-bell jitter"></i>
                    {unreadCount > 0 && (
                        <span className="notification-badge">{unreadCount}</span>
                    )}
                </button>

                {isOpen && (
                    <div className="dropdown-menu dropdown-menu-end show notification-dropdown">
                        <div className="notification-dropdown-header">Thông báo</div>
                        <div className="notification-list">
                            {notifications.filter(n => !n.isRead).length > 0 ? (
                                notifications.filter(n => !n.isRead).map(n => (
                                    <div
                                        key={n.id}
                                        className="notification-item unread"
                                        onClick={() => handleNotificationClick(n)}
                                        style={{ cursor: 'pointer' }}
                                    >
                                        <p className="mb-1">{n.message}</p>
                                        <small className="text-muted">
                                            {new Date(n.createdAt).toLocaleString('vi-VN')}
                                        </small>
                                    </div>
                                ))
                            ) : (
                                <div className="no-notification-text">Không có thông báo mới</div>
                            )}
                        </div>
                        <div className="notification-dropdown-footer">
                            <button className="btn btn-link" onClick={handleMarkAllRead}>Đánh dấu đã đọc</button>
                            <Link to="/notifications" className="btn btn-link" onClick={() => setIsOpen(false)}>Xem tất cả</Link>
                        </div>
                    </div>
                )}
            </li>

            <LeaveRequestViewModal
                isOpen={modalOpen}
                onClose={() => setModalOpen(false)}
                request={selectedRequest}
            />
        </>
    );
}

export default NotificationBell;
