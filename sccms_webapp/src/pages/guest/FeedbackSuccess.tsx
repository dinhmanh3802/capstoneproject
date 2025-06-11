import React from "react"
import { useNavigate } from "react-router-dom"
import { Button, Container, Card } from "react-bootstrap"

const SubmitSuccess: React.FC = () => {
    const navigate = useNavigate()

    const handleGoHome = () => {
        navigate("/home")
    }

    return (
        <Container className="d-flex justify-content-center align-items-center vh-100">
            <Card style={{ maxWidth: "500px" }} className="text-center shadow-lg p-4">
                <Card.Body>
                    <Card.Title className="mb-4 text-success fw-bold" style={{ fontSize: "1.5rem" }}>
                        Bạn đã gửi phản hồi thành công! 🎉
                    </Card.Title>
                    <Card.Text className="mb-4" style={{ fontSize: "1.1rem" }}>
                        Cảm ơn bạn đã gửi phản hồi. Đóng góp của bạn rất quan trọng đối với chúng tôi.
                    </Card.Text>
                    <Button variant="primary" onClick={handleGoHome} className="px-4 py-2">
                        Trở về trang chủ
                    </Button>
                </Card.Body>
            </Card>
        </Container>
    )
}

export default SubmitSuccess
