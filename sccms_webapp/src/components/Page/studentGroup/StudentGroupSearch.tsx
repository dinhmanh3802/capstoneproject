import React, { useState, useEffect } from "react"
import { button } from "../../../utility/Label"
import { Accordion } from "react-bootstrap"
import Select from "react-select"
import { inputHelper } from "../../../helper"
import { SD_Gender, SD_ProcessStatus } from "../../../utility/SD"
import { SingleValue } from "react-select"
import { useNavigate, useLocation } from "react-router-dom"

// Tạo các options cho trạng thái và giới tính
const statusOptions = [
    { value: SD_ProcessStatus.Approved, label: "Chờ nhập học" },
    { value: SD_ProcessStatus.Enrolled, label: "Nhập học" },
    { value: SD_ProcessStatus.Graduated, label: "Tốt nghiệp" },
    { value: SD_ProcessStatus.DropOut, label: "Bỏ học" },
]

// Tạo component StudentSearch
function StudentGroupSearch({ onSearch, studentGroup }: { onSearch: any; studentGroup: any }) {
    const navigate = useNavigate()
    const location = useLocation()

    // Hàm để lấy giá trị khởi tạo từ query parameters
    const getInitialFormData = () => {
        const searchParams = new URLSearchParams(location.search)
        const initialData: any = {
            studentCode: searchParams.get("studentCode") || "",
            name: searchParams.get("name") || "",
            phone: searchParams.get("phone") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            gender: searchParams.get("gender") ? parseInt(searchParams.get("gender")!) : "",
            dateOfBirthFrom: searchParams.get("dateOfBirthFrom") || "",
            dateOfBirthTo: searchParams.get("dateOfBirthTo") || "",
        }
        return initialData
    }

    const [formData, setFormData] = useState(getInitialFormData())

    // Cập nhật formData khi URL thay đổi hoặc khi currentCourse thay đổi
    useEffect(() => {
        const updatedFormData = getInitialFormData()
        if (JSON.stringify(updatedFormData) !== JSON.stringify(formData)) {
            setFormData(updatedFormData)
            onSearch(updatedFormData)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search])

    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const tempData = inputHelper(e, formData)
        setFormData(tempData)
    }

    const handleSelect = (field: string) => (selectedOption: SingleValue<{ value: any; label: any }>) => {
        setFormData((prevData: any) => ({
            ...prevData,
            [field]: selectedOption ? selectedOption.value : "",
        }))
    }

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()

        // Chuyển đổi formData thành URL query parameters
        const queryParams = new URLSearchParams(formData as any).toString()

        // Cập nhật URL với query parameters
        navigate(`/student-groups/${studentGroup.id}?${queryParams}`, { replace: false })

        // Thực hiện tìm kiếm
        onSearch(formData)
    }

    const handleReset = () => {
        const resetFormData = {
            studentCode: "",
            name: "",
            phone: "",
            status: "",
            gender: "",
            dateOfBirthFrom: "",
            dateOfBirthTo: "",
        }
        setFormData(resetFormData)
        navigate(`/student-groups/${studentGroup.id}`, { replace: false })
        onSearch(resetFormData)
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
                                <label className="form-label">Mã khóa sinh</label>
                                <input
                                    type="text"
                                    value={formData.studentCode}
                                    className="form-control"
                                    name="studentCode"
                                    placeholder="Mã khóa sinh..."
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Tên khóa sinh</label>
                                <input
                                    type="text"
                                    value={formData.name}
                                    className="form-control"
                                    name="name"
                                    placeholder="Tên khóa sinh..."
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-3 mb-3">
                                <label className="form-label">SĐT</label>
                                <input
                                    type="text"
                                    value={formData.phone}
                                    className="form-control"
                                    name="phone"
                                    placeholder="Số điện thoại..."
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Trạng thái</label>
                                <Select
                                    name="status"
                                    options={statusOptions}
                                    onChange={handleSelect("status")}
                                    isClearable
                                    placeholder="Chọn trạng thái"
                                    value={statusOptions?.find((option) => option.value === formData.status) || null}
                                />
                            </div>
                        </div>
                        <div className="row">
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Ngày sinh từ</label>
                                <input
                                    type="date"
                                    value={formData.dateOfBirthFrom}
                                    className="form-control"
                                    name="dateOfBirthFrom"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Ngày sinh đến</label>
                                <input
                                    type="date"
                                    value={formData.dateOfBirthTo}
                                    className="form-control"
                                    name="dateOfBirthTo"
                                    onChange={handleUserInput}
                                />
                            </div>

                            <div className="col-md-6 mb-3 text-end">
                                <label htmlFor="endDate" className="form-label">
                                    <span style={{ visibility: "hidden" }}>.</span>
                                </label>
                                <div className="text-end">
                                    <button type="submit" className="btn btn-primary">
                                        {button.search}
                                    </button>
                                    <button type="button" className="btn btn-secondary ms-2" onClick={handleReset}>
                                        Xóa
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div className="row"></div>
                    </form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default StudentGroupSearch
