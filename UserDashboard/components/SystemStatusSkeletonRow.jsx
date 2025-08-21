import React from 'react';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';

// Component này mô phỏng giao diện của một hàng ServiceStatusRow khi đang tải
const SystemStatusSkeletonRow = () => {
    return (
        <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '1rem',
            backgroundColor: '#fff',
            borderRadius: '0.5rem',
            boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
        }}>
            {/* Phần bên trái: Tên service và status */}
            <div style={{ flex: '0 0 250px' }}>
                <Skeleton height={24} width={180} style={{ marginBottom: '0.5rem' }} />
                <Skeleton height={20} width={80} />
            </div>

            {/* Phần bên phải: Các thanh log status */}
            <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '2px',
                flexGrow: 1,
                justifyContent: 'flex-end'
            }}>
                {/* Hiển thị 60 thanh skeleton để khớp với số log được hiển thị */}
                {Array.from({ length: 60 }).map((_, index) => (
                    <Skeleton key={index} width={6} height={24} />
                ))}
            </div>
        </div>
    );
};

export default SystemStatusSkeletonRow;