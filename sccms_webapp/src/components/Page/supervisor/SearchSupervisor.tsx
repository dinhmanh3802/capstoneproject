// src/components/Page/Supervisor/SearchSupervisor.tsx

import React, { useState, useEffect } from "react"
import { SD_Gender, SD_UserStatus } from "../../../utility/SD"
import { button } from "../../../utility/Label"
import { Accordion } from "react-bootstrap"
import Select from "react-select"
import { inputHelper } from "../../../helper"
import { SingleValue } from "react-select"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"

interface SearchSupervisorProps {
    onSearch: (params: any) => void
    currentCourse: any
}

const genderOptions: { value: any; label: string }[] = [
    { value: "", label: "Tất cả" },
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

function SearchSupervisor({ onSearch, currentCourse }: SearchSupervisorProps) {
    const [formData, setFormData] = useState({
        name: "",
        phoneNumber: "",
        email: "",
        gender: "",
    })

    // Hàm xử lý nhập liệu từ các trường input
    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const tempData = inputHelper(e, formData)
        setFormData(tempData)
    }

    // Hàm xử lý reset form và tìm kiếm lại với các giá trị mặc định
    const handleReset = () => {
        setFormData({
            name: "",
            phoneNumber: "",
            email: "",
            gender: "",
        })
        onSearch({
            courseId: currentCourse?.id ? parseInt(currentCourse.id) : 0,
            name: "",
            phoneNumber: "",
            email: "",
            gender: "",
        })
    }

    // Hàm xử lý submit form tìm kiếm
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onSearch({
            courseId: currentCourse?.id ? parseInt(currentCourse.id) : 0,
            ...formData,
            status: SD_UserStatus.ACTIVE, // Đặt trạng thái luôn là "Hoạt động"
        })
    }

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
                            <div className="col-md-3 mb-3">
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
                            <div className="col-md-3 mb-3">
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
                            <div className="col-md-3 mb-3">
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
                            <div className="col-md-3 mb-3">
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

                        {/* Nút tìm kiếm và xóa */}
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

export default SearchSupervisor
