import React, { useEffect, useState } from "react";
import "./dashboardPage.css";
import dashboarImage from "../asserts/dashboard.png";
import TaskTable from "../components/TaskTable";
import TaskModal from "../components/TaskModal";
import Nav from "../components/Nav"; // ⬅️ Imported new Nav component
import { Button, Form, message } from "antd";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { logoutUser, setUserFromSession } from "../redux/slices/authSlice";

const DashboardPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user, token, loading, error } = useSelector((state) => state.auth);

  const [isModalVisible, setIsModalVisible] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    if (!user) {
      const storedUser = sessionStorage.getItem("user");

      if (storedUser) {
        try {
          const parsedUser = JSON.parse(storedUser);

          if (parsedUser && parsedUser.firstName && parsedUser.email) {
            dispatch(setUserFromSession(parsedUser));
          } else {
            console.warn("Parsed user is invalid:", parsedUser);
            sessionStorage.removeItem("user");
          }
        } catch (e) {
          console.error("Failed to parse stored user:", e);
          sessionStorage.removeItem("user");
        }
      }
    }
  }, [user, dispatch]);

  const handleLogout = async () => {
    try {
      await dispatch(logoutUser()).unwrap();
      navigate("/");
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };

  const showModal = () => setIsModalVisible(true);
  const handleCancel = () => {
    form.resetFields();
    setIsModalVisible(false);
  };

  const handleCreateTask = (values) => {
    console.log("Form Values:", values);
    message.success("Task created successfully!");
    handleCancel();
    form.resetFields();
    setIsModalVisible(false);

    // TODO: Dispatch action or API call
  };

  if (!user && !token && !loading) {
    navigate("/login");
    return null;
  }

  if (loading) return <div>Loading user...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="dashboard-container">
      {/* Header */}
      <Nav user={user} onLogout={handleLogout} />

      {/* Main Content */}
      <main className="dashboard-content">
        <h2>Welcome to Demo App</h2>
        {user && (
          <p>
            Hello, {user.firstName}! Your email is {user.email}.
          </p>
        )}

        <Button type="primary" onClick={showModal} style={{ marginBottom: 16 }}>
          Create Task
        </Button>
        <div style={{ padding: 24 }}>
          <h2>My Tasks</h2>
          <TaskTable />
        </div>
      </main>

      {/* Task Creation Modal */}
      <TaskModal
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        onSubmit={handleCreateTask}
      />

      {/* Footer */}
      <footer className="dashboard-footer">COPYRIGHT © 2020</footer>
    </div>
  );
};

export default DashboardPage;
