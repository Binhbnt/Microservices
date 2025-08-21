// File: /components/LeaveRequestTableSkeleton.jsx

import React from 'react';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';

// Component nhận props `rows` và `currentUser`
const LeaveRequestTableSkeleton = ({ rows = 5, currentUser }) => {
    // Xác định xem có nên hiển thị các cột của quản lý hay không
    const isManagerView = currentUser?.role !== 'User';

    return (
        <div className="responsive-table-wrapper">
            <table className="app-table">
                <thead>
                    <tr>
                        {isManagerView && <th><Skeleton width="150px" /></th>}
                        {isManagerView && <th><Skeleton width="80px" /></th>}
                        {isManagerView && <th><Skeleton width="100px" /></th>}
                        <th><Skeleton width="100px" /></th>
                        <th><Skeleton width="100px" /></th>
                        <th><Skeleton width="100px" /></th>
                        <th><Skeleton width="60px" /></th>
                        <th><Skeleton width="60px" /></th>
                        <th><Skeleton width="150px" /></th>
                        <th><Skeleton width="100px" /></th>
                        <th style={{ minWidth: '220px' }}><Skeleton width="120px" /></th>
                    </tr>
                </thead>
                <tbody>
                    {Array(rows).fill(0).map((_, index) => (
                        <tr key={index}>
                            {isManagerView && <td><Skeleton /></td>}
                            {isManagerView && <td><Skeleton /></td>}
                            {isManagerView && <td><Skeleton /></td>}
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            {/* Skeleton cho badge trạng thái */}
                            <td><Skeleton height="24px" style={{ borderRadius: '12px' }} /></td>
                            {/* Skeleton cho các nút hành động */}
                            <td><Skeleton height="32px" width="100px" /></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default LeaveRequestTableSkeleton;