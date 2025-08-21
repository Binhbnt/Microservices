import React from 'react';
import SystemStatusSkeletonRow from '/components/SystemStatusSkeletonRow';

// Component này sẽ render một danh sách các hàng skeleton
const SystemStatusPageSkeleton = ({ rows = 3 }) => {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {Array.from({ length: rows }).map((_, index) => (
                <SystemStatusSkeletonRow key={index} />
            ))}
        </div>
    );
};

export default SystemStatusPageSkeleton;