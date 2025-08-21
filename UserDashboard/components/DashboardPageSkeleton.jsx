import React from 'react';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';

const DashboardPageSkeleton = () => {
    return (
        <div className="p-4 dashboard-page">
            {/* KHU VỰC 1: SKELETON CHO THỐNG KÊ NGÀY PHÉP */}
            <h2 className="dashboard-section-title">
                <Skeleton width={400} height={32} />
            </h2>
            <div className="row g-4 mb-5">
                <div className="col-lg-4">
                    <Skeleton height={120} borderRadius="0.5rem" />
                </div>
                <div className="col-lg-4">
                    <Skeleton height={120} borderRadius="0.5rem" />
                </div>
                <div className="col-lg-4">
                    <Skeleton height={120} borderRadius="0.5rem" />
                </div>
            </div>

            {/* KHU VỰC 2: SKELETON CHO TỔNG QUAN ĐƠN TỪ & DỊCH VỤ */}
            <h2 className="dashboard-section-title">
                <Skeleton width={450} height={32} />
            </h2>
            {/* Stat Cards */}
            <div className="row g-3 mb-4">
                {Array.from({ length: 6 }).map((_, index) => (
                    <div className="col-md-4 col-xl-2" key={index}>
                        <Skeleton height={80} borderRadius="0.5rem" />
                    </div>
                ))}
            </div>

            {/* Biểu đồ */}
            <div className="row g-4 mb-5">
                {/* Biểu đồ cột */}
                <div className="col-xl-8 col-lg-7">
                    <Skeleton height={350} borderRadius="0.5rem" />
                </div>
                {/* Biểu đồ tròn */}
                <div className="col-xl-4 col-lg-5">
                     <div className="card shadow-sm h-100">
                        <div className="card-header">
                            <h6 className="m-0">
                                <Skeleton width={150} />
                            </h6>
                        </div>
                        <div className="card-body d-flex align-items-center justify-content-center">
                            <Skeleton circle height={250} width={250} />
                        </div>
                    </div>
                </div>
            </div>


            {/* KHU VỰC 3: SKELETON CHO TRUY CẬP NHANH */}
            <h2 className="dashboard-section-title">
                 <Skeleton width={250} height={32} />
            </h2>
            <div className="row g-4">
                <div className="col-lg-4">
                    <Skeleton height={200} borderRadius="0.5rem" />
                </div>
                <div className="col-lg-4">
                    <Skeleton height={200} borderRadius="0.5rem" />
                </div>
                <div className="col-lg-4">
                    <Skeleton height={200} borderRadius="0.5rem" />
                </div>
            </div>
        </div>
    );
};

export default DashboardPageSkeleton;