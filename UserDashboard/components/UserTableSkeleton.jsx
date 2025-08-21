// File: /components/UserTableSkeleton.jsx

import React from 'react';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';

const UserTableSkeleton = ({ rows = 5 }) => {
    return (
        <div className="responsive-table-wrapper">
            <table className="app-table">
                <thead>
                    <tr>
                        {/* Tùy chỉnh độ rộng cho từng cột header */}
                        <th style={{ width: '100px' }}><Skeleton /></th>
                        <th style={{ width: '180px' }}><Skeleton /></th>
                        <th style={{ width: '200px' }}><Skeleton /></th>
                        <th style={{ width: '120px' }}><Skeleton /></th>
                        <th style={{ width: '100px' }}><Skeleton /></th>
                        <th style={{ width: '120px' }}><Skeleton /></th>
                        <th style={{ width: '100px' }}><Skeleton /></th>
                        <th style={{ width: '120px' }}><Skeleton /></th>
                        <th style={{ width: '100px' }}><Skeleton /></th>
                    </tr>
                </thead>
                <tbody>
                    {/* Lặp để tạo ra số dòng skeleton mong muốn */}
                    {Array(rows).fill(0).map((_, index) => (
                        <tr key={index}>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            <td><Skeleton /></td>
                            {/* Skeleton cho badge vai trò, mô phỏng hình viên thuốc */}
                            <td><Skeleton height="24px" style={{ borderRadius: '12px' }} /></td>
                            <td><Skeleton /></td>
                            {/* Skeleton cho số ngày phép, mô phỏng hình tròn */}
                            <td style={{ textAlign: 'center' }}>
                                <Skeleton circle width="35px" height="35px" />
                            </td>
                            {/* Skeleton cho 2 nút Sửa/Xóa */}
                            <td><Skeleton height="30px" /></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default UserTableSkeleton;