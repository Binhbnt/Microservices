import React, { useEffect, useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageTitleContext } from '/components/PageTitleContext';
import { BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid, ResponsiveContainer } from 'recharts';
import { useSession } from '/src/hooks/useSession';
import { API_GATEWAY_URL } from '/src/config';
import SubscriptionPieChart from './SubscriptionPieChart';
import DashboardPageSkeleton from '/components/DashboardPageSkeleton'; 
// ----- CÁC COMPONENT CON CHO DASHBOARD -----

function LeaveBalanceCard({ label, value, color, icon }) {
  return (
    <div className="col-lg-4">
      <div className={`card text-white h-100 shadow-sm bg-gradient-${color} leave-balance-card`}>
        <div className="card-body d-flex align-items-center p-4">
          <div className="flex-shrink-0 icon-container">
            <i className={`fa-solid ${icon}`}></i>
          </div>
          <div className="flex-grow-1 ms-4 text-end">
            <h5 className="mb-1">{label}</h5>
            <p className="display-5 fw-bold mb-0 value-display">{value.toFixed(1)}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

function StatCard({ label, value, color }) {
  return (
    <div className="col-md-4 col-xl-2">
      <div className={`card text-bg-${color} h-100 shadow-sm`}>
        <div className="card-body text-center p-3">
          <h6 className="card-title text-uppercase small">{label}</h6>
          <p className="display-6 fw-bold mb-0">{value}</p>
        </div>
      </div>
    </div>
  );
}

function RequestListColumn({ title, requests, color, icon }) {
  const navigate = useNavigate();
  const handleNavigate = () => navigate('/leave-requests');

  return (
    <div className="col-lg-4">
      <div className="card h-100 shadow-sm">
        <div className={`card-header text-bg-${color}`}>
          <h5 className="mb-0"><i className={`fa-solid ${icon} me-2`}></i>{title}</h5>
        </div>
        <ul className="list-group list-group-flush">
          {requests && requests.length > 0 ? (
            requests.map(req => (
              <li key={req.id} className="list-group-item list-group-item-action request-list-item" onClick={handleNavigate} style={{ cursor: 'pointer' }}>
                <div className="d-flex w-100 justify-content-between">
                  <h6 className="mb-1">{req.userFullName}</h6>
                  <small>{new Date(req.ngayTu).toLocaleDateString('vi-VN')}</small>
                </div>
                <p className="mb-1 text-muted">{req.loaiPhep}</p>
              </li>
            ))
          ) : (
            <li className="list-group-item text-muted">Không có đơn nào.</li>
          )}
        </ul>
      </div>
    </div>
  );
}

// ----- COMPONENT CHÍNH CỦA TRANG DASHBOARD -----

function DashboardPage() {
  const { currentUser } = useSession();
  const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState(null);
  const canViewOverview = currentUser && (currentUser.role === 'Admin' || currentUser.role === 'SuperUser');

  useEffect(() => {
    setPageTitle('Trang chủ');
    setPageIcon('fa-solid fa-house');

    const fetchStats = async () => {
      try {
        //await new Promise(resolve => setTimeout(resolve, 1500)); Giả lập 1,5s
        const token = localStorage.getItem('jwtToken');
        if (!token) throw new Error('Không tìm thấy token');

        const res = await fetch(`${API_GATEWAY_URL}/api/LeaveRequests/dashboard-stats`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        if (!res.ok) throw new Error(`Lỗi HTTP ${res.status}`);

        const data = await res.json();
        setStats(data);
      } catch (err) {
        setErrorMsg(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [setPageTitle, setPageIcon]);

  if (loading) return <DashboardPageSkeleton />;
  if (errorMsg) return <div className="p-4 alert alert-danger">Lỗi khi tải thống kê: {errorMsg}</div>;
  if (!stats) return <div className="p-5 text-center text-muted">Không có dữ liệu thống kê.</div>;

  return (
    <div className="p-4 dashboard-page">
      {/* KHU VỰC 1: THỐNG KÊ PHÉP NĂM CÁ NHÂN */}
      <h2 className="dashboard-section-title">Thống kê ngày phép năm {new Date().getFullYear()}</h2>
      <div className="row g-4 mb-5">
        <LeaveBalanceCard label="Ngày được hưởng" value={stats.currentUserTotalEntitlement} color="primary" icon="fa-calendar-check" />
        <LeaveBalanceCard label="Ngày đã nghỉ" value={stats.currentUserDaysTaken} color="warning" icon="fa-calendar-xmark" />
        <LeaveBalanceCard label="Ngày còn lại" value={stats.currentUserDaysRemaining} color="success" icon="fa-calendar-plus" />
      </div>

      {canViewOverview && (
        <>
          {/* KHU VỰC 2: TỔNG QUAN ĐƠN TỪ CÔNG TY/NHÓM */}
          <h2 className="dashboard-section-title">Tổng quan Đơn từ & Dịch vụ</h2>
          <div className="row g-3 mb-4">
            <StatCard label="Tổng đơn" value={stats.totalRequests} color="primary" />
            <StatCard label="Chờ duyệt" value={stats.pendingRequests} color="warning" />
            <StatCard label="Đã duyệt" value={stats.approvedRequests} color="success" />
            <StatCard label="Từ chối" value={stats.rejectedRequests} color="danger" />
            <StatCard label="Đã hủy" value={stats.cancelledRequests} color="secondary" />
            <StatCard label="Chờ thu hồi" value={stats.waitingRevocation} color="info" />
          </div>

          {/* SỬA LẠI KHỐI BIỂU ĐỒ */}
          <div className="row g-4 mb-5">
            {/* Biểu đồ đơn từ */}
            <div className="col-xl-8 col-lg-7">
              <div className="card shadow-sm h-100">
                <div className="card-header">
                  <h6 className="m-0 font-weight-bold text-primary">Biểu đồ số đơn theo ngày (30 ngày qua)</h6>
                </div>
                <div className="card-body">
                  {Array.isArray(stats.dailyCounts) && stats.dailyCounts.length > 0 ? (
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={stats.dailyCounts} margin={{ top: 5, right: 20, left: -10, bottom: 5 }}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="date" tickFormatter={(dateStr) => new Date(dateStr).toLocaleDateString('vi-VN')} />
                        <YAxis allowDecimals={false} />
                        <Tooltip />
                        <Bar dataKey="count" fill="#0d6efd" radius={[4, 4, 0, 0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  ) : (
                    <div className="d-flex align-items-center justify-content-center h-100 text-center p-5 text-muted fst-italic">
                      Không có dữ liệu để vẽ biểu đồ.
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Biểu đồ gói dịch vụ */}
            <div className="col-xl-4 col-lg-5">
              <div className="card shadow-sm h-100">
                <div className="card-header">
                  <h6 className="m-0 font-weight-bold text-primary">Tỷ lệ Gói Dịch vụ</h6>
                </div>
                <div className="card-body d-flex align-items-center justify-content-center">
                  <SubscriptionPieChart />
                </div>
              </div>
            </div>
          </div>
        </>
      )}

      {/* KHU VỰC 3: CÁC DANH SÁCH TRUY CẬP NHANH */}
      <h2 className="dashboard-section-title">Truy cập nhanh</h2>
      <div className="row g-4">
        <RequestListColumn title="Đơn chờ duyệt" requests={stats.PendingList || stats.pendingList} color="warning" icon="fa-hourglass-half" />
        <RequestListColumn title="Đơn đã duyệt gần đây" requests={stats.ApprovedList || stats.approvedList} color="success" icon="fa-check-circle" />
        <RequestListColumn title="Đơn bị từ chối/hủy gần đây" requests={stats.RejectedOrCancelledList || stats.rejectedOrCancelledList} color="secondary" icon="fa-times-circle" />
      </div>
    </div>
  );
}

export default DashboardPage;
