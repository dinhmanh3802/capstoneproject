import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"
import customBaseQuery from "./baseQuery"

const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Auth"],
    endpoints: (builder) => ({
        loginUser: builder.mutation({
            query: (userCredential) => ({
                url: `Auth/login`,
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body: userCredential,
            }),
        }),
        forgotPassword: builder.mutation({
            query: (email) => ({
                url: `Auth/request-otp`,
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body: { email },
            }),
        }),

        verifyOtp: builder.mutation({
            query: ({ email, otp }) => ({
                url: `Auth/verify-otp`,
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body: { email, otp },
            }),
        }),

        resetPassword: builder.mutation({
            query: ({ email, newPassword }) => ({
                url: `Auth/reset-password`,
                method: "POST",
                headers: {
                    "Content-type": "application/json",
                },
                body: { email, newPassword },
            }),
        }),
    }),
})

export const { useLoginUserMutation, useForgotPasswordMutation, useResetPasswordMutation, useVerifyOtpMutation } =
    authApi
export default authApi
