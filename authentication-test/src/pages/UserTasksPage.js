import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom"; // ⬅️ Updated
import {
  Card,
  Table,
  Spin,
  Typography,
  message,
  Button,
  Modal,
  Form,
  Input,
  Select,
  DatePicker,
} from "antd";
import axios from "axios";
import dayjs from "dayjs";
import Nav from "../components/Nav"; // ✅ Import Nav

const { Title } = Typography;

const UserTasksPage = () => {
  const { userId } = useParams();
  const navigate = useNavigate(); // ⬅️ Added for navigation
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [currentTask, setCurrentTask] = useState(null);
  const [form] = Form.useForm();

  // 🔐 Get logged-in user from sessionStorage
  const [user, setUser] = useState(null);
  useEffect(() => {
    const stored = sessionStorage.getItem("user");
    if (stored) setUser(JSON.parse(stored));
  }, []);

  const fetchUserTasks = async () => {
    setLoading(true);
    try {
      const token = sessionStorage.getItem("token");
      const response = await axios.get(
        `http://localhost:5000/api/v1/admin/tasks/user/${userId}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      setTasks(response.data);
    } catch (error) {
      console.error("Error fetching tasks:", error);
      message.error("Failed to load user tasks");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUserTasks();
  }, [userId]);

  const showUpdateModal = (task) => {
    setCurrentTask(task);
    form.setFieldsValue({
      ...task,
      dueDate: dayjs(task.dueDate),
    });
    setIsModalVisible(true);
  };

  const handleUpdate = async () => {
    try {
      const values = await form.validateFields();
      const token = sessionStorage.getItem("token");
      await axios.put(
        `http://localhost:5000/api/v1/admin/tasks/${currentTask.id}/user/${userId}`,
        values,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      message.success("Task updated successfully");
      setIsModalVisible(false);
      fetchUserTasks();
    } catch (error) {
      message.error("Update failed");
    }
  };

  const handleDelete = async (taskId) => {
    try {
      const token = sessionStorage.getItem("token");
      await axios.delete(
        `http://localhost:5000/api/v1/admin/tasks/${taskId}/user/${userId}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      message.success("Task deleted successfully");
      fetchUserTasks();
    } catch (error) {
      message.error("Delete failed");
    }
  };

  const columns = [
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
    },
    {
      title: "Description",
      dataIndex: "description",
      key: "description",
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (date) => dayjs(date).format("YYYY-MM-DD"),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <div style={{ display: "flex", gap: "8px" }}>
          <Button type="primary" onClick={() => showUpdateModal(record)}>
            Update
          </Button>
          <Button danger onClick={() => handleDelete(record.id)}>
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="p-6">
      {/* ✅ Nav */}
      <Nav user={user} />

      <Card className="shadow rounded-2xl" style={{ marginTop: 16 }}>
        {/* ✅ Back Button */}
        <Button onClick={() => navigate("/admin")} style={{ marginBottom: 16 }}>
          ← Back to Admin Page
        </Button>

        <Title level={3}>User Tasks</Title>
        {loading ? (
          <Spin size="large" />
        ) : (
          <Table
            dataSource={tasks}
            columns={columns}
            rowKey="id"
            pagination={{ pageSize: 5 }}
          />
        )}
      </Card>

      {/* Update Modal */}
      <Modal
        title="Update Task"
        open={isModalVisible}
        onCancel={() => setIsModalVisible(false)}
        onOk={handleUpdate}
        okText="Update"
      >
        <Form layout="vertical" form={form}>
          <Form.Item
            name="title"
            label="Title"
            rules={[{ required: true, message: "Please enter a title" }]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: "Please enter a description" }]}
          >
            <Input />
          </Form.Item>
          <Form.Item name="status" label="Status" rules={[{ required: true }]}>
            <Select>
              <Select.Option value="ToDo">ToDo</Select.Option>
              <Select.Option value="InProgress">InProgress</Select.Option>
              <Select.Option value="Completed">Completed</Select.Option>
            </Select>
          </Form.Item>
          <Form.Item
            name="dueDate"
            label="Due Date"
            rules={[{ required: true, message: "Please select a due date" }]}
          >
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default UserTasksPage;
