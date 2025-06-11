import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import customBaseQuery from "./baseQuery"

export const teamApi = createApi({
    reducerPath: "teamApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Team"],
    endpoints: (builder) => ({
        // Fetch all teams for a specific course
        getTeamsByCourseId: builder.query({
            query: (courseId: number) => `team/course/${courseId}`,
            providesTags: ["Team"],
        }),

        // Fetch a single team by ID
        getTeamById: builder.query({
            query: (id: number) => `team/${id}`,
            providesTags: ["Team"],
        }),

        // Create a new team
        createTeam: builder.mutation({
            query: (data) => ({
                url: `team`,
                method: "POST",
                body: data,
            }),
            invalidatesTags: ["Team"],
        }),

        // Update an existing team by ID
        updateTeam: builder.mutation({
            query: ({ id, ...data }) => ({
                url: `team/${id}`,
                method: "PUT",
                body: data,
            }),
            invalidatesTags: ["Team"],
        }),

        // Delete a team by ID
        deleteTeam: builder.mutation({
            query: (id: number) => ({
                url: `team/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Team"],
        }),

        // Add volunteers to a team
        addVolunteersToTeam: builder.mutation({
            query: ({ volunteerIds, teamId }) => ({
                url: `volunteerTeam/add-volunteers`,
                method: "POST",
                body: { volunteerIds, teamId },
            }),
            invalidatesTags: ["Team"],
        }),

        // Remove volunteers from a team
        removeVolunteersFromTeam: builder.mutation({
            query: ({ volunteerIds, teamId }) => {
                return {
                    url: `volunteerTeam/remove-volunteers`,
                    method: "DELETE",
                    body: { volunteerIds, teamId },
                }
            },
            invalidatesTags: ["Team"],
        }),
        autoAssignVolunteersToTeam: builder.mutation({
            query: (courseId) => ({
                url: `volunteerTeam/auto-assign?courseId=${courseId}`,
                method: "POST",
            }),
            invalidatesTags: ["Team"],
        }),
        getVolunteersInTeam: builder.query({
            query: ({ teamId, volunteerCode, fullName, phoneNumber, gender, status }) => {
                let query = `team/${teamId}/volunteers`
                const queryParams = []

                if (volunteerCode) queryParams.push(`volunteerCode=${volunteerCode}`)
                if (fullName) queryParams.push(`fullName=${fullName}`)
                if (phoneNumber) queryParams.push(`phoneNumber=${phoneNumber}`)
                if (gender) queryParams.push(`gender=${gender}`)
                if (status) queryParams.push(`status=${status}`)

                if (queryParams.length) {
                    query += `?${queryParams.join("&")}`
                }

                return query
            },
            providesTags: ["Team"],
        }),
    }),
})

export const {
    useGetTeamsByCourseIdQuery,
    useGetTeamByIdQuery,
    useCreateTeamMutation,
    useUpdateTeamMutation,
    useDeleteTeamMutation,
    useAddVolunteersToTeamMutation,
    useRemoveVolunteersFromTeamMutation,
    useAutoAssignVolunteersToTeamMutation,
    useGetVolunteersInTeamQuery,
} = teamApi

export default teamApi
