const API_GATEWAY_URL = import.meta.env.VITE_API_GATEWAY_URL;

export async function fetchLeaveRequestStatuses() {
  
  const res = await fetch(`${API_GATEWAY_URL}/api/enums/leave-request-status`);
  if (!res.ok) throw new Error('Lỗi khi lấy trạng thái đơn');
  return await res.json();
}

export async function fetchLeaveTypes() {
  
  const res = await fetch(`${API_GATEWAY_URL}/api/enums/leave-types`);
  if (!res.ok) throw new Error('Lỗi khi lấy loại phép');
  return await res.json();
}

export async function fetchUserRoles() {
  // Sửa đường dẫn để đi qua Gateway
  const res = await fetch(`${API_GATEWAY_URL}/api/enums/user-roles`);
  if (!res.ok) throw new Error('Lỗi khi lấy quyền người dùng');
  return await res.json();
}
export async function fetchServiceTypes() {
  const res = await fetch(`${API_GATEWAY_URL}/api/enums/service-types`);
  if (!res.ok) throw new Error('Lỗi khi lấy loại dịch vụ');
  return await res.json();
}
