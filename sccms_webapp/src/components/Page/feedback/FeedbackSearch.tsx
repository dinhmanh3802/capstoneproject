import React, { useState, useEffect } from "react"
import { Accordion } from "react-bootstrap"
import Select, { SingleValue } from "react-select"
import { useNavigate, useLocation } from "react-router-dom"
import { inputHelper } from "../../../helper"
import { button } from "../../../utility/Label"

function FeedbackSearch({ onSearch, courseList }: { onSearch: any; courseList: any }) {
    const navigate = useNavigate()
    const location = useLocation()
    // Map course options only if coursesData is an array, otherwise fallback to an empty array
    const courseOptions = Array.isArray(courseList)
        ? courseList?.map((course: any) => ({
              value: course.id,
              label: course.courseName,
          }))
        : []

    // Function to initialize form data from URL query parameters
    const getInitialFormData = () => {
        const searchParams = new URLSearchParams(location.search)
        return {
            courseId: searchParams.get("courseId")
                ? parseInt(searchParams.get("courseId")!)
                : parseInt(courseList[0].id),
            feedbackDateStart: searchParams.get("feedbackDateStart") || "",
            feedbackDateEnd: searchParams.get("feedbackDateEnd") || "",
        }
    }

    const [formData, setFormData] = useState(getInitialFormData())

    // Sync form data with URL changes
    useEffect(() => {
        const updatedFormData = getInitialFormData()
        setFormData(updatedFormData)
        onSearch(updatedFormData)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [location.search, courseList])

    // Handle input changes
    const handleUserInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        const tempData = inputHelper(e, formData)
        setFormData(tempData)
    }

    // Handle dropdown changes for courseId
    const handleSelect = (field: string) => (selectedOption: SingleValue<{ value: any; label: any }>) => {
        setFormData((prevData: any) => ({
            ...prevData,
            [field]: selectedOption ? selectedOption.value : "",
        }))
    }

    // Handle form submission for searching
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        const queryParams = new URLSearchParams(formData as any).toString()
        navigate(`/feedback?${queryParams}`)
        onSearch(formData)
    }

    // Reset search fields
    const handleReset = () => {
        const resetFormData = {
            courseId: parseInt(courseList[0].id) ? parseInt(courseList[0].id) : 0,
            feedbackDateStart: "",
            feedbackDateEnd: "",
        }
        setFormData(resetFormData)
        navigate(`/feedback`)
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
                            <div className="col-md-4 mb-3">
                                <label className="form-label">Ngày phản hồi từ</label>
                                <input
                                    type="date"
                                    value={formData.feedbackDateStart}
                                    className="form-control"
                                    name="feedbackDateStart"
                                    onChange={handleUserInput}
                                />
                            </div>
                            <div className="col-md-4 mb-3">
                                <label className="form-label">Đến ngày</label>
                                <input
                                    type="date"
                                    value={formData.feedbackDateEnd}
                                    className="form-control"
                                    name="feedbackDateEnd"
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

export default FeedbackSearch
