// src/apis/staffFreeTimeApi.ts

import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const staffFreeTimeApi = createApi({
    reducerPath: "staffFreeTimeApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`, // Đảm bảo rằng SD_BASE_URL được định nghĩa chính xác
        prepareHeaders: (headers, { getState }) => {
            const token = localStorage.getItem("token")
            if (token) {
                headers.set("Authorization", `Bearer ${token}`)
            }
            return headers
        },
    }),
    tagTypes: ["StaffFreeTime"],
    endpoints: (builder) => ({
        // Lấy tất cả thời gian rảnh theo userId, courseId và dateTime
        getAllStaffFreeTimes: builder.query({
            query: ({ userId, courseId, dateTime }) => {
                let query = `StaffFreeTime?`
                if (userId) query += `userId=${userId}&`
                if (courseId) query += `courseId=${courseId}&`
                if (dateTime) query += `dateTime=${dateTime}`
                return query
            },
            providesTags: ["StaffFreeTime"],
        }),

        // Lấy thời gian rảnh theo ID
        getStaffFreeTimeById: builder.query({
            query: (id) => `StaffFreeTime/${id}`,
            providesTags: ["StaffFreeTime"],
        }),

        // Tạo mới thời gian rảnh
        createStaffFreeTime: builder.mutation({
            query: (staffFreeTimeDto) => ({
                url: "StaffFreeTime",
                method: "POST",
                body: staffFreeTimeDto,
            }),
            invalidatesTags: ["StaffFreeTime"],
        }),

        // Cập nhật thời gian rảnh
        updateStaffFreeTime: builder.mutation({
            query: ({ id, staffFreeTimeDto }) => ({
                url: `StaffFreeTime/${id}`,
                method: "PUT",
                body: staffFreeTimeDto,
            }),
            invalidatesTags: ["StaffFreeTime"],
        }),

        // Xóa thời gian rảnh
        deleteStaffFreeTime: builder.mutation({
            query: (id) => ({
                url: `StaffFreeTime/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["StaffFreeTime"],
        }),
    }),
})

export const {
    useGetAllStaffFreeTimesQuery,
    useLazyGetAllStaffFreeTimesQuery,
    useGetStaffFreeTimeByIdQuery,
    useCreateStaffFreeTimeMutation,
    useUpdateStaffFreeTimeMutation,
    useDeleteStaffFreeTimeMutation,
} = staffFreeTimeApi

export default staffFreeTimeApi
