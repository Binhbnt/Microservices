// Định nghĩa component này ở cuối file DashboardPage.jsx hoặc tạo file riêng
import { useNavigate } from 'react-router-dom';

function RequestListColumn({ title, requests, color, icon }) {
  const navigate = useNavigate();

  const handleNavigate = () => {
    navigate('/leave-requests');
  };

  return (
    <div className="col-lg-4">
      <div className="card h-100 shadow-sm">
        <div className={`card-header text-bg-${color}`}>
          <h5 className="mb-0">
            <i className={`fa-solid ${icon} me-2`}></i>
            {title}
          </h5>
        </div>
        <ul className="list-group list-group-flush">
          {requests && requests.length > 0 ? (
            requests.map(req => (
              <li key={req.id} className="list-group-item list-group-item-action" onClick={handleNavigate} style={{ cursor: 'pointer' }}>
                <div className="d-flex w-100 justify-content-between">
                  <h6 className="mb-1">{req.userFullName}</h6>
                  <small>{new Date(req.ngayTu).toLocaleDateString('vi-VN')}</small>
                </div>
                <p className="mb-1">{req.loaiPhep}</p>
              </li>
            ))
          ) : (
            <li className="list-group-item">Không có đơn nào.</li>
          )}
        </ul>
      </div>
    </div>
  );
}