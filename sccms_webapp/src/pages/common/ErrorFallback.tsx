// ErrorFallback.jsx
import React from "react"
import { useNavigate, useRouteError } from "react-router-dom"

const ErrorFallback = () => {
    const error = useRouteError()
    const navigate = useNavigate()

    const handleGoHome = () => {
        navigate("/")
    }

    return (
        <div>
            <h1>Đã xảy ra lỗi.</h1>
            <button onClick={handleGoHome} style={styles.button}>
                Quay về trang chủ
            </button>
        </div>
    )
}

const styles = {
    container: {
        textAlign: "center",
        padding: "50px",
    },
    button: {
        padding: "10px 20px",
        fontSize: "16px",
        cursor: "pointer",
    },
}

export default ErrorFallback
