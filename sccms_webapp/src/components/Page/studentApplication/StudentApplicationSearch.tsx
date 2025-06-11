import React, { useState, useEffect } from "react"
import { button } from "../../../utility/Label"
import { Accordion } from "react-bootstrap"
import Select from "react-select"
import { inputHelper } from "../../../helper"
import { SD_Gender, SD_ProcessStatus, SD_Role_Name } from "../../../utility/SD"
import { SingleValue } from "react-select"
import { useNavigate, useLocation } from "react-router-dom"

// Tạo các options cho trạng thái và giới tính
const statusOptions = [
    { value: SD_ProcessStatus.Pending, label: "Đang chờ" },
    { value: SD_ProcessStatus.Approved, label: "Chấp nhận" },
    { value: SD_ProcessStatus.Rejected, label: "Từ chối" },
]

const genderOptions = [
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

// Tạo component StudentApplicationSearch
function StudentApplicationSearch({
    onSearch,
    courseList,
    currentCourse,
    secretaryList,
    currentUserRole,
    currentUserId,
}: {
    onSearch: any
    courseList: any
    currentCourse: any
    secretaryList: any
    currentUserRole: any
    currentUserId: any
}) {
    const navigate = useNavigate()
    const location = useLocation()

    // Tạo các option cho khóa tu và người duyệt
    const courseOptions = courseList?.map((course: any) => ({
        value: course.id,
        label: course.courseName,
    }))
    const reviewerOptions = secretaryList?.map((secretary: any) => ({
        value: secretary.id,
        label: secretary.userName,
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
            name: searchParams.get("name") || "",
            phoneNumber: searchParams.get("phoneNumber") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            reviewerId: searchParams.get("reviewerId") ? parseInt(searchParams.get("reviewerId")!) : "",
            gender: searchParams.get("gender") ? parseInt(searchParams.get("gender")!) : "",
            parentName: searchParams.get("parentName") || "",
            birthDateFrom: searchParams.get("birthDateFrom") || "",
            birthDateTo: searchParams.get("birthDateTo") || "",
            nationalId: searchParams.get("nationalId") || "",
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
    }, [location.search, currentCourse, secretaryList, currentUserRole, currentUserId])

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
        navigate(`/student-applications?${queryParams}`)

        // Thực hiện tìm kiếm
        onSearch(formData)
    }

    const handleReset = () => {
        const resetFormData = {
            courseId: currentCourse ? currentCourse.id : "",
            name: "",
            phoneNumber: "",
            status: "",
            reviewerId: currentUserRole === SD_Role_Name.SECRETARY ? currentUserId : "",
            gender: "",
            parentName: "",
            birthDateFrom: "",
            birthDateTo: "",
        }
        setFormData(resetFormData)
        navigate(`/student-applications`)
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
                                <label className="form-label">Tên phụ huynh</label>
                                <input
                                    type="text"
                                    value={formData.parentName}
                                    className="form-control"
                                    name="parentName"
                                    placeholder="Tên phụ huynh..."
                                    onChange={handleUserInput}
                                />
                            </div>
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
                                <label className="form-label">Số căn cước</label>
                                <input
                                    type="text"
                                    value={formData.nationalId}
                                    className="form-control"
                                    name="nationalId"
                                    placeholder="Số căn cước..."
                                    onChange={handleUserInput}
                                />
                            </div>
                        </div>
                        <div className="row">
                            <div className="col-md-3 mb-3">
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
                                <label className="form-label">Người duyệt</label>
                                <Select
                                    name="reviewer"
                                    options={reviewerOptions}
                                    onChange={handleSelect("reviewerId")}
                                    isClearable
                                    placeholder="Chọn thư ký"
                                    value={
                                        reviewerOptions?.find((option: any) => option.value === formData.reviewerId) ||
                                        null
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
                        </div>
                        <div className="text-end">
                            <button type="submit" className="btn btn-primary">
                                {button.search}
                            </button>
                            <button type="button" className="btn btn-secondary ms-2" onClick={handleReset}>
                                Xóa
                            </button>
                        </div>
                    </form>
                </Accordion.Body>
            </Accordion.Item>
        </Accordion>
    )
}

export default StudentApplicationSearch
