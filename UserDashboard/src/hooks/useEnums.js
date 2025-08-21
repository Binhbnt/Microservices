import { useState, useEffect } from 'react';
import {
  fetchLeaveRequestStatuses,
  fetchLeaveTypes,
  fetchUserRoles,
  fetchServiceTypes
} from '/src/enumsApi';

export function useEnums() {
  const [enumData, setEnumData] = useState({
    statusList: [],
    typeList: [],
    roleList: [],
    serviceTypeList: [],
    statuses: () => '...',
    types: () => '...',
    roles: () => '...',
    serviceTypes: () => '...',
  });

   useEffect(() => {
    const loadEnums = async () => {
      try {
        // SỬA Ở ĐÂY: Đổi tên 'serviceTypeListData' thành 'serviceTypeList' cho nhất quán
        const [statusList, typeList, roleList, serviceTypeList] = await Promise.all([
          fetchLeaveRequestStatuses(),
          fetchLeaveTypes(),
          fetchUserRoles(),
          fetchServiceTypes(),
        ]);

        const mapEnum = (list) => {
          const map = {};
          list.forEach(item => {
            map[item.key] = item.displayName;
          });
          return (key) => map[key] || key;
        };

        setEnumData({
          statusList,
          typeList,
          roleList,
          serviceTypeList, // Bây giờ biến này đã tồn tại và đúng
          statuses: mapEnum(statusList),
          types: mapEnum(typeList),
          roles: mapEnum(roleList),
          serviceTypes: mapEnum(serviceTypeList),
        });
      } catch (error) {
        console.error('Lỗi khi load enums:', error);
      }
    };

    loadEnums();
  }, []);

  return enumData;
}
