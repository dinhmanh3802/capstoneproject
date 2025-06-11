import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import { studentApplicationResultRequest } from "../interfaces"

export const emailApi = createApi({
    reducerPath: "emailApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${SD_BASE_URL}`,
        prepareHeaders: (headers: Headers, api) => {
            const token = localStorage.getItem("token")
            token && headers.append("Authorization", "Bearer " + token)
        },
    }),
    tagTypes: ["Email"],
    endpoints: (builder) => ({
        sendEmail: builder.mutation({
            query: (body: studentApplicationResultRequest) => ({
                url: `Email`,
                method: "POST",
                body,
            }),
        }),
    }),
})

export const { useSendEmailMutation } = emailApi
