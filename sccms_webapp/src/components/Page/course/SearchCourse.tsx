import React, { useState } from "react"
import { SD_CourseStatus } from "../../../utility/SD"
import { button } from "../../../utility/Label"
import { Accordion, Dropdown } from "react-bootstrap"
import Select from "react-select"
import { inputHelper } from "../../../helper"

// Tạo một mảng options chứa các trạng thái khóa tu
const options: { value: any; label: string }[] = [
    { value: "", label: "Tất cả" },
    { value: SD_CourseStatus.notStarted, label: "Chưa bắt đầu" },
    { value: SD_CourseStatus.recruiting, label: "Đang tuyển sinh" },
    { value: SD_CourseStatus.inProgress, label: "Đang diễn ra" },
    { value: SD_CourseStatus.closed, label: "Đã đóng" },
]
const initialFormData = {
    courseName: "",
    courseStatus: " ",
    startDate: "",
    endDate: "",
}
function SearchCourse({ onSearch }: { onSearch: any }) {
    const [formData, setFormData] = useState(initialFormData)

    // Hàm xử lý khi người dùng nhập dữ liệu vào ô input
    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const tempData = inputHelper(e, formData)
        setFormData(tempData)
    }

    // Hàm xử lý khi người dùng ấn nút tìm kiếm, truyền vào hàm onSearch của component SearchCourse
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onSearch(formData)
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
                            <div className="col-md-8 mb-3">
                                <label htmlFor="courseName" className="form-label">
                                    Tên
                                </label>
                                <input
                                    type="text"
                                    value={formData.courseName}
                                    className="form-control"
                                    placeholder="Tên khóa tu..."
                                    name="courseName"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="status" className="form-label">
                                    Trạng thái
                                </label>
                                <select
                                    id="status"
                                    name="courseStatus"
                                    className="form-control"
                                    value={formData.courseStatus}
                                    onChange={handleUserInput}
                                >
                                    {options?.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>

                        <div className="row">
                            <div className="col-md-4 mb-3">
                                <label htmlFor="startDate" className="form-label">
                                    Từ ngày
                                </label>
                                <input
                                    type="date"
                                    value={formData.startDate}
                                    className="form-control"
                                    name="startDate"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="endDate" className="form-label">
                                    Đến ngày
                                </label>
                                <input
                                    type="date"
                                    value={formData.endDate}
                                    className="form-control"
                                    name="endDate"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label htmlFor="endDate" className="form-label">
                                    <span style={{ visibility: "hidden" }}>.</span>
                                </label>
                                <div className="text-end">
                                    <button type="submit" className="btn btn-sm btn-primary" style={{ margin: "5px" }}>
                                        <i className="bi bi-search me-2"></i>
                                        {button.search}
                                    </button>
                                    <button type="reset" className="btn btn-sm btn-secondary ms-2">
                                        <i className="bi bi-x-lg me-2"></i>Xoá
                                    </button>
                                </div>
                            </div>
                        </div>
                    </form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default SearchCourse
