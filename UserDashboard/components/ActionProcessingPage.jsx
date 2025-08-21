import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { API_GATEWAY_URL } from '/src/config';

function ActionProcessingPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [status, setStatus] = useState('Đang tải...');
  const [message, setMessage] = useState('Vui lòng chờ trong giây lát.');
  const [showReasonForm, setShowReasonForm] = useState(false);
  const [reason, setReason] = useState('');

  const token = searchParams.get('token');
  const action = searchParams.get('action');

  useEffect(() => {
    const handleAction = () => {
      if (!token || !action) {
        setStatus('Lỗi');
        setMessage('URL không hợp lệ hoặc thiếu thông tin.');
        return;
      }

      if (['approve', 'reject', 'process_revocation'].includes(action)) {
        setShowReasonForm(true);

        let title = 'Xác nhận';
        if (action === 'approve') title = 'Xác nhận duyệt đơn';
        else if (action === 'reject') title = 'Xác nhận từ chối đơn';
        else if (action === 'process_revocation') title = 'Xử lý thu hồi đơn';

        setStatus(title);
        setMessage('Bạn có thể nhập lý do nếu muốn, hoặc để trống.');
      } else {
        setStatus('Lỗi');
        setMessage('Hành động không hợp lệ.');
      }
    };

    handleAction();
  }, [searchParams, action, token]);

  const processRequest = async (reasonInput = '') => {
    setStatus('Đang xử lý...');
    setMessage('Đang gửi yêu cầu lên hệ thống...');

    try {
      let apiUrl = '';
      let payload = { token, action, lyDoXuLy: reasonInput };

      if (action === 'approve' || action === 'reject') {
        apiUrl = `${API_GATEWAY_URL}/api/leaveRequests/process-approval`;
      } else if (action === 'process_revocation') {
        apiUrl = `${API_GATEWAY_URL}/api/leaveRequests/process-revocation`;
      } else {
        throw new Error('Hành động không được hỗ trợ.');
      }

      const response = await fetch(apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Lỗi xử lý.');

      setStatus('Thành công');
      setMessage(data.message || 'Hành động đã được xử lý thành công.');
    } catch (err) {
      setStatus('Lỗi');
      setMessage(err.message);
    }
  };

  const handleReasonSubmit = () => {
    setShowReasonForm(false);
    processRequest(reason.trim());
  };

  return (
    <div className="d-flex justify-content-center align-items-center vh-100 bg-light">
      <div className="card text-center p-4 shadow-sm" style={{ width: '450px' }}>
        <div className="card-body">
          <h4 className="card-title mb-3">{status}</h4>
          <p className="card-text text-muted">{message}</p>

          {showReasonForm ? (
            <div className="mt-3">
              <textarea
                className="form-control"
                rows="3"
                placeholder="Lý do xử lý (không bắt buộc)"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
              ></textarea>
              <button className="btn btn-primary mt-3 w-100" onClick={handleReasonSubmit}>
                Xác nhận
              </button>
            </div>
          ) : (
            <button className="btn btn-primary mt-3" onClick={() => navigate('/login')}>
              Về trang đăng nhập
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default ActionProcessingPage;
