// components/PrivateRoute.js
import React from "react";
import { Navigate, useLocation } from "react-router-dom";

const PrivateRoute = ({ children, roleRequired }) => {
  const token = sessionStorage.getItem("token");
  const user = JSON.parse(sessionStorage.getItem("user"));

  const location = useLocation();

  if (!token || !user) {
    // Not authenticated
    return <Navigate to="/" state={{ from: location }} replace />;
  }

  if (roleRequired !== undefined && user.role !== roleRequired) {
    // Role not authorized
    return <Navigate to="/" replace />;
  }

  return children;
};

export default PrivateRoute;
