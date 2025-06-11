import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const nightShiftApi = createApi({
    reducerPath: "nightShiftApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`,
        prepareHeaders: (headers, { getState }) => {
            const token = localStorage.getItem("token")
            if (token) {
                headers.set("Authorization", `Bearer ${token}`)
            }
            return headers
        },
    }),
    tagTypes: ["NightShift"],
    endpoints: (builder) => ({
        // Lấy danh sách ca trực theo courseId
        getAllNightShifts: builder.query({
            query: (courseId) => `NightShift?courseId=${courseId}`,
            providesTags: ["NightShift"],
        }),

        // Lấy ca trực theo ID
        getNightShiftById: builder.query({
            query: (id) => `NightShift/${id}`,
            providesTags: ["NightShift"],
        }),

        // Tạo ca trực mới
        createNightShift: builder.mutation({
            query: (nightShiftDto) => ({
                url: "NightShift",
                method: "POST",
                body: nightShiftDto,
            }),
            invalidatesTags: ["NightShift"],
        }),

        // Cập nhật ca trực
        updateNightShift: builder.mutation({
            query: ({ id, nightShiftDto }) => ({
                url: `NightShift/${id}`,
                method: "PUT",
                body: nightShiftDto,
            }),
            invalidatesTags: ["NightShift"],
        }),

        // Xóa ca trực
        deleteNightShift: builder.mutation({
            query: (id) => ({
                url: `NightShift/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["NightShift"],
        }),
    }),
})

export const {
    useGetAllNightShiftsQuery,
    useGetNightShiftByIdQuery,
    useCreateNightShiftMutation,
    useUpdateNightShiftMutation,
    useDeleteNightShiftMutation,
} = nightShiftApi

export default nightShiftApi
