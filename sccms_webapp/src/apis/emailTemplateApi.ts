import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const emailTemplateApi = createApi({
    reducerPath: "emailTemplateApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`,
        prepareHeaders: (headers: Headers, api) => {
            const token = localStorage.getItem("token")
            token && headers.append("Authorization", "Bearer " + token)
        },
    }),
    tagTypes: ["EmailTemplate"],
    endpoints: (builder) => ({
        getAllEmailTemplate: builder.query({
            query: () => "EmailTemplate",
            providesTags: ["EmailTemplate"],
        }),
        getEmailTemplateById: builder.query({
            query: (id) => `email-template/${id}`,
            providesTags: ["EmailTemplate"],
        }),
        updateEmailTemplate: builder.mutation({
            query: ({ id, body }) => ({
                url: `email-template/${id}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["EmailTemplate"],
        }),
    }),
})

export const { useGetAllEmailTemplateQuery, useGetEmailTemplateByIdQuery, useUpdateEmailTemplateMutation } =
    emailTemplateApi
