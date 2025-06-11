import React from "react"
import "./ConfirmationPopup.css"

interface ConfirmationPopupProps {
    isOpen: boolean
    onClose: () => void
    onConfirm: () => void
    message: string
    title?: string
}

const ConfirmationPopup: React.FC<ConfirmationPopupProps> = ({
    isOpen,
    onClose,
    onConfirm,
    message,
    title = "Xác nhận",
}) => {
    if (!isOpen) return null

    return (
        <div className="popup-overlay">
            <div className="popup-wrapper">
                <div className="popup-header">
                    <h5 className="popup-title">{title}</h5>
                    <button type="button" className="close-button" onClick={onClose} aria-label="Đóng">
                        &times;
                    </button>
                </div>
                <div className="popup-body">
                    <p dangerouslySetInnerHTML={{ __html: message }} />
                </div>
                <div className="popup-footer">
                    <button type="button" className="btn btn-secondary" onClick={onClose}>
                        Hủy
                    </button>
                    <button type="button" className="btn btn-primary" onClick={onConfirm}>
                        Xác nhận
                    </button>
                </div>
            </div>
        </div>
    )
}

export default ConfirmationPopup
