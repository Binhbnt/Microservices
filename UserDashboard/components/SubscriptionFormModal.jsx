import React, { useState, useEffect } from 'react';

const SubscriptionFormModal = ({ isOpen, onClose, onSubmit, initialData, serviceTypes = [] }) => {
  const [formData, setFormData] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  useEffect(() => {
    if (isOpen) {
      if (initialData) {
        // The date from `useSubscriptionData` is already in 'YYYY-MM-DD' format, so we can use it directly.
        const formattedDate = initialData.expiryDate ? initialData.expiryDate.substring(0, 10) : '';
        
        const currentServiceType = serviceTypes.find(st => st.displayName === initialData.type);
        
        setFormData({
          name: initialData.name || '',
          type: currentServiceType?.key ?? (serviceTypes[0]?.key || 0),
          provider: initialData.provider || '',
          expiryDate: formattedDate,
          note: initialData.note || ''
        });
      } else {
        // Reset form for "add new"
        setFormData({
          name: '',
          type: serviceTypes[0]?.key || 0,
          provider: '',
          expiryDate: '',
          note: ''
        });
      }
    }
  }, [initialData, isOpen, serviceTypes]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    const finalValue = name === 'type' ? parseInt(value, 10) : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true); // <-- Bắt đầu xử lý, vô hiệu hóa nút
    try {
        await onSubmit(formData); // Gọi hàm submit được truyền từ props
    } catch (error) {
        // Có thể hiển thị lỗi ở đây nếu cần
        console.error("Lỗi khi submit form:", error);
    } finally {
        // Luôn luôn kích hoạt lại nút sau khi xử lý xong (dù thành công hay thất bại)
        setIsSubmitting(false); // <-- Kích hoạt lại nút
    }
};

  if (!isOpen) return null;

  return (
    <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
      <div className="modal-dialog modal-dialog-centered">
        <div className="modal-content">
          <form onSubmit={handleSubmit}>
            <div className="modal-header">
              <h5 className="modal-title">
                {initialData ? 'Chỉnh sửa Gói Dịch vụ' : 'Thêm Gói Dịch vụ mới'}
              </h5>
              <button type="button" className="btn-close" onClick={onClose}></button>
            </div>
            <div className="modal-body">
              <div className="mb-3">
                <label htmlFor="name" className="form-label">
                  Tên Dịch Vụ <span className="text-danger">*</span>
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name || ''}
                  onChange={handleChange}
                  className="form-control"
                  required
                />
              </div>

              <div className="row">
                <div className="col-md-6 mb-3">
                  <label htmlFor="type" className="form-label">Loại Dịch Vụ</label>
                  <select
                    id="type"
                    name="type"
                    value={formData.type || 0}
                    onChange={handleChange}
                    className="form-select"
                  >
                    {serviceTypes.map((type) => (
                      <option key={type.key} value={type.key}>{type.displayName}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-6 mb-3">
                  <label htmlFor="provider" className="form-label">Nhà Cung Cấp</label>
                  <input
                    type="text"
                    id="provider"
                    name="provider"
                    value={formData.provider || ''}
                    onChange={handleChange}
                    className="form-control"
                  />
                </div>
              </div>

              <div className="mb-3">
                <label htmlFor="expiryDate" className="form-label">
                  Ngày Hết Hạn <span className="text-danger">*</span>
                </label>
                <input
                  type="date"
                  id="expiryDate"
                  name="expiryDate"
                  value={formData.expiryDate || ''}
                  onChange={handleChange}
                  className="form-control"
                  required
                />
              </div>

              <div className="mb-3">
                <label htmlFor="note" className="form-label">Ghi Chú</label>
                <textarea
                  id="note"
                  name="note"
                  value={formData.note || ''}
                  onChange={handleChange}
                  rows="3"
                  className="form-control"
                ></textarea>
              </div>
            </div>
            <div className="modal-footer">
               <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSubmitting}>Hủy</button>
              <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                        {isSubmitting ? (
                            <>
                                <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                <span className="ms-1">Đang lưu...</span>
                            </>
                        ) : (
                            initialData ? 'Lưu thay đổi' : 'Thêm mới'
                        )}
                    </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default SubscriptionFormModal;