import React, { useState, useEffect } from "react"
import { button } from "../../../utility/Label"
import { Accordion } from "react-bootstrap"
import Select, { SingleValue } from "react-select"
import { useNavigate, useLocation } from "react-router-dom"
import { inputHelper } from "../../../helper"
import { SD_PostStatus, SD_PostType } from "../../../utility/SD"

// Tạo các options cho trạng thái và loại bài viết
const statusOptions = [
    { value: "", label: "Tất cả" },
    { value: SD_PostStatus.Draft, label: "Bản nháp" },
    { value: SD_PostStatus.Active, label: "Hiển thị" },
]

const postTypeOptions = [
    { value: "", label: "Tất cả" },
    { value: SD_PostType.Introduction, label: "Giới thiệu" },
    { value: SD_PostType.Activities, label: "Hoạt động khoá tu" },
    { value: SD_PostType.Announcement, label: "Thông báo khoá tu" },
]

function PostSearch({ onSearch }: { onSearch: any }) {
    const navigate = useNavigate()
    const location = useLocation()

    // Hàm lấy dữ liệu khởi tạo từ URL hoặc mặc định
    const getInitialFormData = () => {
        const searchParams = new URLSearchParams(location.search)
        return {
            postType: searchParams.get("postType") ? parseInt(searchParams.get("postType")!) : "",
            title: searchParams.get("title") || "",
            content: searchParams.get("content") || "",
            status: searchParams.get("status") ? parseInt(searchParams.get("status")!) : "",
            postDateStart: searchParams.get("postDateStart") || "",
            postDateEnd: searchParams.get("postDateEnd") || "",
        }
    }

    const [formData, setFormData] = useState(getInitialFormData())

    // Đồng bộ formData với URL mỗi khi URL thay đổi
    useEffect(() => {
        const updatedFormData = getInitialFormData()
        setFormData(updatedFormData)
        onSearch(updatedFormData)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search])

    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement>) => {
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
        const queryParams = new URLSearchParams(formData as any).toString()
        navigate(`/posts?${queryParams}`)
        onSearch(formData)
    }

    const handleReset = () => {
        const resetFormData = {
            postType: "",
            title: "",
            content: "",
            status: "", // Đặt lại status thành "Tất cả"
            postDateStart: "",
            postDateEnd: "",
        }
        setFormData(resetFormData)
        navigate(`/posts`)
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
                            <div className="col-md-4 mb-3">
                                <label className="form-label">Mục</label>
                                <Select
                                    name="postType"
                                    options={postTypeOptions}
                                    onChange={handleSelect("postType")}
                                    isClearable
                                    placeholder="Chọn mục"
                                    value={
                                        postTypeOptions?.find((option: any) => option.value === formData.postType) ||
                                        null
                                    }
                                />
                            </div>

                            <div className="col-md-4 mb-3">
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
                            <div className="col-md-4 mb-3">
                                <label className="form-label">Tiêu đề</label>
                                <input
                                    type="text"
                                    value={formData.title}
                                    className="form-control"
                                    name="title"
                                    placeholder="Tiêu đề..."
                                    onChange={handleUserInput}
                                />
                            </div>
                        </div>

                        <div className="text-end mt-3">
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

export default PostSearch
