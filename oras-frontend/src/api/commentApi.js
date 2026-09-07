import axiosClient from "./axiosClient";

export const getAllComments = async (params = {}) => {
    const query = new URLSearchParams();
    if (params.taskId) query.append("taskId", params.taskId);
    const queryString = query.toString();
    const url = queryString ? `/Comment?${queryString}` : "/Comment";
    const response = await axiosClient.get(url);
    return response.data;
};

export const getCommentById = async (id) => {
    const response = await axiosClient.get(`/Comment/${id}`);
    return response.data;
};

export const createComment = async (commentData) => {
    const response = await axiosClient.post("/Comment", commentData);
    return response.data;
};

export const updateComment = async (id, commentData) => {
    const response = await axiosClient.put(`/Comment/${id}`, commentData);
    return response.data;
};

export const deleteComment = async (id) => {
    await axiosClient.delete(`/Comment/${id}`);
};
