import React from "react"
import { Modal, Button, Form } from "react-bootstrap"

interface FeedbackDetailPopupProps {
    isOpen: boolean
    onClose: () => void
    feedback: {
        submissionDate: string
        content: string
    } | null
}

const FeedbackDetailPopup: React.FC<FeedbackDetailPopupProps> = ({ isOpen, onClose, feedback }) => {
    if (!feedback) return null

    // Format the date to "dd/MM/yyyy"
    const formatDate = (dateString: string) => {
        const date = new Date(dateString)
        const day = String(date.getDate()).padStart(2, "0")
        const month = String(date.getMonth() + 1).padStart(2, "0") // Months are 0-based
        const year = date.getFullYear()
        return `${day}/${month}/${year}`
    }

    return (
        <Modal show={isOpen} onHide={onClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Chi tiết phản hồi</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group controlId="formSubmissionDate">
                        <Form.Label>Ngày phản hồi</Form.Label>
                        <Form.Control type="text" value={formatDate(feedback.submissionDate)} readOnly />
                    </Form.Group>
                    <Form.Group controlId="formContent" className="mt-3">
                        <Form.Label>Chi tiết phản hồi</Form.Label>
                        <Form.Control
                            as="textarea"
                            rows={10}
                            value={feedback.content}
                            readOnly
                            style={{ whiteSpace: "pre-wrap" }}
                        />
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onClose}>
                    Thoát
                </Button>
            </Modal.Footer>
        </Modal>
    )
}

export default FeedbackDetailPopup
