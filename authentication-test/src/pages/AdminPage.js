import React, { useEffect, useState } from "react";
import {
  Table,
  Card,
  Typography,
  Spin,
  message,
  Button,
  Modal,
  Form,
  Input,
  Space,
} from "antd";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Nav from "../components/Nav"; // ✅ Add Nav to Admin Page

const { Title } = Typography;

const AdminPage = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [form] = Form.useForm();
  const navigate = useNavigate();

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const token = sessionStorage.getItem("token");
      const response = await axios.get(
        "http://localhost:5000/api/v1/admin/users",
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      setUsers(response.data);
    } catch (error) {
      message.error("Failed to load users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const showUpdateModal = (user) => {
    setEditingUser(user);
    form.setFieldsValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
    });
    setIsModalOpen(true);
  };

  const handleUpdate = async () => {
    try {
      const values = await form.validateFields();
      const token = sessionStorage.getItem("token");
      await axios.put(
        "http://localhost:5000/api/v1/admin/users/update",
        values,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      message.success("User updated successfully");
      setIsModalOpen(false);
      fetchUsers();
    } catch (error) {
      message.error("Update failed");
    }
  };

  const handleDelete = async (email) => {
    try {
      const token = sessionStorage.getItem("token");
      await axios.delete(`http://localhost:5000/api/v1/admin/users/${email}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      message.success("User deleted successfully");
      fetchUsers();
    } catch (error) {
      message.error("Delete failed");
    }
  };

  const handleRowClick = (record) => {
    navigate(`/admin/${record.id}`);
  };

  const columns = [
    {
      title: "ID",
      dataIndex: "id",
      key: "id",
    },
    {
      title: "First Name",
      dataIndex: "firstName",
      key: "firstName",
    },
    {
      title: "Last Name",
      dataIndex: "lastName",
      key: "lastName",
    },
    {
      title: "Email",
      dataIndex: "email",
      key: "email",
    },
    {
      title: "Created At",
      dataIndex: "createdAt",
      key: "createdAt",
      render: (text) => new Date(text).toLocaleString(),
    },
    {
      title: "Updated At",
      dataIndex: "updatedAt",
      key: "updatedAt",
      render: (text) => new Date(text).toLocaleString(),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Button type="primary" onClick={() => showUpdateModal(record)}>
            Update
          </Button>
          <Button danger onClick={() => handleDelete(record.email)}>
            Delete
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Nav />
      <div className="p-6">
        <Card className="shadow rounded-2xl">
          <Title level={3}>User Dashboard</Title>
          {loading ? (
            <div className="text-center my-6">
              <Spin size="large" />
            </div>
          ) : (
            <Table
              dataSource={users}
              columns={columns}
              rowKey="id"
              pagination={{ pageSize: 5 }}
              onRow={(record) => ({ onClick: () => handleRowClick(record) })}
            />
          )}
        </Card>

        <Modal
          title="Update User"
          open={isModalOpen}
          onCancel={() => setIsModalOpen(false)}
          onOk={handleUpdate}
          okText="Update"
        >
          <Form form={form} layout="vertical">
            <Form.Item name="email" label="Email">
              <Input disabled />
            </Form.Item>
            <Form.Item
              name="firstName"
              label="First Name"
              rules={[{ required: true }]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="lastName"
              label="Last Name"
              rules={[{ required: true }]}
            >
              <Input />
            </Form.Item>
          </Form>
        </Modal>
      </div>
    </div>
  );
};

export default AdminPage;
