import React, { useState, useEffect } from 'react';
import Modal from 'react-modal';

const customStyles = {
    overlay: {
        zIndex: 9999, // 👈 đảm bảo nổi bật hơn mọi phần khác
        backgroundColor: 'rgba(0, 0, 0, 0.5)'
    },
    content: {
        top: '50%',
        left: '50%',
        right: 'auto',
        bottom: 'auto',
        marginRight: '-50%',
        transform: 'translate(-50%, -50%)',
        width: '600px',
        maxHeight: '90vh',
        overflowY: 'auto',
        borderRadius: '10px',
        padding: '20px',
        zIndex: 10000 // 👈 thêm nếu muốn chắc chắn
    }
};

Modal.setAppElement('#root');

function LeaveRequestFormModal({ isOpen, onClose, onSubmit }) {
    const [formData, setFormData] = useState({
        loaiPhep: 'PhepNam',
        lyDo: '',
        ngayTu: '',
        ngayDen: '',
        gioTu: '', // <-- Thêm trường mới
        gioDen: '', // <-- Thêm trường mới
        congViecBanGiao: ''
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const handleChange = (e) => {
        const { name, value } = e.target;

        // Tạo một bản sao của formData để cập nhật
        let newFormData = { ...formData, [name]: value };

        // === LOGIC MỚI: TỰ ĐỘNG ĐIỀN "ĐẾN GIỜ" ===
        if (name === 'gioTu') {
            switch (value) {
                case '08:00':
                    newFormData.gioDen = '17:00';
                    break;
                case '08:30':
                    newFormData.gioDen = '17:30';
                    break;
                case '09:00':
                    newFormData.gioDen = '18:00';
                    break;
                default:
                    // Nếu chọn giờ khác, có thể xóa trống ô "Đến giờ"
                    newFormData.gioDen = '';
                    break;
            }
        }
        // Cập nhật state với dữ liệu mới
        setFormData(newFormData);
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        // Kết hợp ngày và giờ trước khi gửi đi nếu cần
        const submissionData = {
            ...formData,
            ngayTu: `${formData.ngayTu}T${formData.gioTu || '00:00'}:00`,
            ngayDen: `${formData.ngayDen}T${formData.gioDen || '00:00'}:00`
        };
        onSubmit(submissionData);
    };

    return (
        <Modal
            isOpen={isOpen}
            onRequestClose={onClose}
            style={customStyles}
            contentLabel="Form Đơn Xin Phép"
        >
            <h4 className="mb-3">Tạo Đơn Xin Nghỉ Phép</h4>
            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label htmlFor="loaiPhep" className="form-label">Loại phép</label>
                    <select
                        id="loaiPhep"
                        name="loaiPhep"
                        className="form-select"
                        value={formData.loaiPhep}
                        onChange={handleChange}
                    >
                        <option value="PhepNam">Phép Năm</option>
                        <option value="PhepBenh">Phép Bệnh</option>
                        <option value="NghiKhongLuong">Nghỉ Không Lương</option>
                        <option value="NghiCheDo">Nghỉ Chế Độ</option>
                    </select>
                </div>

                <div className="row mb-3">
                    <div className="col">
                        <label htmlFor="ngayTu" className="form-label">Từ ngày</label>
                        <input type="date" id="ngayTu" name="ngayTu" className="form-control" value={formData.ngayTu} onChange={handleChange} required />
                    </div>
                    <div className="col">
                        <label htmlFor="ngayDen" className="form-label">Đến ngày</label>
                        <input type="date" id="ngayDen" name="ngayDen" className="form-control" value={formData.ngayDen} onChange={handleChange} required />
                    </div>
                </div>

                {/* THÊM KHỐI NÀY VÀO */}
                <div className="row mb-3">
                    <div className="col">
                        <label htmlFor="gioTu" className="form-label">Từ giờ</label>
                        {/* Thay input text bằng select để giới hạn lựa chọn */}
                        <select
                            id="gioTu"
                            name="gioTu"
                            className="form-select"
                            value={formData.gioTu}
                            onChange={handleChange}
                        >
                            <option value="">-- Chọn giờ --</option>
                            <option value="08:00">08:00</option>
                            <option value="08:30">08:30</option>
                            <option value="09:00">09:00</option>
                        </select>
                    </div>
                    <div className="col">
                        <label htmlFor="gioDen" className="form-label">Đến giờ</label>
                        <input
                            type="text"
                            id="gioDen"
                            name="gioDen"
                            className="form-control"
                            value={formData.gioDen}
                            readOnly // Người dùng không thể sửa
                            title="Giờ kết thúc được tính tự động"
                        />
                    </div>
                </div>

                <div className="mb-3">
                    <label htmlFor="lyDo" className="form-label">Lý do</label>
                    <textarea id="lyDo" name="lyDo" className="form-control" rows="3" value={formData.lyDo} onChange={handleChange} required></textarea>
                </div>

                <div className="mb-3">
                    <label htmlFor="congViecBanGiao" className="form-label">Công việc bàn giao (nếu có)</label>
                    <textarea id="congViecBanGiao" name="congViecBanGiao" className="form-control" rows="3" value={formData.congViecBanGiao} onChange={handleChange}></textarea>
                </div>

                <div className="d-flex justify-content-end gap-2">
                    <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSubmitting}>Hủy</button>
                    <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                        {isSubmitting ? (
                            <>
                                <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                <span className="ms-1">Đang gửi...</span>
                            </>
                        ) : (
                            'Gửi Đơn'
                        )}
                    </button>
                </div>
            </form>
        </Modal>
    );
}

export default LeaveRequestFormModal;