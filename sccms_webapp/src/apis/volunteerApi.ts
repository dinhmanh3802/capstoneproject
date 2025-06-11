import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const volunteerApi = createApi({
    reducerPath: "volunteerApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`,
        prepareHeaders: (headers: Headers) => {
            const token = localStorage.getItem("token")
            if (token) {
                headers.append("Authorization", `Bearer ${token}`)
            }
            return headers
        },
    }),
    tagTypes: ["Volunteer"],
    endpoints: (builder) => ({
        // Lấy danh sách tình nguyện viên theo các tiêu chí tìm kiếm
        getAllVolunteers: builder.query({
            query: ({ fullName, gender, status, teamId, courseId, nationalId, address }) => ({
                url: "volunteer",
                params: {
                    ...(fullName && { fullName }),
                    ...(gender && { gender }),
                    ...(status && { status }),
                    ...(teamId && { teamId }),
                    ...(courseId && { courseId }),
                    ...(nationalId && { nationalId }),
                    ...(address && { address }),
                },
            }),
            providesTags: ["Volunteer"],
        }),

        // Lấy tình nguyện viên theo ID
        getVolunteerById: builder.query({
            query: (volunteerId) => `volunteer/${volunteerId}`,
            providesTags: ["Volunteer"],
        }),

        // Tạo mới tình nguyện viên
        createVolunteer: builder.mutation({
            query: ({ courseId, volunteerData }) => {
                const formData = new FormData()
                formData.append("FullName", volunteerData.fullName)
                formData.append("DateOfBirth", volunteerData.dateOfBirth)
                formData.append("Gender", volunteerData.gender)
                formData.append("NationalId", volunteerData.idNumber)
                formData.append("Address", volunteerData.address)
                formData.append("PhoneNumber", volunteerData.phoneNumber)
                formData.append("Email", volunteerData.email)
                formData.append("Image", volunteerData.photo[0])
                formData.append("NationalImageFront", volunteerData.frontIdCard[0])
                formData.append("NationalImageBack", volunteerData.backIdCard[0])
                return {
                    url: `volunteer?courseId=${courseId}`,
                    method: "POST",
                    body: formData,
                }
            },
            invalidatesTags: ["Volunteer"],
        }),
        updateVolunteerInCourse: builder.mutation({
            query: ({ volunteerId, courseId, volunteerData }) => {
                const formData = new FormData()
                formData.append("FullName", volunteerData.fullName)
                formData.append("DateOfBirth", volunteerData.dateOfBirth)
                formData.append("Gender", volunteerData.gender)
                formData.append("NationalId", volunteerData.nationalId)
                formData.append("Address", volunteerData.address)
                formData.append("TeamId", volunteerData.teamId.toString())
                formData.append("Email", volunteerData.email)
                formData.append("PhoneNumber", volunteerData.phoneNumber)
                if (volunteerData.note) formData.append("Note", volunteerData.note)

                // Check and append files if they exist
                if (volunteerData.image) formData.append("Image", volunteerData.image[0])
                if (volunteerData.nationalImageFront)
                    formData.append("NationalImageFront", volunteerData.nationalImageFront[0])
                if (volunteerData.nationalImageBack)
                    formData.append("NationalImageBack", volunteerData.nationalImageBack[0])

                return {
                    url: `volunteer/${volunteerId}/UpdateInCourse/${courseId}`,
                    method: "PUT",
                    body: formData,
                }
            },
            invalidatesTags: ["Volunteer"],
        }),

        // Cập nhật tình nguyện viên
        updateVolunteer: builder.mutation({
            query: ({ volunteerId, volunteerData }) => ({
                url: `volunteer/${volunteerId}`,
                method: "PUT",
                body: volunteerData,
            }),
            invalidatesTags: ["Volunteer"],
        }),

        // Lấy danh sách tình nguyện viên theo courseId
        getVolunteersByCourseId: builder.query({
            query: (courseId) => ({
                url: `volunteer/ByCourse/${courseId}`,
            }),
            providesTags: ["Volunteer"],
        }),

        // Xuất danh sách tình nguyện viên theo courseId dưới dạng file Excel
        exportVolunteersByCourseId: builder.mutation({
            query: (courseId) => ({
                url: `Volunteer/ExportVolunteersByCourse/${courseId}`,
                method: "GET",
                responseHandler: async (response) => response,
            }),
        }),

        // Xuất danh sách tình nguyện viên theo teamId dưới dạng file Excel
        exportVolunteersByTeamId: builder.mutation({
            query: (teamId) => ({
                url: `volunteer/ExportVolunteersByTeam/${teamId}`,
                method: "GET",
                responseHandler: async (response) => response,
            }),
            invalidatesTags: [],
        }),
    }),
})

export const {
    useGetAllVolunteersQuery,
    useGetVolunteerByIdQuery,
    useCreateVolunteerMutation,
    useUpdateVolunteerMutation,
    useGetVolunteersByCourseIdQuery,
    useExportVolunteersByCourseIdMutation,
    useExportVolunteersByTeamIdMutation,
    useUpdateVolunteerInCourseMutation,
} = volunteerApi

export default volunteerApi
