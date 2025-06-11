import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import customBaseQuery from "./baseQuery"
export interface RegistrationOverTimeDto {
    period: string // YYYY-MM (tháng)
    count: number
}
export interface RegistrationPerCourseDto {
    courseId: number
    courseName: string
    registrationCount: number
}
export const courseApi = createApi({
    reducerPath: "courseApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Course"],
    endpoints: (builder) => ({
        getCourse: builder.query({
            query: ({ name, status, startDate, endDate }) => ({
                url: "course",
                params: {
                    ...(name && { name }),
                    ...(status && { status }),
                    ...(startDate && { startDate: startDate.toISOString() }),
                    ...(endDate && { endDate: endDate.toISOString() }),
                },
            }),
            providesTags: ["Course"],
        }),
        getFeedbackCourse: builder.query({
            query: () => ({
                url: "Course/available-feedback-courses",
            }),
            providesTags: ["Course"],
        }),

        getCourseById: builder.query({
            query: (id) => `course/${id}`,
            providesTags: ["Course"],
        }),
        createCourse: builder.mutation({
            query: (body) => ({
                url: "Course",
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body,
            }),
            invalidatesTags: ["Course"],
        }),
        updateCourse: builder.mutation({
            query: ({ id, body }) => ({
                url: `course/${id}`,
                method: "PATCH",
                body,
            }),
            invalidatesTags: ["Course"],
        }),
        deleteCourse: builder.mutation({
            query: (id) => ({
                url: `course/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Course"],
        }),
        getCurrentCourse: builder.query({
            query: () => "course/current-course",
            providesTags: ["Course"],
        }),
        getCourseDashboard: builder.query({
            query: (courseId) => ({
                url: `course/dashboard/${courseId}`,
            }),
            providesTags: ["Course"],
        }),
        // Endpoint lấy số lượng đăng ký học sinh theo khóa tu
        getStudentRegistrationsPerCourse: builder.query<any, number>({
            query: (years) => `course/registrations/students-per-course?years=${years}`,
        }),

        // Endpoint lấy số lượng đăng ký tình nguyện viên theo khóa tu
        getVolunteerRegistrationsPerCourse: builder.query<any, number>({
            query: (years) => `course/registrations/volunteers-per-course?years=${years}`,
        }),
    }),
})

export const {
    useGetCourseQuery,
    useGetFeedbackCourseQuery,
    useGetCourseByIdQuery,
    useGetCurrentCourseQuery,
    useCreateCourseMutation,
    useUpdateCourseMutation,
    useDeleteCourseMutation,
    useGetCourseDashboardQuery,
    useGetStudentRegistrationsPerCourseQuery,
    useGetVolunteerRegistrationsPerCourseQuery,
} = courseApi
export default courseApi
