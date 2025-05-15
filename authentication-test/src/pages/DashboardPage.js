import React from "react";
import "./dashboardPage.css";
import dashboarImage from "../asserts/dashboard.png";
import { Dropdown, Menu, Avatar } from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useDispatch } from "react-redux";
import { logoutUser } from "../redux/slices/authSlice";
import { useNavigate } from "react-router-dom";

const DashboardPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await dispatch(logoutUser());
    navigate("/");
  };

  const menu = (
    <Menu>
      <Menu.Item key="logout" icon={<LogoutOutlined />} onClick={handleLogout}>
        Logout
      </Menu.Item>
    </Menu>
  );

  return (
    <div className="dashboard-container">
      {/* Top Header */}
      <header className="dashboard-header">
        <div />
        <div className="user-info">
          <div className="user-text">
            <span className="user-name">John Doe</span>
            <span className="user-status">Available</span>
          </div>

          {/* Ant Design Dropdown */}
          <Dropdown overlay={menu} placement="bottomRight" trigger={["click"]}>
            <div className="user-avatar" style={{ cursor: "pointer" }}>
              <Avatar src="https://i.pravatar.cc/40" size={40} />
              <span className="status-dot" />
            </div>
          </Dropdown>
        </div>
      </header>

      {/* Center Section */}
      <main className="dashboard-content">
        <h2>Welcome to Demo App</h2>
        <img src={dashboarImage} alt="Demo App" className="dashboard-image" />
      </main>

      {/* Footer */}
      <footer className="dashboard-footer">COPYRIGHT © 2020</footer>
    </div>
  );
};

export default DashboardPage;
