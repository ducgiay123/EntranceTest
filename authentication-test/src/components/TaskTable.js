import React, { useEffect, useState } from "react";
import {
  Table,
  Tag,
  Alert,
  Button,
  Space,
  Popconfirm,
  Input,
  Select,
} from "antd";
import axios from "axios";
import TaskUpdateModal from "./TaskUpdateModal";

const { Option } = Select;

const TaskTable = () => {
  const [tasks, setTasks] = useState([]);
  const [filteredTasks, setFilteredTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [updateModalOpen, setUpdateModalOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState(null);

  const [searchText, setSearchText] = useState("");
  const [statusFilter, setStatusFilter] = useState(null);

  const fetchTasks = async () => {
    try {
      const token = sessionStorage.getItem("token");
      if (!token) throw new Error("Authorization token not found.");

      const response = await axios.get("http://localhost:5000/api/v1/tasks", {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      });

      setTasks(response.data || []);
      setFilteredTasks(response.data || []);
    } catch (err) {
      setError(err.response?.data?.message || err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTasks();
  }, []);

  // Filter tasks based on searchText and statusFilter
  useEffect(() => {
    let data = [...tasks];

    // Filter by search text in title
    if (searchText) {
      const lowerSearch = searchText.toLowerCase();
      data = data.filter((task) =>
        task.title.toLowerCase().includes(lowerSearch)
      );
    }

    // Filter by status
    if (statusFilter) {
      data = data.filter((task) => task.status === statusFilter);
    }

    setFilteredTasks(data);
  }, [searchText, statusFilter, tasks]);

  const handleUpdateClick = (task) => {
    setSelectedTask(task);
    setUpdateModalOpen(true);
  };

  const handleDelete = async (taskId) => {
    try {
      setLoading(true);
      const token = sessionStorage.getItem("token");
      await axios.delete(`http://localhost:5000/api/v1/tasks/${taskId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchTasks();
    } catch (err) {
      setError(err.response?.data?.message || err.message);
    } finally {
      setLoading(false);
    }
  };

  const columns = [
    { title: "Title", dataIndex: "title", key: "title" },
    { title: "Description", dataIndex: "description", key: "description" },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status) => {
        let color = "default";
        if (status === "ToDo") color = "red";
        else if (status === "InProgress") color = "blue";
        else if (status === "Completed" || status === "Completed")
          color = "green";
        return <Tag color={color}>{status}</Tag>;
      },
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      sorter: (a, b) => new Date(a.dueDate) - new Date(b.dueDate),
      render: (date) => new Date(date).toLocaleString(),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space size="middle">
          <Button type="link" onClick={() => handleUpdateClick(record)}>
            Update
          </Button>
          <Popconfirm
            title="Are you sure to delete this task?"
            onConfirm={() => handleDelete(record._id || record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button type="link" danger>
              Delete
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      {error && (
        <Alert message="Error" description={error} type="error" showIcon />
      )}

      <Space style={{ marginBottom: 16 }}>
        <Input
          placeholder="Search Title"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          allowClear
          style={{ width: 200 }}
        />
        <Select
          placeholder="Filter by Status"
          value={statusFilter}
          onChange={(value) => setStatusFilter(value)}
          allowClear
          style={{ width: 200 }}
        >
          <Option value="ToDo">To Do</Option>
          <Option value="InProgress">In Progress</Option>
          <Option value="Completed">Completed</Option>
        </Select>
      </Space>

      <Table
        columns={columns}
        dataSource={filteredTasks}
        rowKey={(record) => record._id || record.id}
        loading={loading}
        pagination={{ pageSize: 5 }}
      />

      {selectedTask && (
        <TaskUpdateModal
          open={updateModalOpen}
          onCancel={() => setUpdateModalOpen(false)}
          onSuccess={() => {
            fetchTasks();
            setUpdateModalOpen(false);
          }}
          task={selectedTask}
        />
      )}
    </div>
  );
};

export default TaskTable;
