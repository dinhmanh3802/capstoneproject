import React from "react"
import { useNavigate } from "react-router-dom"

function NotFound() {
    const navigate = useNavigate()

    const handleGoHome = () => {
        navigate("/")
    }

    return (
        <div className="container d-flex flex-column justify-content-center align-items-center vh-100">
            <i className="bi bi-exclamation-triangle-fill text-danger" style={{ fontSize: "6rem" }}></i>
            <h1 className="display-4 mt-4">404 - Trang Không Tìm Thấy</h1>
            <p className="lead text-center">
                Rất tiếc! Trang bạn đang tìm kiếm không tồn tại hoặc không có quyền truy cập.
            </p>
            <button className="btn btn-primary mt-3" onClick={handleGoHome}>
                <i className="bi bi-house-door-fill me-2"></i> Về Trang Chủ
            </button>
        </div>
    )
}

export default NotFound
