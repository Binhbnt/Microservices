// File: /components/SubscriptionTableSkeleton.jsx

import React from 'react';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css'; // Import CSS một lần ở đây

const SubscriptionTableSkeleton = ({ rows = 5 }) => {
    return (
        <div className="responsive-table-wrapper">
            <table className="app-table">
                <thead>
                    <tr>
                        <th><Skeleton width="150px" /></th>
                        <th><Skeleton width="100px" /></th>
                        <th><Skeleton width="120px" /></th>
                        <th><Skeleton width="100px" /></th>
                        <th><Skeleton width="70px" /></th>
                        <th><Skeleton width="100px" /></th>
                    </tr>
                </thead>
                <tbody>
                    {/* Tạo ra một mảng ảo để lặp và hiển thị số dòng skeleton mong muốn */}
                    {Array(rows).fill(0).map((_, index) => (
                        <tr key={index}>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default SubscriptionTableSkeleton;