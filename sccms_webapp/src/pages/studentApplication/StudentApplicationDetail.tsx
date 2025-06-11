import { Link, useNavigate, useParams, useLocation } from "react-router-dom"
import {
    useGetStudentApplicationByIdQuery,
    useUpdateStudentApplicationMutation,
} from "../../apis/studentApplicationApi"
import { MainLoader } from "../../components/Page"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { apiResponse, studentApplicationModel } from "../../interfaces"
import { SD_CourseStatus, SD_ProcessStatus, SD_Role_Name } from "../../utility/SD"
import { format } from "date-fns"
import { useState } from "react"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { Button, Modal } from "react-bootstrap"
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

function StudentApplicationDetail() {
    const navigate = useNavigate()
    const { id } = useParams()
    const [updateStudentApplication] = useUpdateStudentApplicationMutation()
    const { data: studentCourse, isLoading: studentLoading } = useGetStudentApplicationByIdQuery(id)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)

    // State for handling rejection popup visibility and reason
    const [showRejectPopup, setShowRejectPopup] = useState(false)
    const [showDeletePopup, setShowDeletePopup] = useState(false)
    const [rejectionReason, setRejectionReason] = useState("")

    // State for handling approval confirmation popup visibility
    const [showApprovalPopup, setShowApprovalPopup] = useState(false)

    if (studentLoading) {
        return <MainLoader />
    }

    const studentApplication: studentApplicationModel = studentCourse?.result

    const handleDelete = () => {
        setShowDeletePopup(true) // Show rejection popup
    }

    const handleConfirmDelete = async () => {
        const studentApplicationUpdate = {
            Ids: [studentApplication.id],
            Status: SD_ProcessStatus.Deleted,
            reviewerId: currentUserId,
            note: rejectionReason,
        }
        try {
            const response: apiResponse = await updateStudentApplication(studentApplicationUpdate)
            if (response.data?.isSuccess) {
                toastNotify("Xóa đơn thành công", "success")
                navigate(-1)
            } else {
                toastNotify("Xóa đơn thất bại", "error")
            }
        } catch (error) {
            toastNotify("Duyệt đơn thất bại", "error")
        }
        setShowDeletePopup(false) // Close popup after rejection
    }

    // Function to handle rejection confirmation
    const handleReject = () => {
        setShowRejectPopup(true) // Show rejection popup
    }

    // Function to confirm rejection and send rejection reason to the server
    const handleConfirmReject = async () => {
        const studentApplicationUpdate = {
            Ids: [studentApplication.id],
            Status: SD_ProcessStatus.Rejected,
            reviewerId: currentUserId,
            note: rejectionReason,
        }
        try {
            const response: apiResponse = await updateStudentApplication(studentApplicationUpdate)
            if (response.data?.isSuccess) {
                toastNotify("Duyệt đơn thành công", "success")
            } else {
                toastNotify("Duyệt đơn thất bại", "error")
            }
        } catch (error) {
            toastNotify("Duyệt đơn thất bại", "error")
        }
        setShowRejectPopup(false) // Close popup after rejection
    }

    // Function to handle approval confirmation
    const handleAccept = () => {
        setShowApprovalPopup(true) // Show approval confirmation popup
    }

    // Function to confirm approval and send approval request to the server
    const handleConfirmAccept = async () => {
        const studentApplicationUpdate = {
            Ids: [studentApplication.id],
            Status: SD_ProcessStatus.Approved,
            reviewerId: currentUserId,
            note: "",
        }
        try {
            const response: apiResponse = await updateStudentApplication(studentApplicationUpdate)
            if (response.data?.isSuccess) {
                toastNotify("Duyệt đơn thành công", "success")
            } else {
                toastNotify("Duyệt đơn thất bại", "error")
            }
        } catch (error) {
            toastNotify("Duyệt đơn thất bại", "error")
        }
        setShowApprovalPopup(false) // Close the popup after approval
    }
    return (
        <div className="container">
            <div className="mt-0 mb-3">
                <h3 className="fw-bold">Thông tin chi tiết đơn đăng ký</h3>
            </div>
            {studentApplication.sameNationId > 0 && (
                <div className="pt-3 pb-3">
                    <span
                        style={{ cursor: "pointer" }}
                        className="alert alert-warning pointer-event"
                        onClick={() =>
                            navigate(
                                `/student-applications?courseId=${studentApplication.courseId}&nationalId=${studentApplication.student?.nationalId}`,
                            )
                        }
                    >
                        {`Có ${studentApplication.sameNationId + 1} đơn đăng ký có số căn cước là:`}{" "}
                        <strong>{studentApplication.student?.nationalId}</strong>
                    </span>
                </div>
            )}
            <form className="row g-3">
                <div className="row">
                    <div className="row col-md-9 g-3">
                        <div className="col-md-4">
                            <label htmlFor="course" className="form-label fw-medium">
                                Khóa tu
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="fullName"
                                value={studentApplication.course?.courseName || ""}
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="fullName" className="form-label fw-medium">
                                Họ và tên
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="fullName"
                                value={studentApplication.student?.fullName}
                                disabled
                            />
                        </div>
                        <div className="col-md-2">
                            <label htmlFor="fullName" className="form-label fw-medium">
                                Giới tính
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="fullName"
                                value={studentApplication.student?.gender === 0 ? "Nam" : "Nữ"}
                                disabled
                            />
                        </div>
                        <div className="col-md-2">
                            <label htmlFor="status" className="form-label fw-medium">
                                Trạng thái
                            </label>
                            <div>{getApplicationStatusText(studentApplication.status)}</div>
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="dob" className="form-label fw-medium">
                                Ngày sinh
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="dob"
                                value={
                                    studentApplication.student?.dateOfBirth
                                        ? format(new Date(studentApplication.student.dateOfBirth), "dd-MM-yyyy")
                                        : ""
                                }
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="address" className="form-label fw-medium">
                                Địa chỉ
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="address"
                                value={studentApplication.student?.address}
                                disabled
                            />
                        </div>
                        <div className="col-md-2">
                            <label htmlFor="education" className="form-label fw-medium">
                                Học lực
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="education"
                                value={studentApplication.student?.academicPerformance}
                                disabled
                            />
                        </div>
                        <div className="col-md-2">
                            <label htmlFor="conduct" className="form-label fw-medium">
                                Hạnh kiểm
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="conduct"
                                value={studentApplication.student?.conduct}
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="parentName" className="form-label fw-medium">
                                Họ và tên cha / mẹ
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="parentName"
                                value={studentApplication.student?.parentName}
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="phone" className="form-label fw-medium">
                                Điện thoại phụ huynh
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="phone"
                                value={studentApplication.student?.emergencyContact}
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="email" className="form-label fw-medium">
                                Địa chỉ email
                            </label>
                            <input
                                type="email"
                                className="form-control"
                                id="email"
                                value={studentApplication.student?.email}
                                disabled
                            />
                        </div>
                        <div className="col-md-4">
                            <label htmlFor="idNumber" className="form-label fw-medium">
                                Số CMND
                            </label>
                            <input
                                type="text"
                                className="form-control"
                                id="idNumber"
                                value={studentApplication.student?.nationalId}
                                disabled
                            />
                        </div>
                        {currentUserRole !== SD_Role_Name.SECRETARY && (
                            <div className="col-md-4">
                                <label htmlFor="reviewer" className="form-label fw-medium">
                                    Người duyệt
                                </label>
                                <input
                                    type="text"
                                    className="form-control"
                                    id="idNumber"
                                    value={`${studentApplication.reviewer?.userName} - ${studentApplication.reviewer?.fullName}`}
                                    disabled
                                />
                            </div>
                        )}
                    </div>
                    <div className="col-md-3">
                        <label htmlFor="endDate" className="form-label">
                            <span style={{ visibility: "hidden" }}>.</span>
                        </label>
                        <div className="border p-1 text-center">
                            <img src={`${studentApplication.student?.image}`} alt="Front ID" className="img-fluid" />
                        </div>
                    </div>
                </div>
                <div className="col-md-6">
                    <label className="form-label fw-medium">Ảnh CCCD mặt trước</label>
                    <div className="border p-3 text-center">
                        <img
                            src={`${studentApplication.student?.nationalImageFront}`}
                            alt="Front ID"
                            className="img-fluid"
                            style={{ maxHeight: "30rem" }}
                        />
                    </div>
                </div>
                <div className="col-md-6">
                    <label className="form-label fw-medium">Ảnh CCCD mặt sau</label>
                    <div className="border p-3 text-center">
                        <img
                            src={`${studentApplication.student?.nationalImageBack}`}
                            alt="Back ID"
                            className="img-fluid"
                            style={{ maxHeight: "30rem" }}
                        />
                    </div>
                </div>
                {studentApplication.status === SD_ProcessStatus.Rejected && (
                    <div className="col-md-12">
                        <label htmlFor="note" className="form-label fw-medium">
                            Lý do từ chối
                        </label>
                        <textarea
                            id="note"
                            className="form-control"
                            value={studentApplication.note}
                            disabled
                            rows={4}
                        />
                    </div>
                )}

                <div className="row mt-4">
                    <div className="col-6 text-start">
                        <a className="btn btn-secondary me-2" onClick={() => navigate(-1)}>
                            Quay lại
                        </a>
                        {currentUserRole == SD_Role_Name.MANAGER &&
                            (studentCourse?.result.course?.status == SD_CourseStatus.notStarted ||
                                studentCourse?.result.course?.status == SD_CourseStatus.recruiting) && (
                                <button type="button" className="btn btn-danger " onClick={handleDelete}>
                                    Xóa Đơn
                                </button>
                            )}
                    </div>
                    <div className="col-6 text-end">
                        {(currentUserId == studentApplication.reviewerId ||
                            currentUserRole != SD_Role_Name.SECRETARY) &&
                            (studentCourse?.result.course?.status == SD_CourseStatus.notStarted ||
                                studentCourse?.result.course?.status == SD_CourseStatus.recruiting) && (
                                <>
                                    {studentApplication.status === SD_ProcessStatus.Pending && (
                                        <>
                                            <button
                                                type="button"
                                                className="btn btn-warning me-2"
                                                onClick={handleReject}
                                            >
                                                Từ chối
                                            </button>
                                            <button type="button" className="btn btn-primary" onClick={handleAccept}>
                                                Duyệt đơn
                                            </button>
                                        </>
                                    )}
                                    {studentApplication.status === SD_ProcessStatus.Rejected && (
                                        <div>
                                            <button type="button" className="btn btn-primary" onClick={handleAccept}>
                                                Duyệt đơn
                                            </button>
                                        </div>
                                    )}
                                    {studentApplication.status === SD_ProcessStatus.Approved && (
                                        <div>
                                            <button
                                                type="button"
                                                className="btn btn-warning me-2"
                                                onClick={handleReject}
                                            >
                                                Từ chối
                                            </button>
                                        </div>
                                    )}
                                </>
                            )}
                    </div>
                </div>
            </form>

            {showRejectPopup && (
                <Modal show={showRejectPopup} onHide={() => setShowRejectPopup(false)} centered backdrop="static">
                    <Modal.Header closeButton>
                        <Modal.Title>Xác nhận từ chối</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <div className="form-group">
                            <label htmlFor="rejectionReason">Lý do từ chối</label>
                            <textarea
                                id="rejectionReason"
                                className="form-control"
                                value={rejectionReason}
                                onChange={(e) => setRejectionReason(e.target.value)}
                                rows={4}
                                required={true}
                                placeholder="Nhập lý do từ chối"
                            />
                        </div>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={() => setShowRejectPopup(false)}>
                            Hủy bỏ
                        </Button>
                        <Button variant="danger" onClick={handleConfirmReject} disabled={!rejectionReason.trim()}>
                            Xác nhận từ chối
                        </Button>
                    </Modal.Footer>
                </Modal>
            )}
            {/* Approval Confirmation Popup */}
            <ConfirmationPopup
                isOpen={showApprovalPopup}
                onClose={() => setShowApprovalPopup(false)} // Close popup
                onConfirm={handleConfirmAccept} // Confirm approval
                message={`Xác nhận duyệt đơn đăng ký của khóa sinh <strong>${studentApplication.student?.fullName}</strong>?`}
                title="Xác nhận duyệt đơn"
            />
            <ConfirmationPopup
                isOpen={showDeletePopup}
                onClose={() => setShowDeletePopup(false)}
                onConfirm={handleConfirmDelete}
                message={`Xác nhận Xóa đơn đăng ký của khóa sinh <strong>${studentApplication.student?.fullName}</strong>?`}
                title="Xác nhận Xóa đơn"
            />
        </div>
    )
}

export default StudentApplicationDetail
