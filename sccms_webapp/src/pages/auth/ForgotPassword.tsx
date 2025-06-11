import React, { useState } from "react"
import { useForgotPasswordMutation, useVerifyOtpMutation, useResetPasswordMutation } from "../../apis/AuthApi"
import logo from "../../assets/img/logo.png"
import { MainLoader } from "../../components/Page"
import { Link, useNavigate } from "react-router-dom"
import { toastNotify } from "../../helper"

const ForgotPassword = () => {
    const [email, setEmail] = useState("")
    const [otp, setOtp] = useState(["", "", "", ""]) // 4 chữ số OTP
    const [newPassword, setNewPassword] = useState("")
    const [confirmPassword, setConfirmPassword] = useState("") // Thêm trường xác nhận mật khẩu
    const [isEmailSent, setIsEmailSent] = useState(false)
    const [isOtpVerified, setIsOtpVerified] = useState(false)
    const [forgotPassword] = useForgotPasswordMutation()
    const [verifyOtp] = useVerifyOtpMutation()
    const [resetPassword] = useResetPasswordMutation()
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState({ email: "", otp: "", newPassword: "", confirmPassword: "" })
    const navigate = useNavigate()

    const handleForgotPassword = async () => {
        setLoading(true)
        setError({ ...error, email: "" })
        try {
            await forgotPassword(email).unwrap()
            toastNotify("OTP đã được gửi đến email của bạn.", "success")
            setIsEmailSent(true)
        } catch (err) {
            toastNotify("Không thể gửi OTP.", "error")
            setError({ ...error, email: "Không thể gửi OTP." })
        }
        setLoading(false)
    }

    const handleVerifyOtp = async () => {
        setLoading(true)
        setError({ ...error, otp: "" })
        try {
            await verifyOtp({ email, otp: otp.join("") }).unwrap()
            toastNotify("OTP xác minh thành công.", "success")
            setIsOtpVerified(true)
        } catch (err) {
            toastNotify("OTP không hợp lệ hoặc đã hết hạn.", "error")
            setError({ ...error, otp: "OTP không hợp lệ hoặc đã hết hạn." })
        }
        setLoading(false)
    }

    const handleResetPassword = async () => {
        setLoading(true)
        setError({ ...error, newPassword: "", confirmPassword: "" })

        // Kiểm tra xác nhận mật khẩu
        if (newPassword !== confirmPassword) {
            setError((prev) => ({ ...prev, confirmPassword: "Mật khẩu xác nhận không khớp." }))
            setLoading(false)
            return
        }

        // Kiểm tra chất lượng mật khẩu
        const passwordError = validatePassword(newPassword)
        if (passwordError) {
            setError((prev) => ({ ...prev, newPassword: passwordError }))
            setLoading(false)
            return
        }

        try {
            await resetPassword({ email, newPassword }).unwrap()
            toastNotify("Mật khẩu đã được thay đổi.")
            navigate("/auth/login")
        } catch (err) {
            setError({ ...error, newPassword: "Có lỗi xảy ra khi đặt mật khẩu." })
        }
        setLoading(false)
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

    const handleOtpChange = (index: number, value: string) => {
        const newOtp = [...otp]
        newOtp[index] = value
        setOtp(newOtp)
        if (value && index < otp.length - 1) {
            const nextInput = document.getElementById(`otp-${index + 1}`)
            if (nextInput) {
                nextInput.focus()
            }
        }
        setError({ ...error, otp: "" })
    }

    return (
        <div>
            {loading && <MainLoader />}
            {!isEmailSent ? (
                <>
                    <div className="mb-3">
                        <label htmlFor="email" className="form-label">
                            Email
                        </label>
                        <input
                            type="email"
                            id="email"
                            className={`form-control ${error.email ? "is-invalid" : ""}`}
                            placeholder="Nhập email của bạn"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                        {error.email && <div className="invalid-feedback">{error.email}</div>}
                    </div>

                    <div className="d-grid mb-3">
                        <button className="btn btn-primary" onClick={handleForgotPassword}>
                            Gửi OTP
                        </button>
                    </div>
                </>
            ) : (
                <>
                    {!isOtpVerified && ( // Chỉ hiển thị phần nhập OTP nếu chưa xác minh
                        <div className="mb-3">
                            <h6>Vui lòng nhập mã OTP đã được gửi qua Email.</h6>
                            <div className="d-flex justify-content-between mb-2">
                                {otp.map((_, idx) => (
                                    <input
                                        key={idx}
                                        type="text"
                                        id={`otp-${idx}`}
                                        className={`form-control text-center`}
                                        maxLength={1}
                                        value={otp[idx]}
                                        onChange={(e) => handleOtpChange(idx, e.target.value)}
                                        style={{ width: "60px", height: "60px", fontSize: "24px" }}
                                    />
                                ))}
                            </div>
                            {error.otp && <div className="text-danger">{error.otp}</div>}
                        </div>
                    )}

                    {isOtpVerified ? (
                        <>
                            <div className="mb-3">
                                <label htmlFor="newPassword" className="form-label">
                                    Mật khẩu mới
                                </label>
                                <input
                                    type="password"
                                    id="newPassword"
                                    className={`form-control ${error.newPassword ? "is-invalid" : ""}`}
                                    placeholder="Mật khẩu mới"
                                    value={newPassword}
                                    onChange={(e) => setNewPassword(e.target.value)}
                                />
                                {error.newPassword && <div className="invalid-feedback">{error.newPassword}</div>}
                            </div>

                            <div className="mb-3">
                                <label htmlFor="confirmPassword" className="form-label">
                                    Xác nhận mật khẩu mới
                                </label>
                                <input
                                    type="password"
                                    id="confirmPassword"
                                    className={`form-control ${error.confirmPassword ? "is-invalid" : ""}`}
                                    placeholder="Xác nhận mật khẩu mới"
                                    value={confirmPassword}
                                    onChange={(e) => setConfirmPassword(e.target.value)}
                                />
                                {error.confirmPassword && (
                                    <div className="invalid-feedback">{error.confirmPassword}</div>
                                )}
                            </div>

                            <div className="d-grid">
                                <button className="btn btn-success" onClick={handleResetPassword}>
                                    Đặt mật khẩu mới
                                </button>
                            </div>
                        </>
                    ) : (
                        <>
                            <div className="d-grid mb-3 mt-4">
                                <button className="btn btn-success" onClick={handleVerifyOtp}>
                                    Xác Minh OTP
                                </button>
                            </div>

                            <div className="d-grid mb-3">
                                <button
                                    className="btn btn-secondary"
                                    onClick={() => {
                                        setIsEmailSent(false) // Quay lại bước nhập email
                                        setOtp(["", "", "", ""]) // Reset OTP
                                    }}
                                >
                                    Quay lại nhập email
                                </button>
                            </div>
                        </>
                    )}
                </>
            )}
            <div className="text-end mt-3">
                <Link to="/auth/login">
                    <strong>Quay lại Đăng Nhập</strong>
                </Link>
            </div>
        </div>
    )
}

export default ForgotPassword
