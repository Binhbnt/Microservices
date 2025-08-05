// src/context/NotificationContext.jsx
import React, { createContext, useContext, useState, useCallback } from "react";
import { useSession } from "/src/hooks/useSession";
import { API_GATEWAY_URL } from "/src/config";

const NotificationContext = createContext();

export function NotificationProvider({ children }) {
    const { getAuthHeaders } = useSession();
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);

    const fetchNotifications = useCallback(async () => {
        try {
            const res = await fetch(`${API_GATEWAY_URL}/api/AppNotifications/state`, {
                headers: getAuthHeaders()
            });
            if (!res.ok) return;
            const data = await res.json();
            setNotifications(data.recentNotifications);
            setUnreadCount(data.unreadCount);
        } catch (e) {
            console.error("Failed to fetch notifications:", e);
        }
    }, [getAuthHeaders]);

    // ✅ ĐÃ SỬA: Bọc hàm trong useCallback
    const markAsRead = useCallback(async (id) => {
        try {
            await fetch(`${API_GATEWAY_URL}/api/AppNotifications/${id}/mark-as-read`, {
                method: "POST",
                headers: getAuthHeaders()
            });
            setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
            setUnreadCount(prev => Math.max(0, prev - 1));
        } catch (e) {
            console.error("Failed to mark as read", e);
        }
    }, [getAuthHeaders]);

    // ✅ ĐÃ SỬA: Bọc hàm trong useCallback
    const markAllAsRead = useCallback(async () => {
        try {
            await fetch(`${API_GATEWAY_URL}/api/AppNotifications/mark-all-as-read`, {
                method: "POST",
                headers: getAuthHeaders()
            });
            setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
            setUnreadCount(0);
        } catch (e) {
            console.error("Failed to mark all as read", e);
        }
    }, [getAuthHeaders]);

    // ✅ ĐÃ SỬA: Bọc hàm trong useCallback
    const addNotification = useCallback((notification) => {
        setNotifications(prev => [notification, ...prev.slice(0, 9)]);
        setUnreadCount(prev => prev + 1);
    }, []);

    return (
        <NotificationContext.Provider value={{
            notifications,
            unreadCount,
            fetchNotifications,
            markAsRead,
            markAllAsRead,
            addNotification
        }}>
            {children}
        </NotificationContext.Provider>
    );
}

export const useNotifications = () => useContext(NotificationContext);