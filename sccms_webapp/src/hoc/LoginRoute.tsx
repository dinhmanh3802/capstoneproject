import { Navigate } from "react-router-dom"
import jwt_decode from "jwt-decode"
import { SD_Role, SD_Role_Name } from "../utility/SD"

type DecodedToken = {
    role: string
}

const LoginRoute = ({ children }: { children: JSX.Element }) => {
    const token = localStorage.getItem("token")

    if (!token) {
        // Chuyển hướng đến trang đăng nhập nếu không có token
        window.location.replace("/home")
        return null
    }
    return children
}

export default LoginRoute
