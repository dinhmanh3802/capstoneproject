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

const genderOptions = [
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

// Tạo component StudentSearch
function StudentSearch({
    onSearch,
    courseList,
    currentCourse,
    studentGroupList,
}: {
    onSearch: any
    courseList: any
    currentCourse: any
    studentGroupList: any
}) {
    const navigate = useNavigate()
    const location = useLocation()

    // Tạo các option cho khóa tu và nhóm sinh viên
    const courseOptions = courseList?.map((course: any) => ({
        value: course.id,
        label: course.courseName,
    }))

    const studentGroupOptions = studentGroupList?.map((group: any) => ({
        value: group.id,
        label: group.groupName,
    }))

    // Hàm để lấy giá trị khởi tạo từ query parameters
    const getInitialFormData = () => {
        const searchParams = new URLSearchParams(location.search)
        const initialData: any = {
            courseId: searchParams.get("courseId")
                ? parseInt(searchParams.get("courseId")!)
                : currentCourse
                ? currentCourse.id
                : "",
            studentCode: searchParams.get("studentCode") || "",
            name: searchParams.get("name") || "",
            phoneNumber: searchParams.get("phoneNumber") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            studentGroup: searchParams.get("studentGroup") ? parseInt(searchParams.get("studentGroup")!) : "",
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

        // Nếu currentCourse được cập nhật và không có trong query params, cập nhật courseId
        if (currentCourse && !location.search.includes("courseId")) {
            updatedFormData.courseId = currentCourse.id
        }

        setFormData(updatedFormData)
        onSearch(updatedFormData)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search, currentCourse])

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
        navigate(`/students?${queryParams}`)

        // Thực hiện tìm kiếm
        onSearch(formData)
    }

    const handleReset = () => {
        const resetFormData = {
            courseId: currentCourse ? currentCourse.id : "",
            studentCode: "",
            name: "",
            phoneNumber: "",
            status: "",
            studentGroup: "",
            gender: "",
            dateOfBirthFrom: "",
            dateOfBirthTo: "",
        }
        setFormData(resetFormData)
        navigate(`/students`)
        onSearch(resetFormData)
    }

    // Tránh render form nếu currentCourse chưa được tải
    if (!currentCourse) {
        return null
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
                            <div className="col-md-6 mb-3">
                                <label className="form-label">Khóa tu</label>
                                <Select
                                    name="courseId"
                                    options={courseOptions}
                                    onChange={handleSelect("courseId")}
                                    isClearable
                                    placeholder="Chọn khóa tu"
                                    value={
                                        courseOptions?.find((option: any) => option.value === formData.courseId) || null
                                    }
                                />
                            </div>
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
                        </div>
                        <div className="row">
                            <div className="col-md-3 mb-3">
                                <label className="form-label">SĐT</label>
                                <input
                                    type="text"
                                    value={formData.phoneNumber}
                                    className="form-control"
                                    name="phoneNumber"
                                    placeholder="Số điện thoại..."
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Giới tính</label>
                                <Select
                                    name="gender"
                                    options={genderOptions}
                                    onChange={handleSelect("gender")}
                                    isClearable
                                    placeholder="Giới tính"
                                    value={
                                        genderOptions?.find((option: any) => option.value === formData.gender) || null
                                    }
                                />
                            </div>
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
                        </div>
                        <div className="row">
                            <div className="col-md-3 mb-3">
                                <label className="form-label">Chánh</label>
                                <Select
                                    name="studentGroup"
                                    options={studentGroupOptions}
                                    onChange={handleSelect("studentGroup")}
                                    isClearable
                                    placeholder="Chọn chánh"
                                    value={
                                        studentGroupOptions?.find(
                                            (option: any) => option.value === formData.studentGroup,
                                        ) || null
                                    }
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
                    </form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default StudentSearch
