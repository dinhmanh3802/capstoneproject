import { Link, useNavigate, useParams } from "react-router-dom"
import {
    useGetStudentApplicationByIdQuery,
    useUpdateStudentApplicationMutation,
} from "../../apis/studentApplicationApi"
import { MainLoader, StudentCourseInfo, StudentInfo, StudentReportInfo } from "../../components/Page"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { apiResponse, studentApplicationModel } from "../../interfaces"
import { SD_CourseStatus, SD_ProcessStatus, SD_Role_Name } from "../../utility/SD"
import { format } from "date-fns"
import { useState } from "react"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { Accordion, Button, Modal } from "react-bootstrap"
import { toastNotify } from "../../helper"

const styles = {
    statusPending: {
        backgroundColor: "#ffc107", // Yellow
        color: "white",
        padding: "5px 10px",
        borderRadius: "4px",
    },
    statusApproved: {
        backgroundColor: "#28a745", // Green
        color: "white",
        padding: "5px 10px",
        borderRadius: "4px",
    },
    statusRejected: {
        backgroundColor: "#dc3545", // Red
        color: "white",
        padding: "5px 10px",
        borderRadius: "4px",
    },
}

const getApplicationStatusText = (status: any) => {
    let statusStyle = {}
    let statusText = ""
    switch (status) {
        case SD_ProcessStatus.Pending:
            statusStyle = styles.statusPending
            statusText = "Chờ duyệt"
            break
        case SD_ProcessStatus.Rejected:
            statusStyle = styles.statusRejected
            statusText = "Từ chối"
            break
        default:
            statusStyle = styles.statusApproved
            statusText = "Chấp nhập"
            break
    }

    return <div style={statusStyle}>{statusText}</div>
}

function StudentDetail() {
    const navigate = useNavigate()
    const { id, courseId } = useParams()
    const { data: studentCourse, isLoading: studentLoading, refetch } = useGetStudentApplicationByIdQuery(id)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    if (studentLoading) {
        return <MainLoader />
    }
    const studentApplication: studentApplicationModel = studentCourse?.result
    return (
        <div className="container">
            <div className="mt-0 mb-3">
                <h3 className="fw-bold">Thông tin chi tiết khóa sinh</h3>
            </div>
            <Accordion defaultActiveKey={["1", "2", "3"]} alwaysOpen>
                <Accordion.Item eventKey="1" className="mb-3">
                    <Accordion.Header>
                        <i className="bi bi-person me-2"></i>
                        Thông tin cá nhân
                    </Accordion.Header>
                    <Accordion.Body>
                        <StudentInfo studentApplication={studentApplication} refetchStudent={refetch} />
                    </Accordion.Body>
                </Accordion.Item>
                <Accordion.Item eventKey="2" className="mb-3">
                    <Accordion.Header>
                        <i className="bi bi-person me-2"></i>
                        Thông tin khóa tu
                    </Accordion.Header>
                    <Accordion.Body>
                        <StudentCourseInfo
                            studentApplication={studentApplication}
                            courseId={courseId}
                            refetchStudent={refetch}
                        />
                    </Accordion.Body>
                </Accordion.Item>
                <Accordion.Item eventKey="3">
                    <Accordion.Header>
                        <i className="bi bi-person me-2"></i>
                        Thông tin đánh giá
                    </Accordion.Header>
                    <Accordion.Body>
                        <StudentReportInfo courseId={courseId} studentApplication={studentApplication} />
                    </Accordion.Body>
                </Accordion.Item>
            </Accordion>
            <div className="row-12 mt-2 text-start">
                <button type="button" className="btn btn-secondary me-2" onClick={() => navigate(-1)}>
                    Quay lại
                </button>
            </div>
        </div>
    )
}

export default StudentDetail
