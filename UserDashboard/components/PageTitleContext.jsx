// /components/PageTitleContext.jsx

import { createContext } from 'react';

// Tạo một Context mới, không cần giá trị mặc định phức tạp
export const PageTitleContext = createContext({
  setPageTitle: () => {},
  setPageIcon: () => {},
});