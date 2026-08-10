import axiosClient from "./axiosClient";

export const registerUser = async (userData) => {
    const response = await axiosClient.post(
        "/Auth/Register",
        userData
    );

    return response.data;
};

export const loginUser = async (loginData) => {
    const response = await axiosClient.post(
        "/Auth/Login",
        loginData
    );

    return response.data;
};