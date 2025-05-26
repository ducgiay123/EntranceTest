import React from "react";
import { Modal, Form, Input, Select, DatePicker, Button } from "antd";
import axios from "axios";

const TaskModal = ({ open, onCancel, onSuccess }) => {
  const [form] = Form.useForm();

  const handleFinish = async (values) => {
    try {
      const token = sessionStorage.getItem("token");

      if (!token) {
        alert("Authorization token not found.");
        onCancel(); // Close modal
        return;
      }

      const payload = {
        title: values.Title,
        description: values.Description || null,
        status: values.Status,
        dueDate: values.DueDate.toISOString(),
      };

      console.log("Payload:", payload);

      await axios.post("http://localhost:5000/api/v1/tasks", payload, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      });

      alert("Task created successfully!");
      if (typeof onSuccess === "function") {
        onSuccess();
      }
      form.resetFields();
      onCancel(); // Close modal
    } catch (error) {
      console.error("Task creation failed:", error);
      alert(error.response?.data?.message || "Failed to create task.");
      onCancel(); // Close modal on failure too
    }
  };

  return (
    <Modal
      title="Create Task"
      open={open}
      onCancel={() => {
        form.resetFields();
        onCancel();
      }}
      footer={null}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleFinish}
        initialValues={{ Status: "ToDo" }}
      >
        <Form.Item
          name="Title"
          label="Title"
          rules={[
            { required: true, message: "Title is required" },
            { max: 100, message: "Max 100 characters" },
          ]}
        >
          <Input />
        </Form.Item>

        <Form.Item
          name="Description"
          label="Description"
          rules={[{ max: 500, message: "Max 500 characters" }]}
        >
          <Input.TextArea rows={3} />
        </Form.Item>

        <Form.Item
          name="Status"
          label="Status"
          rules={[{ required: true, message: "Status is required" }]}
        >
          <Select>
            <Select.Option value="ToDo">To Do</Select.Option>
            <Select.Option value="InProgress">In Progress</Select.Option>
            <Select.Option value="Completed">Completed</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="DueDate"
          label="Due Date"
          rules={[{ required: true, message: "Due date is required" }]}
        >
          <DatePicker style={{ width: "100%" }} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block>
            Submit
          </Button>
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default TaskModal;
