// ErrorBoundary.jsx
import React from "react"
import { ErrorBoundary as ReactErrorBoundary } from "react-error-boundary"
import { useNavigate } from "react-router-dom"

const ErrorFallback = ({ error, resetErrorBoundary }) => {
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

const ErrorBoundary = ({ children }) => {
    return (
        <ReactErrorBoundary
            FallbackComponent={ErrorFallback}
            onReset={() => {
                // Bạn có thể thực hiện các hành động khi reset boundary nếu cần
            }}
        >
            {children}
        </ReactErrorBoundary>
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

export default ErrorBoundary
