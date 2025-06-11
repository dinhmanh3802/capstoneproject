import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import { url } from "inspector"
import nightShiftAssignmentCreate from "../interfaces/nightShiftAssignmentCreate"

export const nightShiftAssignmentApi = createApi({
    reducerPath: "nightShiftAssignmentApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}/NightShiftAssignment`,
        prepareHeaders: (headers, { getState }) => {
            const token = localStorage.getItem("token")
            if (token) {
                headers.set("Authorization", `Bearer ${token}`)
            }
            return headers
        },
    }),
    tagTypes: ["NightShiftAssignment"],
    endpoints: (builder) => ({
        // 1. Lấy tất cả các phân công ca trực theo courseId và dateTime
        getAllNightShift: builder.query({
            query: ({ courseId, dateTime, status }) => ({
                url: ``,
                params: {
                    ...(courseId && { courseId }),
                    ...(dateTime && { dateTime }),
                    ...(status && { status }),
                },
            }),
            providesTags: ["NightShiftAssignment"],
        }),

        // 2. Lấy các ca trực của người dùng cụ thể trong một khóa tu
        getMyNightShifts: builder.query({
            query: ({ courseId, userId }) => ({
                url: `my-nightShift`,
                params: {
                    ...(userId && { userId }),
                    ...(courseId && { courseId }),
                },
            }),
            providesTags: ["NightShiftAssignment"],
        }),

        // 3. Lấy phân công ca trực theo ID
        getAssignmentById: builder.query({
            query: (id) => `/${id}`,
            providesTags: (result, error, id) => [{ type: "NightShiftAssignment", id }],
        }),

        // 4. Lên lịch các ca trực
        autoAssignNightShifts: builder.mutation({
            query: ({ courseId }) => ({
                url: `/auto-assign?courseId=${courseId}`,
                method: "POST",
            }),
            invalidatesTags: ["NightShiftAssignment"],
        }),

        // 5. Gợi ý nhân viên cho một ca trực
        suggestStaffForShift: builder.query({
            query: ({ date, shiftId, roomId, courseId }) =>
                `suggest?date=${date}&shiftId=${shiftId}&roomId=${roomId}&courseId=${courseId}`,
            providesTags: ["NightShiftAssignment"],
        }),

        // 6. Phân công nhân viên vào một ca trực
        assignStaffToShift: builder.mutation({
            query: (assignmentDto: nightShiftAssignmentCreate) => ({
                url: "/AssignStaff",
                method: "POST",
                body: assignmentDto,
            }),
            invalidatesTags: ["NightShiftAssignment"],
        }),

        // 7. Xóa phân công ca trực theo ID
        deleteAssignment: builder.mutation({
            query: ({ id }) => ({
                url: `/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["NightShiftAssignment"],
        }),

        updateAssignmentStatus: builder.mutation({
            query: (updateDto) => ({
                url: `/update-status/${updateDto.id}`,
                method: "PATCH",
                body: updateDto,
            }),
            invalidatesTags: (result, error, { id }) => [{ type: "NightShiftAssignment", id }],
        }),

        // 9. Gán lại ca trực cho người khác
        reassignStaffToShift: builder.mutation({
            query: (reassignDto) => ({
                url: `/reassign`,
                method: "PATCH",
                body: reassignDto,
            }),
            invalidatesTags: ["NightShiftAssignment"],
        }),

        addAssignment: builder.mutation({
            query: (assignmentDto) => ({
                url: `/`,
                method: "POST",
                body: assignmentDto,
            }),
            invalidatesTags: ["NightShiftAssignment"],
        }),

        // 11. Sửa ca trực
        editAssignment: builder.mutation({
            query: ({ id, ...patch }) => ({
                url: `/${id}`,
                method: "PUT",
                body: patch,
            }),
            invalidatesTags: (result, error, { id }) => [{ type: "NightShiftAssignment", id }],
        }),
    }),
})

export const {
    useGetAllNightShiftQuery,
    useGetMyNightShiftsQuery,
    useGetAssignmentByIdQuery,
    useAutoAssignNightShiftsMutation,
    useSuggestStaffForShiftQuery,
    useAssignStaffToShiftMutation,
    useDeleteAssignmentMutation,
    useUpdateAssignmentStatusMutation,
    useReassignStaffToShiftMutation,
    useAddAssignmentMutation,
    useEditAssignmentMutation,
} = nightShiftAssignmentApi

export default nightShiftAssignmentApi
