import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import { studentApplicationResultRequest } from "../interfaces"
import customBaseQuery from "./baseQuery"

export const studentApplicationApi = createApi({
    reducerPath: "studentApplicationApi",
    baseQuery: customBaseQuery,
    tagTypes: ["StudentApplication"],
    endpoints: (builder) => ({
        getStudentApplications: builder.query({
            query: ({
                courseId,
                name,
                phoneNumber,
                status,
                reviewerId,
                gender,
                parentName,
                birthDateFrom,
                birthDateTo,
                nationalId,
            }) => ({
                url: "studentApplication",
                params: {
                    ...(courseId && { courseId }),
                    ...(name && { name }),
                    ...(phoneNumber && { phoneNumber }),
                    ...(status != null && { status }),
                    ...(reviewerId && { reviewerId }),
                    ...(gender != null && { gender }),
                    ...(parentName && { parentName }),
                    ...(birthDateFrom && { startDob: birthDateFrom.toISOString() }),
                    ...(birthDateTo && { endDob: birthDateTo.toISOString() }),
                    ...(nationalId && { nationalId }),
                },
            }),
            providesTags: ["StudentApplication"],
        }),
        getStudentCourse: builder.query({
            query: ({
                courseId,
                name,
                phoneNumber,
                status,
                reviewerId,
                gender,
                parentName,
                dateOfBirthFrom,
                dateOfBirthTo,
                studentCode,
                studentGroup,
                StudentGroupExcept,
                isGetStudentDrop,
            }) => ({
                url: "studentApplication/student",
                params: {
                    ...(courseId && { courseId }),
                    ...(name && { name }),
                    ...(phoneNumber != null && { phoneNumber }),
                    ...(status != null && { status }),
                    ...(reviewerId && { reviewerId }),
                    ...(gender != null && { gender }),
                    ...(parentName && { parentName }),
                    ...(dateOfBirthFrom != null && { startDob: dateOfBirthFrom }),
                    ...(dateOfBirthTo != null && { endDob: dateOfBirthTo }),
                    ...(studentCode && { studentCode }),
                    ...(studentGroup && { studentGroup }),
                    ...(StudentGroupExcept && { StudentGroupExcept }),
                    ...(isGetStudentDrop != null && { isGetStudentDrop }),
                },
            }),
            providesTags: ["StudentApplication"],
        }),
        getStudentApplicationById: builder.query({
            query: (id) => `studentApplication/${id}`,
            providesTags: ["StudentApplication"],
        }),
        createStudentApplication: builder.mutation({
            query: (body) => ({
                url: "studentApplication",
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body,
            }),
            invalidatesTags: ["StudentApplication"],
        }),
        sendStudentApplicationResult: builder.mutation({
            query: (body: studentApplicationResultRequest) => ({
                url: "studentApplication/SendResult",
                method: "POST",
                body,
            }),
            invalidatesTags: ["StudentApplication"],
        }),
        updateStudentApplication: builder.mutation({
            query: (body) => ({
                url: `studentApplication`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["StudentApplication"],
        }),
        updateStudentApplicationDetail: builder.mutation({
            query: (body) => ({
                url: `studentApplication/detail`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["StudentApplication"],
        }),
        autoAssignApplications: builder.mutation({
            query: (courseId) => ({
                url: `studentApplication/AutoAssign?courseId=${courseId}`,
                method: "PUT",
            }),
            invalidatesTags: ["StudentApplication"],
        }),

        deleteStudentApplication: builder.mutation({
            query: (id) => ({
                url: `studentApplication/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["StudentApplication"],
        }),
        printStudentCards: builder.mutation({
            query: ({ studentIds, courseId }) => ({
                url: `studentApplication/printCards/${courseId}`,
                method: "POST",
                body: studentIds,
                responseHandler: async (response) => response,
                headers: {
                    "Content-Type": "application/json",
                },
            }),
        }),
    }),
})

export const {
    useGetStudentApplicationsQuery,
    useGetStudentCourseQuery,
    useGetStudentApplicationByIdQuery,
    useCreateStudentApplicationMutation,
    useUpdateStudentApplicationMutation,
    useUpdateStudentApplicationDetailMutation,
    useDeleteStudentApplicationMutation,
    useAutoAssignApplicationsMutation,
    usePrintStudentCardsMutation,
    useSendStudentApplicationResultMutation,
} = studentApplicationApi

export default studentApplicationApi
