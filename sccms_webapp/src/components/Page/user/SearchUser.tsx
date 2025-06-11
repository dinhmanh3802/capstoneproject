// src/components/Page/user/SearchUser.tsx
import React, { useState, useEffect } from "react"
import { SD_Gender, SD_Role, SD_UserStatus } from "../../../utility/SD"
import { button } from "../../../utility/Label"
import { Accordion } from "react-bootstrap"
import { inputHelper } from "../../../helper"

interface SearchUserProps {
    onSearch: any
    excludeAdmin?: boolean
    initialValues?: {
        name: string
        phoneNumber: string
        email: string
        role: string
        status: string
        gender: string
    }
}

const statusOptions: { value: any; label: string }[] = [
    { value: "", label: "Tất cả" },
    { value: SD_UserStatus.ACTIVE, label: "Hoạt động" },
    { value: SD_UserStatus.DEACTIVE, label: "Không hoạt động" },
]

const genderOptions: { value: any; label: string }[] = [
    { value: "", label: "Tất cả" },
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

const initialFormData = {
    name: "",
    phoneNumber: "",
    email: "",
    role: "",
    status: "",
    gender: "",
}

function SearchUser({ onSearch, excludeAdmin = false, initialValues }: SearchUserProps) {
    const [formData, setFormData] = useState(initialFormData)

    useEffect(() => {
        if (initialValues) {
            setFormData(initialValues)
        }
    }, [initialValues])

    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const tempData = inputHelper(e, formData)
        setFormData(tempData)
    }

    const handleReset = () => {
        setFormData(initialFormData)
        onSearch(initialFormData) // Reset tìm kiếm
    }

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onSearch(formData)
    }

    const roleOptions: { value: any; label: string }[] = [
        { value: "", label: "Tất cả" },
        ...(excludeAdmin ? [] : [{ value: SD_Role.ADMIN, label: "Quản trị viên" }]),
        { value: SD_Role.MANAGER, label: "Quản lý" },
        { value: SD_Role.SECRETARY, label: "Thư ký" },
        { value: SD_Role.STAFF, label: "Nhân viên" },
        { value: SD_Role.SUPERVISOR, label: "Huynh trưởng" },
        { value: SD_Role.TEAM_LEADER, label: "Trưởng ban" },
    ]

    return (
        <Accordion>
            <Accordion.Item eventKey="0">
                <Accordion.Header>
                    <i className="bi bi-search me-2"></i>
                    {button.search}
                </Accordion.Header>
                <Accordion.Body>
                    <form onSubmit={handleSubmit}>
                        <div className="row">
                            <div className="col-md-4 mb-3">
                                <label htmlFor="name" className="form-label">
                                    Họ tên
                                </label>
                                <input
                                    type="text"
                                    value={formData.name}
                                    className="form-control"
                                    placeholder="Họ tên..."
                                    name="name"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="phoneNumber" className="form-label">
                                    Số điện thoại
                                </label>
                                <input
                                    type="text"
                                    value={formData.phoneNumber}
                                    className="form-control"
                                    placeholder="Số điện thoại..."
                                    name="phoneNumber"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="email" className="form-label">
                                    Email
                                </label>
                                <input
                                    type="text"
                                    value={formData.email}
                                    className="form-control"
                                    placeholder="Email..."
                                    name="email"
                                    onChange={handleUserInput}
                                />
                            </div>
                        </div>
                        <div className="row">
                            <div className="col-md-4 mb-3">
                                <label htmlFor="status" className="form-label">
                                    Trạng thái
                                </label>
                                <select
                                    id="status"
                                    name="status"
                                    className="form-control"
                                    value={formData.status}
                                    onChange={handleUserInput}
                                >
                                    {statusOptions?.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="role" className="form-label">
                                    Vai trò
                                </label>
                                <select
                                    id="role"
                                    name="role"
                                    className="form-control"
                                    value={formData.role}
                                    onChange={handleUserInput}
                                >
                                    {roleOptions?.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="gender" className="form-label">
                                    Giới tính
                                </label>
                                <select
                                    id="gender"
                                    name="gender"
                                    className="form-control"
                                    value={formData.gender}
                                    onChange={handleUserInput}
                                >
                                    {genderOptions?.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>
                        {/* Thêm một hàng mới cho nút */}
                        <div className="row">
                            <div className="col-md-12 mb-3 text-end">
                                <button type="submit" className="btn btn-primary me-2">
                                    <i className="bi bi-search me-2"></i>
                                    {button.search}
                                </button>
                                <button type="button" className="btn btn-secondary" onClick={handleReset}>
                                    <i className="bi bi-x-lg me-2"></i>Xóa
                                </button>
                            </div>
                        </div>
                    </form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default SearchUser
