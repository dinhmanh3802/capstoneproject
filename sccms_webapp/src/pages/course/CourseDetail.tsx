import { CourseInfo, MainLoader } from "../../components/Page"
import { useParams } from "react-router-dom"
import { useGetCourseByIdQuery } from "../../apis/courseApi"

function CourseDetail() {
    const { id } = useParams()
    const { data, isLoading } = useGetCourseByIdQuery(id)
    if (isLoading) {
        return <MainLoader />
    }
    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Chi tiết Khóa Tu</h3>
            </div>
            <CourseInfo course={data?.result} />
        </div>
    )
}

export default CourseDetail
