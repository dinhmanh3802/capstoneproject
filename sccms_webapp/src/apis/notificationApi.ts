import { createApi } from "@reduxjs/toolkit/query/react"
import customBaseQuery from "./baseQuery"

export const notificationApi = createApi({
    reducerPath: "notificationApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Notification"],
    endpoints: (builder) => ({
        getNotifications: builder.query({
            query: (userId: number) => ({
                url: `notification/${userId}`,
            }),
            providesTags: ["Notification"],
        }),
        markAsRead: builder.mutation({
            query: (notificationId: number) => ({
                url: `notification/mark-as-read/${notificationId}`,
                method: "POST",
            }),
            invalidatesTags: ["Notification"],
        }),
        markAllAsRead: builder.mutation<void, void>({
            query: () => ({
                url: `notification/mark-all-as-read`,
                method: "POST",
            }),
            invalidatesTags: ["Notification"],
        }),
    }),
})

export const { useGetNotificationsQuery, useMarkAsReadMutation, useMarkAllAsReadMutation } = notificationApi
export default notificationApi
