import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const feedbackApi = createApi({
    reducerPath: "feedbackApi",
    baseQuery: fetchBaseQuery({ baseUrl: `${SD_BASE_URL}` }),
    tagTypes: ["Feedback"],
    endpoints: (builder) => ({
        // Get all feedbacks with courseId as a required query parameter
        getFeedbacks: builder.query({
            query: ({ courseId, feedbackDateStart, feedbackDateEnd }) => ({
                url: `feedback`,
                params: {
                    courseId, // courseId is required
                    ...(feedbackDateStart && { feedbackDateStart: feedbackDateStart }),
                    ...(feedbackDateEnd && { feedbackDateEnd: feedbackDateEnd }),
                },
            }),
            providesTags: ["Feedback"],
        }),

        // Get a feedback by ID
        getFeedbackById: builder.query({
            query: (id) => ({
                url: `feedback/${id}`,
            }),
            providesTags: (id) => [{ type: "Feedback", id }],
        }),

        // Create a new feedback
        createFeedback: builder.mutation({
            query: (feedbackCreateDto) => ({
                url: "feedback",
                method: "POST",
                body: feedbackCreateDto,
            }),
            invalidatesTags: ["Feedback"],
        }),

        // Delete a feedback by ID
        deleteFeedback: builder.mutation({
            query: (id) => ({
                url: `feedback/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Feedback"],
        }),

        // Bulk delete feedbacks by IDs
        deleteFeedbacksByIds: builder.mutation({
            query: (ids) => ({
                url: `feedback/bulk-delete`,
                method: "DELETE",
                body: ids,
            }),
            invalidatesTags: ["Feedback"],
        }),
    }),
})

export const {
    useGetFeedbacksQuery,
    useGetFeedbackByIdQuery,
    useCreateFeedbackMutation,
    useDeleteFeedbackMutation,
    useDeleteFeedbacksByIdsMutation,
} = feedbackApi

export default feedbackApi
