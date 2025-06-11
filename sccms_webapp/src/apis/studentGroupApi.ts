import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import customBaseQuery from "./baseQuery"

export const studentGroupApi = createApi({
    reducerPath: "studentGroupApi",
    baseQuery: customBaseQuery, // Sử dụng customBaseQuery
    tagTypes: ["StudentGroup"],
    endpoints: (builder) => ({
        getStudentGroups: builder.query({
            query: (courseId: any) => `StudentGroup/course/${courseId}`,
            providesTags: ["StudentGroup"],
        }),
        getStudentGroup: builder.query({
            query: (id: any) => `StudentGroup/${id}`,
            providesTags: ["StudentGroup"],
        }),
        createStudentGroup: builder.mutation({
            query: (data) => ({
                url: `StudentGroup`,
                method: "POST",
                body: data,
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        updateStudentGroup: builder.mutation({
            query: (data) => ({
                url: `StudentGroup/${data.id}`,
                method: "PUT",
                body: data,
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        deleteStudentGroup: builder.mutation({
            query: (id: number) => ({
                url: `StudentGroup/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        addStudentsIntoGroup: builder.mutation({
            query: ({
                studentIds,
                studentGroupId,
                courseId,
            }: {
                studentIds: number[]
                studentGroupId: number
                courseId: number
            }) => ({
                url: `StudentGroupAssignment`,
                method: "POST",
                body: { studentIds, studentGroupId, courseId },
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        removeStudentsFromGroup: builder.mutation({
            query: (data) => ({
                url: `studentGroupAssignment/remove`,
                method: "DELETE",
                body: data,
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        autoAssignStudentsToGroup: builder.mutation({
            query: (courseId) => ({
                url: `studentGroupAssignment/auto-assign?courseId=${courseId}`,
                method: "PUT",
            }),
            invalidatesTags: ["StudentGroup"],
        }),
        autoAssignSupervisors: builder.mutation<any, number>({
            query: (courseId) => ({
                url: `studentGroupAssignment/auto-assign-supervisors?courseId=${courseId}`,
                method: "POST",
            }),
            invalidatesTags: ["StudentGroup"], // Invalid   ate để refetch data
        }),
    }),
})

export const {
    useGetStudentGroupsQuery,
    useGetStudentGroupQuery,
    useCreateStudentGroupMutation,
    useUpdateStudentGroupMutation,
    useDeleteStudentGroupMutation,
    useAddStudentsIntoGroupMutation,
    useRemoveStudentsFromGroupMutation,
    useAutoAssignStudentsToGroupMutation,
    useAutoAssignSupervisorsMutation,
} = studentGroupApi
