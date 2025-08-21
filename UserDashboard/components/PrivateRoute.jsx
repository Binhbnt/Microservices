// src/components/PrivateRoute.jsx
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

function PrivateRoute({ children, currentUser }) {
  const navigate = useNavigate(); // useNavigate ở đây là đúng

  useEffect(() => {
      const token = localStorage.getItem('jwtToken');
      if (!token && !currentUser) { // Nếu không có token VÀ currentUser là null, điều hướng đến trang login
          navigate('/login');
      }
  }, [currentUser, navigate]); // Dependencies: currentUser và navigate

  return currentUser ? children : null;
}

export default PrivateRoute;