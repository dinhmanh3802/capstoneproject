import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { MainLoader } from "../../components/Page"
import { useChangePasswordMutation } from "../../apis/userApi"
import jwtDecode from "jwt-decode"
import { authModel } from "../../interfaces"
import apiResponse from "../../interfaces/apiResponse"
import { toastNotify } from "../../helper"

function ChangePassword() {
    const navigate = useNavigate()
    const [loading, setLoading] = useState(false)
    const [inputErrors, setInputErrors] = useState({
        oldPassword: "",
        newPassword: "",
        confirmNewPassword: "",
    })
    const [changePassword] = useChangePasswordMutation()

    const [passwordInput, setPasswordInput] = useState({
        oldPassword: "",
        newPassword: "",
        confirmNewPassword: "",
    })

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setPasswordInput((prev) => ({ ...prev, [name]: value }))
        setInputErrors((prev) => ({ ...prev, [name]: "" })) // Reset error for the specific field
    }

    const validatePassword = (password: string) => {
        const minLength = 8 // Độ dài tối thiểu
        const hasUpperCase = /[A-Z]/.test(password) // Có chữ hoa
        const hasLowerCase = /[a-z]/.test(password) // Có chữ thường
        const hasNumbers = /\d/.test(password) // Có số
        const hasSpecialChars = /[!@#$%^&*(),.?":{}|<>]/.test(password) // Có ký tự đặc biệt

        if (password.length < minLength) {
            return "Mật khẩu phải có ít nhất 8 ký tự."
        }
        if (!hasUpperCase) {
            return "Mật khẩu phải có ít nhất 1 chữ hoa."
        }
        if (!hasLowerCase) {
            return "Mật khẩu phải có ít nhất 1 chữ thường."
        }
        if (!hasNumbers) {
            return "Mật khẩu phải có ít nhất 1 số."
        }
        if (!hasSpecialChars) {
            return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt."
        }
        return "" // Không có lỗi
    }

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        setLoading(true)
        setInputErrors({ oldPassword: "", newPassword: "", confirmNewPassword: "" }) // Reset all input errors

        // Kiểm tra mật khẩu mới và xác nhận
        if (passwordInput.newPassword !== passwordInput.confirmNewPassword) {
            setInputErrors((prev) => ({ ...prev, confirmNewPassword: "Mật khẩu xác nhận không khớp." }))
            setLoading(false)
            return
        }

        // Validate mật khẩu mới
        const passwordError = validatePassword(passwordInput.newPassword)
        if (passwordError) {
            setInputErrors((prev) => ({ ...prev, newPassword: passwordError }))
            setLoading(false)
            return
        }

        const token = localStorage.getItem("token")
        let userId

        if (token) {
            try {
                const decoded: authModel = jwtDecode(token)
                userId = decoded.userId
            } catch (error) {
                setInputErrors((prev) => ({ ...prev, oldPassword: "Đã có lỗi xảy ra khi xác thực người dùng." }))
                setLoading(false)
                return
            }
        } else {
            setInputErrors((prev) => ({ ...prev, oldPassword: "Bạn cần đăng nhập để thay đổi mật khẩu." }))
            setLoading(false)
            return
        }

        // Gọi API thay đổi mật khẩu
        const response: apiResponse = await changePassword({
            userId,
            body: {
                oldPassword: passwordInput.oldPassword,
                newPassword: passwordInput.newPassword,
                confirmPassword: passwordInput.confirmNewPassword,
            },
        })

        // Xử lý phản hồi
        if (response.data) {
            toastNotify("Đổi mật khẩu thành công", "success")
            navigate("/") // Chuyển về trang chính
        } else if (response.error) {
            const errorMessage = response.error.data?.errorMessages[0] || "Đã có lỗi xảy ra."
            if (errorMessage.includes("Mật khẩu cũ không chính xác.")) {
                setInputErrors((prev) => ({ ...prev, oldPassword: errorMessage })) // Hiển thị lỗi cho mật khẩu cũ
            } else {
                toastNotify(errorMessage, "error") // Hiển thị lỗi chung
            }
        }

        setLoading(false)
    }

    return (
        <div className="container mt-5">
            <h2 className="mb-4" style={{ fontWeight: "bold" }}>
                Thay Đổi Mật Khẩu
            </h2>

            {loading && <MainLoader />}

            <div className="card shadow p-4 rounded w-50">
                <form onSubmit={handleSubmit}>
                    <div className="mb-3">
                        <label htmlFor="oldPassword" className="form-label">
                            Mật khẩu cũ
                        </label>
                        <input
                            type="password"
                            className={`form-control ${inputErrors.oldPassword ? "is-invalid" : ""}`}
                            id="oldPassword"
                            name="oldPassword"
                            value={passwordInput.oldPassword}
                            onChange={handleInputChange}
                            required
                        />
                        {inputErrors.oldPassword && <div className="invalid-feedback">{inputErrors.oldPassword}</div>}
                    </div>
                    <div className="mb-3">
                        <label htmlFor="newPassword" className="form-label">
                            Mật khẩu mới
                        </label>
                        <input
                            type="password"
                            className={`form-control ${inputErrors.newPassword ? "is-invalid" : ""}`}
                            id="newPassword"
                            name="newPassword"
                            value={passwordInput.newPassword}
                            onChange={handleInputChange}
                            required
                        />
                        {inputErrors.newPassword && <div className="invalid-feedback">{inputErrors.newPassword}</div>}
                    </div>
                    <div className="mb-3">
                        <label htmlFor="confirmNewPassword" className="form-label">
                            Xác nhận mật khẩu mới
                        </label>
                        <input
                            type="password"
                            className={`form-control ${inputErrors.confirmNewPassword ? "is-invalid" : ""}`}
                            id="confirmNewPassword"
                            name="confirmNewPassword"
                            value={passwordInput.confirmNewPassword}
                            onChange={handleInputChange}
                            required
                        />
                        {inputErrors.confirmNewPassword && (
                            <div className="invalid-feedback">{inputErrors.confirmNewPassword}</div>
                        )}
                    </div>
                    <div className="d-flex justify-content-end">
                        <button type="button" className="btn btn-secondary w-25 me-2" onClick={() => navigate("/")}>
                            Quay lại
                        </button>
                        <button type="submit" className="btn btn-primary w-25">
                            Đổi Mật Khẩu
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}

export default ChangePassword
