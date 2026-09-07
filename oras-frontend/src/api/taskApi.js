import axiosClient from "./axiosClient";

export const getAllTasks = async (params = {}) => {
    const query = new URLSearchParams();
    if (params.projectId) query.append("projectId", params.projectId);
    if (params.status) query.append("status", params.status);
    if (params.assigneeId) query.append("assigneeId", params.assigneeId);

    const queryString = query.toString();
    const url = queryString ? `/Task?${queryString}` : "/Task";
    const response = await axiosClient.get(url);
    return response.data;
};

export const getTaskById = async (id) => {
    const response = await axiosClient.get(`/Task/${id}`);
    return response.data;
};

export const createTask = async (taskData) => {
    const response = await axiosClient.post("/Task", taskData);
    return response.data;
};

export const updateTask = async (id, taskData) => {
    const response = await axiosClient.put(`/Task/${id}`, taskData);
    return response.data;
};

export const deleteTask = async (id) => {
    await axiosClient.delete(`/Task/${id}`);
};
