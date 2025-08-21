import React, { useState, useEffect } from 'react';
import { useEnums } from '/src/hooks/useEnums';

function UserFormModal({ isOpen, onClose, onSubmit, initialData }) {
  const { roleList } = useEnums();

  const [formData, setFormData] = useState({
    maSoNhanVien: '',
    hoTen: '',
    email: '',
    matKhau: '',
    chucVu: '',
    boPhan: '',
    role: 'User',
    avatarUrl: ''
  });

  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  useEffect(() => {
    if (initialData) {
      setFormData({
        id: initialData.id,
        maSoNhanVien: initialData.maSoNhanVien || '',
        hoTen: initialData.hoTen || '',
        email: initialData.email || '',
        matKhau: '',
        chucVu: initialData.chucVu || '',
        boPhan: initialData.boPhan || '',
        role: initialData.role || 'User',
        avatarUrl: initialData.avatarUrl || ''
      });
    } else {
      setFormData({
        maSoNhanVien: '',
        hoTen: '',
        email: '',
        matKhau: '',
        chucVu: '',
        boPhan: '',
        role: 'User',
        avatarUrl: ''
      });
    }
  }, [initialData]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    setErrors(prev => ({ ...prev, [name]: '' }));
  };

  const validateForm = () => {
    const newErrors = {};
    if (!formData.maSoNhanVien.trim()) newErrors.maSoNhanVien = 'Mã số NV không được để trống';
    if (!initialData && !formData.matKhau.trim()) newErrors.matKhau = 'Mật khẩu là bắt buộc';
    else if (!initialData && formData.matKhau.trim().length < 6) newErrors.matKhau = 'Mật khẩu phải có ít nhất 6 ký tự';
    if (!formData.hoTen.trim()) newErrors.hoTen = 'Họ tên không được để trống';
    if (!formData.email.trim()) newErrors.email = 'Email không được để trống';
    else if (!/\S+@\S+\.\S+/.test(formData.email)) newErrors.email = 'Email không hợp lệ';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    const dataToSend = { ...formData };
    if (initialData && !dataToSend.matKhau.trim()) {
      delete dataToSend.matKhau;
    }

    await onSubmit(dataToSend);
  };

  if (!isOpen) return null;

  const modalTitle = initialData ? 'Chỉnh Sửa Thông Tin Người Dùng' : 'Thêm Người Dùng Mới';
  const submitButtonText = initialData ? 'Cập Nhật' : 'Thêm';
  const isPasswordFieldRequired = !initialData;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h2>{modalTitle}</h2>
          <button className="close-button" onClick={onClose}>&times;</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="maSoNhanVien">Mã số NV:</label>
            <input type="text" name="maSoNhanVien" value={formData.maSoNhanVien} onChange={handleChange} required />
            {errors.maSoNhanVien && <p className="error-text">{errors.maSoNhanVien}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="hoTen">Họ tên:</label>
            <input type="text" name="hoTen" value={formData.hoTen} onChange={handleChange} required />
            {errors.hoTen && <p className="error-text">{errors.hoTen}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input type="email" name="email" value={formData.email} onChange={handleChange} required />
            {errors.email && <p className="error-text">{errors.email}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="matKhau">Mật khẩu:</label>
            <input
              type="password"
              name="matKhau"
              value={formData.matKhau}
              onChange={handleChange}
              required={isPasswordFieldRequired}
              placeholder={initialData ? 'Để trống nếu không muốn đổi mật khẩu' : ''}
            />
            {errors.matKhau && <p className="error-text">{errors.matKhau}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="chucVu">Chức vụ:</label>
            <input type="text" name="chucVu" value={formData.chucVu} onChange={handleChange} />
          </div>
          <div className="form-group">
            <label htmlFor="boPhan">Bộ phận:</label>
            <input type="text" name="boPhan" value={formData.boPhan} onChange={handleChange} />
          </div>
          <div className="form-group">
            <label htmlFor="role">Vai trò:</label>
            <select name="role" value={formData.role} onChange={handleChange}>
              {roleList?.length > 0
                ? roleList.map(({ key, displayName }) => (
                  <option key={key} value={key}>{displayName}</option>
                ))
                : <option disabled>Đang tải...</option>}
            </select>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSubmitting}>Hủy</button>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                  <span className="ms-1">Đang xử lý...</span>
                </>
              ) : (
                initialData ? 'Cập Nhật' : 'Thêm mới'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default UserFormModal;
