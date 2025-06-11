// src/apis/supervisorApi.ts

import { createApi } from "@reduxjs/toolkit/query/react"
import customBaseQuery from "./baseQuery"
import { SD_BASE_URL } from "../utility/SD"

export const supervisorApi = createApi({
    reducerPath: "supervisorApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Supervisor"],
    endpoints: (builder) => ({
        getSupervisors: builder.query({
            query: ({ courseId, name, email, phoneNumber, status, gender }) => ({
                url: "supervisor",
                params: {
                    courseId,
                    ...(name && { name }),
                    ...(email && { email }),
                    ...(phoneNumber && { phoneNumber }),
                    ...(status && { status }),
                    ...(gender && { gender }),
                },
            }),
            providesTags: ["Supervisor"],
        }),
        getSupervisorById: builder.query({
            query: (id) => `supervisor/${id}`,
            providesTags: ["Supervisor"],
        }),
        updateSupervisor: builder.mutation({
            query: ({ id, body }) => ({
                url: `supervisor/${id}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["Supervisor"],
        }),

        changeSupervisorsGroup: builder.mutation<void, { supervisorIds: number[]; newGroupId: number }>({
            query: ({ supervisorIds, newGroupId }) => ({
                url: "supervisor/change-group",
                method: "PUT",
                body: supervisorIds,
                params: { newGroupId },
                headers: {
                    "Content-Type": "application/json",
                },
            }),
            invalidatesTags: ["Supervisor"],
        }),
        getAvailableSupervisors: builder.query({
            query: (courseId) => `supervisor/available?courseId=${courseId}`,
            providesTags: ["Supervisor"],
        }),
    }),
})

export const {
    useGetSupervisorsQuery,
    useGetSupervisorByIdQuery,
    useUpdateSupervisorMutation,
    useChangeSupervisorsGroupMutation,
    useGetAvailableSupervisorsQuery,
} = supervisorApi

export default supervisorApi
