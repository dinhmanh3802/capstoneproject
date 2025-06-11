import { fetchBaseQuery, FetchArgs, FetchBaseQueryError } from "@reduxjs/toolkit/query/react"
import type { BaseQueryFn } from "@reduxjs/toolkit/query"
import { logout } from "../store/slice/authSlice" // Điều chỉnh đường dẫn nếu cần
import { toastNotify } from "../helper"
import { SD_BASE_URL } from "../utility/SD"

const baseQuery = fetchBaseQuery({
    baseUrl: SD_BASE_URL,
    prepareHeaders: (headers) => {
        const token = localStorage.getItem("token")
        if (token) {
            headers.set("Authorization", `Bearer ${token}`)
        }
        return headers
    },
})

const customBaseQuery: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
    args,
    api,
    extraOptions,
) => {
    const result = await baseQuery(args, api, extraOptions)
    if (result.error && result.error.status === 401) {
        // Thực hiện đăng xuất
        api.dispatch(logout())
        // Hiển thị thông báo
        toastNotify("Phiên đã hết hạn.", "error")
    }
    return result
}

export default customBaseQuery
