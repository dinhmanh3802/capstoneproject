import React, { useState, useEffect } from "react"
import { useDownloadTemplateMutation, useBulkCreateUsersMutation } from "../../apis/userApi"
import { toastNotify } from "../../helper"
import jwtDecode from "jwt-decode"
import { Button, Alert, Form } from "react-bootstrap"
import { SD_Role_Name } from "../../utility/SD"

interface BulkCreateUserProps {
    onClose: () => void
}

function BulkCreateUser({ onClose }: BulkCreateUserProps) {
    const [file, setFile] = useState<File | null>(null)
    const [errors, setErrors] = useState<string[]>([])
    const [creatorRole, setCreatorRole] = useState<string | undefined>()
    const [downloadTemplate] = useDownloadTemplateMutation()
    const [bulkCreateUsers, { isLoading }] = useBulkCreateUsersMutation()

    useEffect(() => {
        const token = localStorage.getItem("token")
        if (token) {
            try {
                const decoded: { role: string } = jwtDecode(token)
                setCreatorRole(decoded.role)
            } catch (error) {
                console.error("Lỗi khi giải mã token:", error)
            }
        }
    }, [])

    const handleDownloadTemplate = async () => {
        try {
            const blob = await downloadTemplate().unwrap()
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement("a")
            link.href = url
            link.setAttribute("download", "UserTemplate.xlsx")
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
        } catch (error) {
            console.error("Lỗi khi tải file mẫu:", error)
            toastNotify("Không thể tải file mẫu.", "error")
        }
    }

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            setFile(e.target.files[0]) // Cập nhật file mới
            setErrors([]) // Xóa lỗi cũ khi chọn file mới
        }
    }

    const handleFileClick = (e: React.MouseEvent<HTMLInputElement, MouseEvent>) => {
        setFile(null) // Xóa file trong state khi người dùng nhấn vào input
        e.currentTarget.value = "" // Reset giá trị của input để có thể chọn lại file
    }

    const handleConfirm = async () => {
        setErrors([]) // Xóa lỗi trước khi bắt đầu xác nhận
        if (!file) {
            toastNotify("Vui lòng chọn file trước khi xác nhận.", "error")
            return
        }

        // Xác định roleId dựa trên creatorRole
        let roleId: number
        if (creatorRole === SD_Role_Name.ADMIN) {
            roleId = 2 // manager
        } else if (creatorRole === SD_Role_Name.MANAGER) {
            roleId = 4 // thu ky
        } else {
            toastNotify("Không có quyền tạo người dùng mới.", "error")
            return
        }

        const formData = new FormData()
        formData.append("file", file)

        try {
            const response: any = await bulkCreateUsers(formData)
            if (response.data?.isSuccess) {
                toastNotify("Tạo người dùng hàng loạt thành công.", "success")
                onClose()
            } else if (response.error && response.error.data && response.error.data.errorMessages) {
                // Xử lý lỗi trả về từ API với status code 400
                setErrors(response.error.data.errorMessages)
            } else {
                toastNotify("Vui lòng chọn lại File.", "error")
            }
        } catch (error: any) {
            // Xử lý các lỗi kỹ thuật khác (nếu có)
            toastNotify("Đã có lỗi xảy ra.", "error")
            console.error(error)
        }
    }

    return (
        <div>
            <div className="mb-3 d-flex align-items-center">
                <Button variant="primary" onClick={handleDownloadTemplate} disabled={isLoading} className="me-3">
                    Tải file mẫu
                </Button>
                <Form.Group controlId="formFile" className="mb-0">
                    <Form.Label className="mb-0">
                        <Button variant="secondary" as="span">
                            Chọn File
                        </Button>
                    </Form.Label>
                    <Form.Control
                        type="file"
                        accept=".xlsx, .xls"
                        onChange={handleFileChange}
                        onClick={handleFileClick}
                        style={{ display: "none" }}
                    />
                    {file && <span className="ms-2">{file.name}</span>}
                </Form.Group>
            </div>
            {errors.length > 0 && (
                <Alert variant="danger">
                    {errors?.map((error, index) => (
                        <div key={index} dangerouslySetInnerHTML={{ __html: error }} />
                    ))}
                </Alert>
            )}
            <div className="text-end">
                <Button variant="secondary" onClick={onClose} className="me-2">
                    Hủy
                </Button>
                <Button variant="primary" onClick={handleConfirm} disabled={isLoading}>
                    {isLoading ? "Đang xử lý..." : "Xác nhận"}
                </Button>
            </div>
        </div>
    )
}

export default BulkCreateUser
