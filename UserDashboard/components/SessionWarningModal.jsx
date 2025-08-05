// src/components/SessionWarningModal.jsx
import React, { useState } from 'react';
import { useTooltipContext } from '/src/TooltipContext';

function SessionWarningModal({ show, onClose, onRenewSession, expiresInMinutes }) {
    const { showTooltip, hideTooltip } = useTooltipContext();
    const [loading, setLoading] = useState(false);
    const tooltipText = loading ? "Đang gia hạn..." : "Gia hạn";

    const handleRenewClick = async () => {
        setLoading(true);
        await onRenewSession();
    };

    if (!show) return null;

    const displayMinutes = Math.ceil(expiresInMinutes);

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Cảnh báo phiên làm việc</h2>
                </div>
                <div className="modal-body">
                    {/* <p>Phiên làm việc của bạn sẽ hết hạn trong <strong>{displayMinutes > 0 ? displayMinutes : 1} phút</strong>{' nữa.'}</p> */}
                    <p>
                        Phiên làm việc của bạn sẽ hết hạn trong
                        <span> <strong>{displayMinutes > 0 ? displayMinutes : 1} phút</strong>{' nữa.'}</span>
                    </p>
                    <p>Bạn có muốn gia hạn phiên làm việc không?</p>
                </div>
                <div className="modal-footer">
                    <button
                        type="button"
                        className="btn btn-primary"
                        onClick={handleRenewClick}
                        disabled={loading}
                        onMouseEnter={(e) => showTooltip(tooltipText, e)}
                        onMouseLeave={hideTooltip}
                    >
                        {tooltipText}
                    </button>
                    <button
                        type="button"
                        className="btn btn-secondary"
                        disabled={loading}
                        onClick={onClose}
                    >
                        Không (sẽ đăng xuất)
                    </button>
                </div>
            </div>
        </div>
    );
}

export default SessionWarningModal;