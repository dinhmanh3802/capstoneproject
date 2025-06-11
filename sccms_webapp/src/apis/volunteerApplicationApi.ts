import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import customBaseQuery from "./baseQuery"
import volunteerApplicationResultRequest from "../interfaces/volunteerApplicationResultRequest"

export const volunteerApplicationApi = createApi({
    reducerPath: "volunteerApplicationApi",
    baseQuery: customBaseQuery,
    tagTypes: ["VolunteerApplication"],
    endpoints: (builder) => ({
        // Lấy danh sách ứng dụng tình nguyện viên
        getVolunteerApplications: builder.query({
            query: ({
                courseId,
                name,
                gender,
                phoneNumber,
                status,
                reviewerId,
                dateOfBirthFrom,
                dateOfBirthTo,
                teamId,
                volunteerCode,
                nationalId,
            }) => ({
                url: `VolunteerCourse/GetVolunteerApplication/${courseId}`,
                params: {
                    ...(name && { name }),
                    ...(gender != null && { gender }),
                    ...(phoneNumber && { phoneNumber }),
                    ...(status != null && { status }),
                    ...(reviewerId && { reviewerId }),
                    ...(teamId != null && { teamId }),
                    ...(dateOfBirthFrom && { startDob: dateOfBirthFrom }),
                    ...(dateOfBirthTo && { endDob: dateOfBirthTo }),
                    ...(phoneNumber && { phoneNumber: phoneNumber }),
                    ...(volunteerCode && { volunteerCode: volunteerCode }),
                    ...(nationalId && { nationalId: nationalId }),
                },
            }),
            providesTags: ["VolunteerApplication"],
        }),

        // Lấy thông tin tình nguyện viên trong một khóa tu theo ID tình nguyện viên và ID khóa tu
        getVolunteerCourseByVolunteerIdAndCourseId: builder.query({
            query: ({ volunteerId, courseId }) => ({
                url: `VolunteerCourse/volunteer/${volunteerId}/course/${courseId}`,
            }),
            providesTags: ["VolunteerApplication"],
        }),

        // Lấy ứng dụng tình nguyện viên theo ID
        getVolunteerApplicationById: builder.query({
            query: (id) => ({
                url: `VolunteerCourse/${id}`,
            }),
            providesTags: ["VolunteerApplication"],
        }),

        // Cập nhật tình trạng ứng dụng tình nguyện viên
        updateVolunteerApplication: builder.mutation({
            query: (body) => ({
                url: `VolunteerCourse`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["VolunteerApplication"],
        }),

        // Tự động gán ứng dụng tình nguyện viên vào khóa tu
        autoAssignVolunteerApplications: builder.mutation({
            query: (courseId) => ({
                url: `VolunteerCourse/AutoAssign?courseId=${courseId}`,
                method: "PUT",
            }),
            invalidatesTags: ["VolunteerApplication"],
        }),
        sendVolunteerApplicationResult: builder.mutation({
            query: (body: volunteerApplicationResultRequest) => {
                return {
                    url: "volunteerCourse/SendResult",
                    method: "POST",
                    body,
                }
            },
            invalidatesTags: ["VolunteerApplication"],
        }),
        printVolunteerCards: builder.mutation({
            query: ({ volunteerIds, courseId }) => ({
                url: `volunteerCourse/printCards/${courseId}`,
                method: "POST",
                body: volunteerIds,
                responseHandler: async (response) => response,
                headers: {
                    "Content-Type": "application/json",
                },
            }),
        }),
        printVolunteerCertificate: builder.mutation({
            query: ({ volunteerIds, courseId }) => ({
                url: `volunteerCourse/printCertificate/${courseId}`,
                method: "POST",
                body: volunteerIds,
                responseHandler: async (response) => response,
                headers: {
                    "Content-Type": "application/json",
                },
            }),
        }),
        updateVolunteerInformationInACourse: builder.mutation({
            query: (volunteerInfoDto) => {
                return {
                    url: `VolunteerCourse/UpdateVolunteerInformation`,
                    method: "PUT",
                    body: volunteerInfoDto,
                }
            },
            invalidatesTags: ["VolunteerApplication"],
        }),
    }),
})

export const {
    useGetVolunteerApplicationsQuery,
    useGetVolunteerCourseByVolunteerIdAndCourseIdQuery,
    useGetVolunteerApplicationByIdQuery, // Newly added hook for fetching by ID
    useUpdateVolunteerApplicationMutation,
    useAutoAssignVolunteerApplicationsMutation,
    useSendVolunteerApplicationResultMutation,
    usePrintVolunteerCardsMutation,
    usePrintVolunteerCertificateMutation,
    useUpdateVolunteerInformationInACourseMutation,
} = volunteerApplicationApi

export default volunteerApplicationApi
