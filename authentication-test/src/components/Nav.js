// components/Nav.jsx
import React from "react";
import { Dropdown, Menu, Avatar } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../redux/slices/authSlice";

const Nav = ({ user }) => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await dispatch(logoutUser()).unwrap();
      navigate("/");
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };

  const menu = (
    <Menu>
      <Menu.Item key="logout" icon={<LogoutOutlined />} onClick={handleLogout}>
        Logout
      </Menu.Item>
    </Menu>
  );

  return (
    <header className="dashboard-header">
      <div />
      <div className="user-info">
        <div className="user-text">
          <span className="user-name">
            {user ? `${user.firstName} ${user.lastName}` : "Guest"}
          </span>
          <span className="user-status">
            {user ? user.email : "Not logged in"}
          </span>
        </div>
        <Dropdown overlay={menu} placement="bottomRight" trigger={["click"]}>
          <div className="user-avatar" style={{ cursor: "pointer" }}>
            <Avatar
              src={user?.avatar || "https://i.pravatar.cc/40"}
              size={40}
            />
            <span className="status-dot" />
          </div>
        </Dropdown>
      </div>
    </header>
  );
};

export default Nav;
