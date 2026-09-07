import axiosClient from "./axiosClient";

export const getAllUsers = async () => {
    const response = await axiosClient.get("/User");
    return response.data;
};

export const getUserById = async (id) => {
    const response = await axiosClient.get(`/User/${id}`);
    return response.data;
};

export const createUser = async (userData) => {
    const response = await axiosClient.post("/User", userData);
    return response.data;
};

export const updateUser = async (id, userData) => {
    const response = await axiosClient.put(`/User/${id}`, userData);
    return response.data;
};

export const deleteUser = async (id) => {
    await axiosClient.delete(`/User/${id}`);
};
