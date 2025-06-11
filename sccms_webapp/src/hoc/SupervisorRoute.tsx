import React from "react"
import { Navigate } from "react-router-dom"
import jwt_decode from "jwt-decode"
import { SD_Role, SD_Role_Name } from "../utility/SD"

type DecodedToken = {
    role: string
}

const SupervisorRoute = ({ children }: { children: JSX.Element }) => {
    const token = localStorage.getItem("token")

    if (!token) {
        // Chuyển hướng đến trang đăng nhập nếu không có token
        return <Navigate to="/home" replace />
    }

    try {
        const decoded: DecodedToken = jwt_decode(token)
        // Kiểm tra vai trò admin hoặc manager hoặc secretary
        if (
            decoded.role !== SD_Role_Name.ADMIN &&
            decoded.role !== SD_Role_Name.MANAGER &&
            decoded.role !== SD_Role_Name.SECRETARY &&
            decoded.role !== SD_Role_Name.SUPERVISOR
        ) {
            // Chuyển hướng đến trang không có quyền truy cập
            return <Navigate to="/accessDenied" replace />
        }

        // Nếu là admin, render component con
        return children
    } catch (error) {
        console.error("Token không hợp lệ:", error)
        return <Navigate to="/home" replace />
    }
}

export default SupervisorRoute
