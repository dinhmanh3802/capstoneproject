// src/utils/toastNotify.ts

import { toast, TypeOptions, ToastOptions } from "react-toastify"

const toastNotify = (message: string | React.ReactNode, type: TypeOptions = "success", options?: ToastOptions) => {
    toast(message, {
        type: type,
        position: "top-right",
        autoClose: 4000, // Mặc định tự động đóng sau 2000ms
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "dark",
        ...options, // Cho phép ghi đè các tùy chọn nếu có
    })
}

export default toastNotify
