import React, { useEffect, useState } from "react"
import SearchCourse from "../../components/Page/course/SearchCourse"
import CourseList from "../../components/Page/course/CourseList"
import { Link } from "react-router-dom"
import { useGetCourseQuery } from "../../apis/courseApi"
import { MainLoader } from "../../components/Page"

function ListCourse() {
    //Khởi tạo giá trị ban đầu cho searchParams
    const [searchParams, setSearchParams] = useState({
        name: "",
        status: "",
        secretaryLeaderId: "",
        startDate: "",
        endDate: "",
    })

    //Gọi API để lấy dữ liệu
    const { data, isLoading } = useGetCourseQuery({
        name: searchParams.name,
        status: searchParams.status,
        secretaryLeaderId: searchParams.secretaryLeaderId,
        startDate: searchParams.startDate,
        endDate: searchParams.endDate,
    })

    //Hàm xử lý khi người dùng tìm kiếm
    const handleSearch = (params: any) => {
        setSearchParams({
            name: params.courseName,
            status: params.courseStatus,
            secretaryLeaderId: params.LeaderId,
            startDate: params.startDate,
            endDate: params.endDate,
        })
    }
    if (isLoading) return <MainLoader />
    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách khóa tu</h3>
            </div>
            <SearchCourse onSearch={handleSearch} />
            <div className="container text-end mt-4">
                <Link to={"/create-course"} className="btn btn-sm btn-primary">
                    <i className="bi bi-plus-lg me-1"></i>Tạo mới
                </Link>
            </div>
            <div className="mt-2">
                <CourseList course={data?.result} />
            </div>
        </div>
    )
}

export default ListCourse
