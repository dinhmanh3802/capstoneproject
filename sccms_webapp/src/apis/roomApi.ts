// src/apis/roomApi.ts

import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const roomApi = createApi({
    reducerPath: "roomApi",
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
    tagTypes: ["Room"],
    endpoints: (builder) => ({
        // Lấy danh sách phòng theo courseId
        getAllRooms: builder.query({
            query: (courseId) => `Room?courseId=${courseId}`,
            providesTags: ["Room"],
        }),

        // Lấy phòng theo ID
        getRoomById: builder.query({
            query: (id) => `Room/${id}`,
            providesTags: ["Room"],
        }),

        // Tạo phòng mới
        createRoom: builder.mutation({
            query: (roomDto) => ({
                url: "Room",
                method: "POST",
                body: roomDto,
            }),
            invalidatesTags: ["Room"],
        }),

        // Cập nhật phòng
        updateRoom: builder.mutation({
            query: ({ id, roomDto }) => ({
                url: `Room/${id}`,
                method: "PUT",
                body: roomDto,
            }),
            invalidatesTags: ["Room"],
        }),

        // Xóa phòng
        deleteRoom: builder.mutation({
            query: (id) => ({
                url: `Room/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Room"],
        }),
    }),
})

export const {
    useGetAllRoomsQuery,
    useGetRoomByIdQuery,
    useCreateRoomMutation,
    useUpdateRoomMutation,
    useDeleteRoomMutation,
} = roomApi

export default roomApi
