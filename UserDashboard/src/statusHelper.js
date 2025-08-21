// utils/statusHelper.js

export const translateStatus = (statusInEnglish) => {
  switch (statusInEnglish) {
    case 'Healthy':
      return 'Hoạt động';
    case 'Error':
      return 'Lỗi';
    case 'Unreachable':
      return 'Không thể kết nối';
    default:
      return statusInEnglish; // Trả về giá trị gốc nếu không có bản dịch
  }
};