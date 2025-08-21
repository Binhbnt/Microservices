// src/config/menuItems.js

export const menuItems = [
  {
    title: 'Trang chủ',
    icon: 'fa-solid fa-house',
    path: '/'
  },
  {
    title: 'Nhân viên',
    icon: 'fa-solid fa-circle-user',
    path: '/employees',
    roles: ['Admin', 'SuperUser']
  },
  {
    title: 'Đơn xin phép',
    icon: 'fas fa-file-alt',
    path: '/leave-requests'
  },
  {
    title: 'Quản lý gói dịch vụ',
    icon: 'fa-solid fa-list-check',
    path: '/subscription',
    roles: ['Admin', 'SuperUser']
  },
  {
  title: 'Tình trạng hệ thống',
  path: '/system-status',
  icon: 'fa-solid fa-server', // hoặc dùng icon phù hợp
  roles: ['Admin'] // hoặc thêm 'SuperUser' nếu cần
  }
];