import React, { useEffect } from "react";
import { Modal, Form, Input, Select, DatePicker, Button } from "antd";
import axios from "axios";
import dayjs from "dayjs";

const TaskUpdateModal = ({ open, onCancel, onSuccess, task }) => {
  const [form] = Form.useForm();

  useEffect(() => {
    if (task && open) {
      form.setFieldsValue({
        Title: task.title,
        Description: task.description,
        Status: task.status,
        DueDate: dayjs(task.dueDate),
      });
    } else {
      form.resetFields();
    }
  }, [task, form, open]);

  const handleFinish = async (values) => {
    try {
      const token = sessionStorage.getItem("token");
      if (!token) {
        alert("Authorization token not found.");
        onCancel();
        return;
      }

      const payload = {
        id: task.id || task._id || task.ID, // use whichever exists
        title: values.Title,
        description: values.Description || null,
        status: values.Status,
        dueDate: values.DueDate.toISOString(),
      };

      await axios.put(`http://localhost:5000/api/v1/tasks`, payload, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      });

      alert("Task updated successfully!");
      form.resetFields();
      onCancel();

      if (typeof onSuccess === "function") {
        onSuccess();
      }
    } catch (error) {
      console.error("Task update failed:", error);
      alert(error.response?.data?.message || "Failed to update task.");
      onCancel();
    }
  };

  return (
    <Modal
      title="Update Task"
      open={open}
      onCancel={() => {
        form.resetFields();
        onCancel();
      }}
      footer={null}
      destroyOnClose
    >
      <Form form={form} layout="vertical" onFinish={handleFinish}>
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
            <Select.Option value="Completed">Completed</Select.Option>{" "}
            {/* added since your example has it */}
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
            Update
          </Button>
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default TaskUpdateModal;
