import React, { useState, useEffect } from "react"
import { useDispatch } from "react-redux"
import { Link, useNavigate } from "react-router-dom"
import jwtDecode from "jwt-decode"
import { setCredentials } from "../../store/slice/authSlice" // Cập nhật import cho setCredentials
import apiResponse from "../../interfaces/apiResponse"
import { authModel } from "../../interfaces"
import { fieldLabels, button } from "../../utility/Label"
import { useLoginUserMutation } from "../../apis/AuthApi"
import { MainLoader } from "../../components/Page"

const Login = () => {
    const dispatch = useDispatch()
    const navigate = useNavigate()
    const [userInput, setUserInput] = useState({ username: "", password: "" })
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState("")
    const [loginUser] = useLoginUserMutation()

    // Kiểm tra xem có token hợp lệ trong localStorage không, nếu có thì chuyển hướng
    useEffect(() => {
        const token = localStorage.getItem("token")
        if (token) {
            try {
                const { userId } = jwtDecode(token) as authModel
                if (userId) {
                    navigate("/")
                }
            } catch (error) {
                localStorage.removeItem("token")
            }
        }
    }, [navigate])

    // Cập nhật giá trị input của người dùng
    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setUserInput({ ...userInput, [name]: value })
    }

    // Xử lý khi người dùng submit form đăng nhập
    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        setLoading(true)
        setError("")

        const response: apiResponse = await loginUser(userInput)

        if (response.data) {
            const { token } = response.data.result
            localStorage.setItem("token", token)
            const { userId, username, role }: authModel = jwtDecode(token)
            // Lưu token và thông tin người dùng vào Redux store
            dispatch(setCredentials({ token, user: { userId, username, role } }))
            navigate("/")
        } else if (response.error) {
            setError(response.error.data.errorMessages[0])
        }
        setLoading(false)
    }

    return (
        <form onSubmit={handleSubmit}>
            {loading && <MainLoader />}
            <h5 className="mb-3 fw-bold text-muted text-center">Chào mừng bạn tới với hệ thống</h5>
            {error && <p className="text-danger text-center small">{error}</p>}

            {/* Input tên đăng nhập */}
            <div className="form-floating mb-3">
                <input
                    type="text"
                    className="form-control"
                    id="username"
                    name="username"
                    placeholder="Tên đăng nhập"
                    value={userInput.username}
                    onChange={handleUserInput}
                    required
                    autoComplete="off"
                />
                <label htmlFor="username">{fieldLabels.username}</label>
            </div>

            {/* Input mật khẩu */}
            <div className="form-floating mb-3">
                <input
                    type="password"
                    className="form-control"
                    id="password"
                    name="password"
                    placeholder="Mật khẩu"
                    value={userInput.password}
                    onChange={handleUserInput}
                    required
                />
                <label htmlFor="password">{fieldLabels.password}</label>
            </div>

            {/* Link tới quên mật khẩu */}
            <div className="text-end mb-3">
                <Link className="text-decoration-none h6" to="/auth/forgot-password">
                    Quên mật khẩu?
                </Link>
            </div>

            {/* Nút đăng nhập */}
            <div className="d-grid">
                <button type="submit" className="btn btn-primary btn-lg">
                    {button.login}
                </button>
            </div>
        </form>
    )
}

export default Login
