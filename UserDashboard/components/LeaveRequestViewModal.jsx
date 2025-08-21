import React from 'react';
import {
    Dialog,
    DialogContent,
    DialogOverlay,
    DialogPortal,
    DialogTitle,
    DialogDescription
} from '@radix-ui/react-dialog';
import '/src/LeaveRequestViewModal.css';
import { useEnums } from '/src/hooks/useEnums';
import { useTooltipContext } from '/src/TooltipContext';

function LeaveRequestViewModal({ isOpen, onClose, request }) {
    const { showTooltip, hideTooltip } = useTooltipContext();
    const { statuses, types } = useEnums();

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    const formatTime = (timeString) => {
        if (!timeString) return 'N/A';
        return timeString.substring(0, 5);
    };

    const formatDuration = (duration) => {
        if (duration === null || typeof duration === 'undefined') {
            return 'N/A';
        }
        return `${duration.toFixed(1)} ngày`;
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogPortal>
                <DialogOverlay className="modal-overlay" style={{ backgroundColor: 'rgba(0,0,0,0.5)', position: 'fixed', inset: 0, zIndex: 9998 }} />
                <DialogContent className="modal-content leave-request-modal" style={{ position: 'fixed', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', zIndex: 9999 }}>
                    <DialogTitle className="visually-hidden">Chi tiết đơn nghỉ phép</DialogTitle>
                    <DialogDescription className="visually-hidden">Thông tin chi tiết về đơn xin nghỉ phép của nhân viên.</DialogDescription>

                    {!request ? (
                        <div style={{ padding: '2rem' }}>Đang tải dữ liệu...</div>
                    ) : (
                        <>
                            <div className="modal-header-row">
                                <img src="/img/logo.png" alt="Logo" className="header-logo" />
                                <h3 className="header-title">ĐƠN XIN NGHỈ PHÉP</h3>
                                <div className="header-doc-info">
                                    <div>STR-HR-WI-003 F-0</div>
                                    <div>Rev.01: 15/01/2021</div>
                                </div>
                            </div>

                            <div className="modal-body">
                                <div className="info-section">
                                    <p><strong>Họ Tên:</strong> {request.hoTen}</p>
                                    <p><strong>Mã Nhân Viên:</strong> {request.maSoNhanVien}</p>
                                    <p><strong>Chức Vụ:</strong> {request.chucVu}</p>
                                    <p><strong>Bộ Phận:</strong> {request.boPhan}</p>
                                </div>

                                <h5 className="section-title">Nội dung nghỉ phép</h5>
                                <table className="leave-table">
                                    <thead>
                                        <tr>
                                            <th>Loại Phép</th>
                                            <th colSpan={2}>Ngày</th>
                                            <th colSpan={2}>Thời gian (giờ)</th>
                                            <th>Tổng cộng</th>
                                        </tr>
                                        <tr>
                                            <th></th>
                                            <th>Từ Ngày</th>
                                            <th>Đến Ngày</th>
                                            <th>Từ Giờ</th>
                                            <th>Đến Giờ</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td>{types(request.loaiPhep)}</td>
                                            <td>{formatDate(request.ngayTu)}</td>
                                            <td>{formatDate(request.ngayDen)}</td>
                                            <td>{formatTime(request.gioTu)}</td>
                                            <td>{formatTime(request.gioDen)}</td>
                                            <td>
                                                <strong>{formatDuration(request.durationInDays)}</strong>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>

                                <p><strong>Lý Do:</strong> {request.lyDo}</p>

                                {request.lyDoXuLy && (
                                    <div className="mt-3">
                                        <p><strong>Lý do xử lý:</strong> {request.lyDoXuLy}</p>
                                    </div>
                                )}

                                <div className="handover-section">
                                    <p><strong>Bàn giao công việc (nếu có):</strong></p>
                                    <p><em>{request.congViecBanGiao || 'Không có thông tin bàn giao công việc.'}</em></p>
                                </div>

                                <p><strong>Ngày Tạo:</strong> {formatDate(request.ngayTao)}</p>
                                <p><strong>Trạng Thái:</strong> <span className="badge-success">{statuses(request.trangThai)}</span></p>

                                {request.trangThai === 'Approved' && <div className="watermark-approved">ĐÃ DUYỆT</div>}

                                <div className="signatures">
                                    <div>
                                        <p><strong>Nhân viên xin nghỉ</strong><br />(Ký, ghi rõ họ tên)</p>
                                        <p className="signature-name">{request.hoTen || '........................'}</p>
                                    </div>
                                    <div>
                                        <p><strong>Quản lý Phòng ban</strong><br />(Ký, ghi rõ họ tên)</p>
                                        <p className="signature-name">{request.quanLyPhongBan || '........................'}</p>
                                    </div>
                                    <div>
                                        <p><strong>Ban Giám Đốc</strong><br />(Ký, ghi rõ họ tên)</p>
                                        <p className="signature-name">{request.banGiamDoc || '........................'}</p>
                                    </div>
                                </div>
                            </div>

                            <div className="modal-footer">
                                <button
                                    className="btn btn-secondary"
                                    onMouseEnter={(e) => showTooltip("Quay lại", e)}
                                    onMouseLeave={hideTooltip}
                                    onClick={onClose}
                                >
                                    ← Quay lại
                                </button>
                            </div>
                        </>
                    )}
                </DialogContent>
            </DialogPortal>
        </Dialog>
    );
}

export default LeaveRequestViewModal;
