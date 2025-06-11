import React, { useState, useEffect } from "react"
import { Accordion } from "react-bootstrap"
import Select from "react-select"
import { useNavigate, useLocation } from "react-router-dom"
import { SD_Gender, SD_ProcessStatus, SD_Role_Name } from "../../../utility/SD"

const statusOptions = [
    { value: SD_ProcessStatus.Pending, label: "Đang chờ" },
    { value: SD_ProcessStatus.Approved, label: "Chấp nhận" },
    { value: SD_ProcessStatus.Rejected, label: "Từ chối" },
]

const genderOptions = [
    { value: SD_Gender.Male, label: "Nam" },
    { value: SD_Gender.Female, label: "Nữ" },
]

function VolunteerApplicationSearch({
    onSearch,
    courseList,
    currentCourse,
    secretaryList,
    teamList,
    currentUserRole,
    currentUserId,
}: {
    onSearch: any
    courseList: any
    currentCourse: any
    secretaryList: any
    currentUserRole: any
    currentUserId: any
    teamList: any
}) {
    const navigate = useNavigate()
    const location = useLocation()

    const courseOptions = courseList?.map((course: any) => ({
        value: course.id,
        label: course.courseName,
    }))

    const reviewerOptions = secretaryList?.map((secretary: any) => ({
        value: secretary.id,
        label: secretary.userName,
    }))
    const teamOptions = teamList?.map((team: any) => ({
        value: team.id,
        label: team.teamName,
    }))
    teamOptions.unshift({ value: 0, label: "Chưa phân" })

    const getInitialFormData = () => {
        const searchParams = new URLSearchParams(location.search)
        return {
            courseId: searchParams.get("courseId") ? parseInt(searchParams.get("courseId")!) : currentCourse?.id,
            name: searchParams.get("name") || "",
            phoneNumber: searchParams.get("phoneNumber") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            reviewerId: searchParams.get("reviewerId") ? parseInt(searchParams.get("reviewerId")!) : "",
            gender: searchParams.get("gender") ? parseInt(searchParams.get("gender")!) : "",
            teamId: searchParams.get("teamId") ? parseInt(searchParams.get("teamId")!) : "",
            nationalId: searchParams.get("nationalId") || "",
        }
    }

    const [formData, setFormData] = useState(getInitialFormData)

    useEffect(() => {
        const updatedFormData = getInitialFormData()

        if (currentCourse && !location.search.includes("courseId")) {
            updatedFormData.courseId = currentCourse.id
        }

        setFormData(updatedFormData)
        onSearch(updatedFormData)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search, currentCourse])

    const handleSelectChange = (field: string) => (option: any) => {
        setFormData((prev) => ({ ...prev, [field]: option ? option.value : "" }))
    }

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        const query = new URLSearchParams(formData as any).toString()
        navigate(`/volunteer-applications?${query}`)
        onSearch(formData)
    }

    const handleReset = () => {
        const resetData = {
            courseId: currentCourse?.id || "",
            name: "",
            phoneNumber: "",
            status: "",
            reviewerId: "",
            gender: "",
            nationalId: "",
        } // @ts-ignore
        setFormData(resetData)
        navigate(`/volunteer-applications`)
        onSearch(resetData)
    }

    if (!currentCourse) return null

    return (
        <Accordion>
            <Accordion.Item eventKey="0">
                <Accordion.Header>
                    <i className="bi bi-search me-2"></i> Tìm kiếm
                </Accordion.Header>
                <Accordion.Body>
                    <form onSubmit={handleSubmit}>
                        <div className="row">
                            <div className="col-md-3">
                                <label>Khóa</label>
                                <Select
                                    options={courseOptions}
                                    onChange={handleSelectChange("courseId")}
                                    value={courseOptions?.find((opt) => opt.value === formData.courseId)}
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Căn cước</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={formData.nationalId}
                                    name="nationalId"
                                    placeholder="Nhập căn cước..."
                                    onChange={(e) => setFormData({ ...formData, nationalId: e.target.value })}
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Tên tình nguyện viên</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={formData.name}
                                    name="name"
                                    placeholder="Nhập tên..."
                                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Số điện thoại</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={formData.phoneNumber}
                                    name="phoneNumber"
                                    placeholder="Nhập số điện thoại..."
                                    onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                                />
                            </div>
                        </div>
                        <div className="row mt-3">
                            <div className="col-md-3">
                                <label>Giới tính</label>
                                <Select
                                    options={genderOptions}
                                    onChange={handleSelectChange("gender")}
                                    value={genderOptions?.find((opt) => opt.value === formData.gender) || null}
                                    isClearable
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Người duyệt</label>
                                <Select
                                    options={reviewerOptions}
                                    onChange={handleSelectChange("reviewerId")}
                                    value={reviewerOptions?.find((opt) => opt.value === formData.reviewerId) || null}
                                    isClearable
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Trạng thái</label>
                                <Select
                                    options={statusOptions}
                                    onChange={handleSelectChange("status")}
                                    value={statusOptions?.find((opt) => opt.value === formData.status) || null}
                                    isClearable
                                />
                            </div>
                            <div className="col-md-3">
                                <label>Ban</label>
                                <Select
                                    options={teamOptions}
                                    onChange={handleSelectChange("teamId")}
                                    value={teamOptions?.find((opt) => opt.value === formData.teamId) || null}
                                    isClearable
                                />
                            </div>
                        </div>

                        <div className="text-end mt-4">
                            <button type="submit" className="btn btn-primary">
                                Tìm kiếm
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

export default VolunteerApplicationSearch
