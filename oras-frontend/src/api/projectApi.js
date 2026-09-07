import axiosClient from "./axiosClient";

export const getAllProjects = async () => {
    const response = await axiosClient.get("/Project");
    return response.data;
};

export const getProjectById = async (id) => {
    const response = await axiosClient.get(`/Project/${id}`);
    return response.data;
};

export const createProject = async (projectData) => {
    const response = await axiosClient.post("/Project", projectData);
    return response.data;
};

export const updateProject = async (id, projectData) => {
    const response = await axiosClient.put(`/Project/${id}`, projectData);
    return response.data;
};

export const deleteProject = async (id) => {
    await axiosClient.delete(`/Project/${id}`);
};
