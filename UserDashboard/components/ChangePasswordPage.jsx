import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTooltipContext } from '/src/TooltipContext';
function ChangePasswordPage({ API_GATEWAY_URL, getAuthHeaders, onLogout, onToastMessage }) {
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { showTooltip, hideTooltip } = useTooltipContext();
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (newPassword !== confirmPassword) {
            setError('Mật khẩu mới và mật khẩu xác nhận không khớp.');
            return;
        }
        if (newPassword.length < 6) {
            setError('Mật khẩu mới phải có ít nhất 6 ký tự.');
            return;
        }

        setLoading(true);

        try {
            const response = await fetch(`${API_GATEWAY_URL}/api/Users/change-password`, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify({ oldPassword, newPassword })
            });

            if (response.status === 401) {
                onLogout('Phiên làm việc đã hết hạn.');
                return;
            }

            if (!response.ok) {
                const responseData = await response.json();
                throw new Error(responseData.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.');
            }

            onToastMessage({ type: 'success', message: 'Đổi mật khẩu thành công! Vui lòng đăng nhập lại.' });

            // Đợi 2 giây để user đọc thông báo rồi mới logout
            setTimeout(() => {
                onLogout('Đăng nhập lại với mật khẩu mới.');
            }, 2000);

        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-6">
                    <div className="card shadow-sm">
                        <div className="card-header">
                            <h4 className="card-title mb-0">Đổi mật khẩu</h4>
                        </div>
                        <div className="card-body">
                            <form onSubmit={handleSubmit}>
                                <div className="mb-3">
                                    <label className="form-label fw-bold">Mật khẩu cũ</label>
                                    <input
                                        type="password"
                                        className="form-control"
                                        value={oldPassword}
                                        onChange={(e) => setOldPassword(e.target.value)}
                                        required
                                    />
                                </div>
                                <div className="mb-3">
                                    <label className="form-label fw-bold">Mật khẩu mới</label>
                                    <input
                                        type="password"
                                        className="form-control"
                                        value={newPassword}
                                        onChange={(e) => setNewPassword(e.target.value)}
                                        required
                                    />
                                </div>
                                <div className="mb-3">
                                    <label className="form-label fw-bold">Xác nhận mật khẩu mới</label>
                                    <input
                                        type="password"
                                        className="form-control"
                                        value={confirmPassword}
                                        onChange={(e) => setConfirmPassword(e.target.value)}
                                        required
                                    />
                                </div>
                                {error && <div className="alert alert-danger py-2">{error}</div>}
                                <div className="d-flex justify-content-end">
                                    <button type="button" className="btn btn-secondary me-2"
                                        onMouseEnter={(e) => showTooltip("Hủy", e)}
                                        onMouseLeave={hideTooltip}
                                        onClick={() => navigate(-1)}>Hủy</button>
                                    <button type="submit" className="btn btn-primary"
                                        onMouseEnter={(e) => showTooltip(loading ? 'Đang lưu...' : 'Lưu thay đổi', e)}
                                        onMouseLeave={hideTooltip}
                                        disabled={loading}>
                                        {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default ChangePasswordPage;