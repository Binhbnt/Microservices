import { createContext, useContext } from 'react';

// Tạo và export Context
export const TooltipContext = createContext(null);

// Tạo và export hook để dễ sử dụng
export const useTooltipContext = () => {
  const context = useContext(TooltipContext);
  if (!context) {
    throw new Error('useTooltipContext must be used within a TooltipProvider');
  }
  return context;
};