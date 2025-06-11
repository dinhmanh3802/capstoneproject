import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const studentApi = createApi({
    reducerPath: "studentApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`,
        prepareHeaders: (headers: Headers, api) => {
            const token = localStorage.getItem("token")
            token && headers.append("Authorization", "Bearer " + token)
        },
    }),
    tagTypes: ["Student"],
    endpoints: (builder) => ({
        // Lấy danh sách sinh viên theo các tiêu chí tìm kiếm
        getAllStudents: builder.query({
            query: ({ fullName, email, genders, status, courseId, studentGroupId }) => ({
                url: "student",
                params: {
                    ...(fullName && { fullName }),
                    ...(email && { email }),
                    ...(genders && { genders }),
                    ...(status && { status }),
                    ...(courseId && { courseId }),
                    ...(studentGroupId && { studentGroupId }),
                },
            }),
            providesTags: ["Student"],
        }),

        // Lấy sinh viên theo ID
        getStudentById: builder.query({
            query: (id) => `student/${id}`,
            providesTags: ["Student"],
        }),

        // Lấy sinh viên theo courseId và studentId
        getStudentByCourseAndStudentId: builder.query({
            query: ({ courseId, studentId }) => ({
                url: `course/${courseId}/student/${studentId}`,
            }),
            providesTags: ["Student"],
        }),

        // Tạo mới sinh viên
        createStudent: builder.mutation({
            query: ({ courseId, studentData }) => {
                // Convert studentData to FormData
                const formData = new FormData()
                formData.append("FullName", studentData.fullName)
                formData.append("DateOfBirth", studentData.dateOfBirth)
                formData.append("Gender", studentData.gender)
                formData.append("NationalId", studentData.idNumber)
                formData.append("AcademicPerformance", studentData.academicPerformance)
                formData.append("Conduct", studentData.conduct)
                formData.append("Address", studentData.address)
                formData.append("ParentName", studentData.parentName)
                formData.append("EmergencyContact", studentData.parentPhone)
                formData.append("Email", studentData.email)
                formData.append("Image", studentData.photo[0])
                formData.append("NationalImageFront", studentData.frontIdCard[0])
                formData.append("NationalImageBack", studentData.backIdCard[0])
                return {
                    url: `Student/${courseId}`,
                    method: "POST",
                    body: formData,
                }
            },
            invalidatesTags: ["Student"],
        }),

        // Cập nhật thông tin sinh viên
        updateStudent: builder.mutation({
            query: ({ studentId, studentData }) => {
                const formData = new FormData()
                formData.append("fullName", studentData.fullName)
                formData.append("dateOfBirth", studentData.dateOfBirth)
                formData.append("gender", studentData.gender)
                formData.append("nationalId", studentData.nationalId)
                formData.append("academicPerformance", studentData.academicPerformance)
                formData.append("conduct", studentData.conduct)
                formData.append("address", studentData.address)
                formData.append("parentName", studentData.parentName)
                formData.append("emergencyContact", studentData.emergencyContact)
                formData.append("email", studentData.email)
                if (studentData.image && studentData.image.length > 0) {
                    formData.append("Image", studentData.image[0])
                }
                if (studentData.nationalImageFront && studentData.nationalImageFront.length > 0) {
                    formData.append("NationalImageFront", studentData.nationalImageFront[0])
                }
                if (studentData.nationalImageBack && studentData.nationalImageBack.length > 0) {
                    formData.append("NationalImageBack", studentData.nationalImageBack[0])
                }

                return {
                    url: `student/${studentId}`,
                    method: "PATCH",
                    body: formData,
                }
            },
            invalidatesTags: ["Student"],
        }),

        exportStudentsByCourse: builder.mutation({
            query: (courseId) => ({
                url: `student/${courseId}/export`,
                method: "GET",
                responseHandler: async (response) => response,
            }),
        }),
        exportStudentsByStudentGroup: builder.mutation({
            query: (id) => ({
                url: `student/ExportStudentsByGroup/${id}`,
                method: "GET",
                responseHandler: async (response) => response,
            }),
        }),

        // Lấy danh sách sinh viên theo courseId
        getStudentsByCourseId: builder.query({
            query: (courseId) => ({
                url: `student/by-course/${courseId}`,
            }),
            providesTags: ["Student"],
        }),
    }),
})

export const {
    useGetAllStudentsQuery,
    useGetStudentByIdQuery,
    useGetStudentByCourseAndStudentIdQuery,
    useCreateStudentMutation,
    useUpdateStudentMutation,
    useExportStudentsByCourseMutation,
    useExportStudentsByStudentGroupMutation,
    useGetStudentsByCourseIdQuery,
} = studentApi

export default studentApi
