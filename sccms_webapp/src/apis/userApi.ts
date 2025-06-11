// src/apis/userApi.ts
import { createApi } from "@reduxjs/toolkit/query/react"
import customBaseQuery from "./baseQuery"
import { SD_BASE_URL, SD_UserStatus } from "../utility/SD"
import { apiResponse } from "../interfaces"

export const userApi = createApi({
    reducerPath: "userApi",
    baseQuery: customBaseQuery, // Sử dụng customBaseQuery
    tagTypes: ["User", "Supervisor", "StudentGroup", "Course"],
    endpoints: (builder) => ({
        getUsers: builder.query({
            query: ({ name, email, phoneNumber, status, gender, roleId, startDate, endDate }) => ({
                url: "user",
                params: {
                    ...(name && { name }),
                    ...(email && { email }),
                    ...(phoneNumber && { phoneNumber }),
                    ...(status && { status }),
                    ...(gender && { gender }),
                    ...(roleId && { roleId }),
                    ...(startDate && { startDate: startDate.toISOString() }),
                    ...(endDate && { endDate: endDate.toISOString() }),
                },
            }),
            providesTags: ["User"],
        }),
        getUserById: builder.query({
            query: (id) => `user/${id}`,
            providesTags: ["User"],
        }),
        createUser: builder.mutation({
            query: (body) => ({
                url: "user",
                method: "POST",
                body,
            }),
            invalidatesTags: ["User"],
        }),
        updateUser: builder.mutation({
            query: ({ id, body }) => ({
                url: `user/${id}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["User"],
        }),
        deleteUser: builder.mutation({
            query: (id) => ({
                url: `user/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["User"],
        }),
        changePassword: builder.mutation({
            query: ({ userId, body }) => ({
                url: `user/change-password/${userId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["User"],
        }),
        changeUserStatus: builder.mutation({
            query: ({ userIds, newStatus }: { userIds: number[]; newStatus: SD_UserStatus }) => ({
                url: "user/change-status",
                method: "PUT",
                body: userIds,
                params: {
                    newStatus,
                },
            }),
            invalidatesTags: ["User"],
        }),
        resetUserPassword: builder.mutation({
            query: ({ userId, newPassword }) => ({
                url: `User/reset-password/${userId}`,
                method: "PUT",
                headers: {
                    "Content-type": "application/json",
                },
                body: { newPassword },
            }),
            invalidatesTags: ["User"],
        }),
        downloadTemplate: builder.mutation<Blob, void>({
            query: () => ({
                url: "user/download-template",
                method: "GET",
                responseHandler: (response) => response.blob(),
            }),
        }),
        bulkCreateUsers: builder.mutation<apiResponse, FormData>({
            query: (formData) => ({
                url: "user/bulk-create",
                method: "POST",
                body: formData,
            }),
            invalidatesTags: ["User"],
        }),
        changeUserRole: builder.mutation({
            query: ({ userIds, newRoleId }) => ({
                url: "user/change-role",
                method: "PUT",
                body: {
                    userIds,
                    newRoleId,
                },
            }),
            invalidatesTags: ["User", "Supervisor", "StudentGroup", "Course"],
        }),
        getAvailableSupervisors: builder.query<apiResponse, void>({
            query: () => ({
                url: "user/available-supervisors",
                method: "GET",
            }),
            providesTags: ["Supervisor"],
        }),
    }),
})

export const {
    useGetAvailableSupervisorsQuery,
    useGetUsersQuery,
    useCreateUserMutation,
    useGetUserByIdQuery,
    useUpdateUserMutation,
    useDeleteUserMutation,
    useChangePasswordMutation,
    useChangeUserStatusMutation,
    useResetUserPasswordMutation,
    useDownloadTemplateMutation,
    useBulkCreateUsersMutation,
    useChangeUserRoleMutation,
} = userApi

export default userApi
