import React, { useState, useEffect, useRef } from 'react';
import { useTooltipContext } from '/src/TooltipContext';
function ReLoginModal({ show, userToReLogin, onReLoginAttempt, onLogout }) {
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const passwordInputRef = useRef(null);
    const { showTooltip, hideTooltip } = useTooltipContext();
    useEffect(() => {
        if (show && passwordInputRef.current) {
            setTimeout(() => passwordInputRef.current.focus(), 100);
        }
    }, [show]);

    useEffect(() => {
        if (!show) {
            setPassword('');
            setError('');
            setLoading(false);
        }
    }, [show]);

    if (!show) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!password) {
            setError('Vui lòng nhập mật khẩu.');
            return;
        }
        setLoading(true);
        setError('');

        const success = await onReLoginAttempt(password);

        setLoading(false);
        if (!success) {
            setError('Mật khẩu không đúng. Vui lòng thử lại.');
            setPassword('');
            if (passwordInputRef.current) {
                passwordInputRef.current.focus();
            }
        }
    };

    const handleSwitchAccount = (e) => {
        e.preventDefault();
        onLogout('Chuyển sang tài khoản khác.');
    };

    return (
        <div className="modal-backdrop">
            <div className="modal-content" style={{ maxWidth: '400px' }}>
                <form onSubmit={handleSubmit}>
                    <div className="modal-header" style={{ borderBottom: 'none', justifyContent: 'center' }}>
                        <h4 className="modal-title fw-bold">Phiên làm việc đã kết thúc</h4>
                    </div>
                    <div className="modal-body">

                        {/* === USER INPUT WITH ICON === */}
                        <div className="mb-3">
                            <div className="input-group">
                                <span className="input-group-text">
                                    <i className="fa-solid fa-user"></i>
                                </span>
                                <input
                                    type="text"
                                    className="form-control"
                                    readOnly
                                    disabled
                                    value={`${userToReLogin?.maSoNhanVien || ''} - ${userToReLogin?.hoTen || ''}`}
                                    style={{ backgroundColor: '#e9ecef', cursor: 'default' }}
                                />
                            </div>
                        </div>

                        {/* === PASSWORD INPUT WITH ICON === */}
                        <div className="mb-3">
                            <div className="input-group">
                                <span className="input-group-text">
                                    <i className="fa-solid fa-lock"></i>
                                </span>
                                <input
                                    ref={passwordInputRef}
                                    type="password"
                                    className={`form-control ${error ? 'is-invalid' : ''}`}
                                    placeholder="Nhập mật khẩu để tiếp tục"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required
                                />
                            </div>
                        </div>

                        {error && <div className="text-danger text-center small mt-2">{error}</div>}
                    </div>
                    <div className="modal-footer" style={{ borderTop: 'none', flexDirection: 'column', alignItems: 'stretch' }}>
                        <button type="submit" className="btn btn-primary btn-lg w-100" disabled={loading}>
                            {loading ? (
                                <>
                                    <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                    <span className="ms-2">Đang xử lý...</span>
                                </>
                            ) : 'Đăng nhập'}
                        </button>
                        <div className="d-flex justify-content-between mt-3">
                            <a href="#" onClick={handleSwitchAccount} className="text-decoration-none text-secondary small"
                                onMouseEnter={(e) => showTooltip("Đăng nhập bằng tài khoản khác", e)}
                                onMouseLeave={hideTooltip}>Đăng nhập tài khoản khác</a>
                            <a href="#" onClick={() => window.location.reload()} className="text-decoration-none text-secondary small"
                                onMouseEnter={(e) => showTooltip("Tải lại trang", e)}
                                onMouseLeave={hideTooltip}   >Tải lại trang</a>
                        </div>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default ReLoginModal;