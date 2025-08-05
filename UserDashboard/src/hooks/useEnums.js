import { useState, useEffect } from 'react';
import {
  fetchLeaveRequestStatuses,
  fetchLeaveTypes,
  fetchUserRoles
} from '/src/enumsApi';

export function useEnums() {
  const [enumData, setEnumData] = useState({
    statusList: [],
    typeList: [],
    roleList: [],
    statuses: () => '...',
    types: () => '...',
    roles: () => '...',
  });

  useEffect(() => {
    const loadEnums = async () => {
      try {
        const [statusList, typeList, roleList] = await Promise.all([
          fetchLeaveRequestStatuses(),
          fetchLeaveTypes(),
          fetchUserRoles(),
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
          statuses: mapEnum(statusList),
          types: mapEnum(typeList),
          roles: mapEnum(roleList),
        });
      } catch (error) {
        console.error('Lỗi khi load enums:', error);
      }
    };

    loadEnums();
  }, []);

  return enumData;
}
