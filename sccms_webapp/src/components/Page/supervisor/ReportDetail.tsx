import React, { useState, useEffect, useMemo, useCallback } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useSelector } from "react-redux"
import { Button, Card, Form, Spinner, Alert, Badge, OverlayTrigger, Tooltip } from "react-bootstrap"
import { FaCheckCircle, FaTimesCircle, FaPaperPlane, FaArrowLeft } from "react-icons/fa"
import DataTable, { TableColumn } from "react-data-table-component"
import { format, parse, isBefore } from "date-fns"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import { RootState } from "../../../store/store"
import { toastNotify } from "../../../helper"
import {
    useGetReportQuery,
    useSubmitAttendanceReportMutation,
    useSubmitNightShiftReportMutation,
    useRequestReopenReportMutation,
    useMarkReportAsReadMutation,
    useReopenReportMutation,
} from "../../../apis/reportApi"
import { SD_ReportStatus, SD_ReportType, SD_Role_Name } from "../../../utility/SD"
import { StudentReportDto } from "../../../interfaces"

// Hàm kiểm tra ngày hợp lệ
const isValidDate = (date: Date | null): boolean => date !== null && !isNaN(date.getTime())

function ReportDetail() {
    const { reportId } = useParams<{ reportId: string }>()
    const navigate = useNavigate()

    const { data: reportData, isLoading, isError, error, refetch } = useGetReportQuery({ reportId: Number(reportId) })
    const [submitAttendanceReport, { isLoading: isSubmittingAttendance }] = useSubmitAttendanceReportMutation()
    const [submitNightShiftReport, { isLoading: isSubmittingNightShift }] = useSubmitNightShiftReportMutation()
    const [requestReopenReport, { isLoading: isRequestingReopen }] = useRequestReopenReportMutation()
    const [markReportAsRead] = useMarkReportAsReadMutation()
    const [reopenReport] = useReopenReportMutation()

    const currentUser = useSelector((state: RootState) => state.auth.user)
    const userId = currentUser?.userId
    const userRole = currentUser?.role

    // State cho Confirmation Popup
    const [isConfirmOpen, setIsConfirmOpen] = useState(false)
    const [confirmMessage, setConfirmMessage] = useState("")
    const [confirmAction, setConfirmAction] = useState<() => void>(() => {})

    // Lấy báo cáo từ dữ liệu trả về
    const report = reportData?.result[0]

    const [studentReports, setStudentReports] = useState<StudentReportDto[]>([])
    const [reportContent, setReportContent] = useState<string>("")

    useEffect(() => {
        if (report) {
            setStudentReports(report.studentReports)
            setReportContent(report.reportContent || "")
        }
    }, [report])

    // Lấy danh sách khóa tu từ store
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])

    // Tìm khóa tu liên quan đến báo cáo
    const reportCourse = useMemo(() => {
        return listCourseFromStore.find((course) => course.id === report?.courseId) || null
    }, [listCourseFromStore, report?.courseId])

    // Xác định nếu khóa tu đã kết thúc
    const isPastReport =
        reportCourse && reportCourse.endDate ? isBefore(new Date(reportCourse.endDate), new Date()) : false

    // Xác định quyền
    const isManager = userRole === SD_Role_Name.MANAGER
    const isSupervisor = userRole === SD_Role_Name.SUPERVISOR
    const isStaff = userRole === SD_Role_Name.STAFF

    // Lấy các trường mới từ report
    const isSupervisorAssigned = report?.isSupervisorAssigned
    const isStaffAssigned = report?.isStaffAssigned

    // Xác định xem người dùng có thể chỉnh sửa báo cáo không
    let isEditable = report?.isEditable && !isPastReport

    // Nếu trạng thái là NotYet và ngày báo cáo không phải hôm nay, không cho phép chỉnh sửa
    const isReportDateToday =
        report?.reportDate && new Date(report.reportDate).toDateString() === new Date().toDateString()

    if (report?.status === SD_ReportStatus.NotYet && !isReportDateToday) {
        isEditable = false
    }

    // Cập nhật điều kiện cho canRequestReopen
    const canRequestReopen =
        ((isSupervisor && isSupervisorAssigned) || (isStaff && isStaffAssigned)) &&
        !isEditable &&
        !isPastReport &&
        ![SD_ReportStatus.NotYet, SD_ReportStatus.Reopened, SD_ReportStatus.Attending].includes(report?.status)

    // Dành cho Quản lý
    const canMarkAsRead = isManager && report?.status === SD_ReportStatus.Attended && !isPastReport
    const canReopenReport =
        isManager &&
        !isPastReport &&
        ![SD_ReportStatus.NotYet, SD_ReportStatus.Reopened, SD_ReportStatus.Attending].includes(report?.status)

    // Helper function to format time
    const formatTime = (time: string | undefined) => {
        if (!time) return "N/A"
        try {
            const parsedTime = parse(time, "HH:mm:ss", new Date())
            return format(parsedTime, "HH:mm")
        } catch (error) {
            return time
        }
    }

    // Xử lý thay đổi trạng thái điểm danh
    const handleStatusToggle = useCallback(
        (studentId: number) => {
            if (!isEditable) return
            setStudentReports((prevReports) =>
                prevReports?.map((sr) =>
                    sr.studentId === studentId ? { ...sr, status: sr.status === 0 ? 1 : 0 } : sr,
                ),
            )
        },
        [isEditable],
    )

    const handleCommentChange = useCallback(
        (studentId: number, comment: string) => {
            if (!isEditable) return
            setStudentReports((prevReports) =>
                prevReports?.map((sr) => (sr.studentId === studentId ? { ...sr, comment } : sr)),
            )
        },
        [isEditable],
    )

    const handleSubmit = async () => {
        try {
            if (report.reportType === SD_ReportType.DailyReport) {
                await submitAttendanceReport({
                    reportId: Number(reportId),
                    studentReports,
                    reportContent,
                }).unwrap()
            } else if (report.reportType === SD_ReportType.NightShift) {
                await submitNightShiftReport({
                    reportId: Number(reportId),
                    studentReports,
                    reportContent,
                }).unwrap()
            }
            toastNotify("Nộp báo cáo thành công!", "success")
            refetch()
        } catch (error: any) {
            toastNotify("Có lỗi xảy ra khi nộp báo cáo: " + (error?.data?.message || "Vui lòng thử lại"), "error")
        }
    }

    // Xử lý đánh dấu đã đọc
    const handleMarkAsRead = () => {
        setConfirmMessage("Bạn có chắc chắn muốn đánh dấu báo cáo này là đã đọc?")
        setConfirmAction(() => async () => {
            try {
                await markReportAsRead(Number(reportId)).unwrap()
                toastNotify("Đã đánh dấu báo cáo là đã đọc", "success")
                refetch()
            } catch (error: any) {
                toastNotify("Có lỗi xảy ra", "error")
            }
        })
        setIsConfirmOpen(true)
    }

    // Xử lý mở lại báo cáo
    const handleReopenReportClick = () => {
        setConfirmMessage("Bạn có chắc chắn muốn mở lại báo cáo này?")
        setConfirmAction(() => async () => {
            try {
                await reopenReport(Number(reportId)).unwrap()
                toastNotify("Đã mở lại báo cáo", "success")
                refetch()
            } catch (error: any) {
                toastNotify("Có lỗi xảy ra", "error")
            }
        })
        setIsConfirmOpen(true)
    }

    // Xử lý yêu cầu mở lại
    const handleRequestReopenClick = () => {
        setConfirmMessage(
            "Bạn có chắc chắn muốn yêu cầu mở lại báo cáo này không? Yêu cầu của bạn sẽ được gửi đến Quản lý.",
        )
        setConfirmAction(() => async () => {
            try {
                await requestReopenReport(Number(reportId)).unwrap()
                toastNotify("Yêu cầu mở lại đã được gửi thành công!", "success")
                refetch()
            } catch (error: any) {
                toastNotify("Có lỗi xảy ra khi gửi yêu cầu: " + (error?.data?.message || "Vui lòng thử lại"), "error")
            }
        })
        setIsConfirmOpen(true)
    }

    // Định nghĩa các cột cho DataTable
    const columns: TableColumn<StudentReportDto>[] = useMemo(
        () => [
            {
                name: "Hình ảnh",
                selector: (row) => row.studentImage,
                cell: (row) =>
                    row.studentImage ? (
                        <div
                            style={{
                                display: "flex",
                                justifyContent: "center",
                                alignItems: "center",
                                padding: "5px",
                            }}
                        >
                            <img
                                src={row.studentImage}
                                alt={`Học viên ${row.studentName}`}
                                className="rounded"
                                style={{
                                    width: "100%",
                                    height: "auto",
                                    maxWidth: "80px",
                                    maxHeight: "120px",
                                    objectFit: "cover",
                                    boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
                                    borderRadius: "10px",
                                }}
                                loading="lazy"
                            />
                        </div>
                    ) : (
                        <div
                            className="placeholder-avatar"
                            style={{
                                display: "flex",
                                justifyContent: "center",
                                alignItems: "center",
                                padding: "5px",
                            }}
                        >
                            <i className="bi bi-person-circle fs-1"></i>
                        </div>
                    ),
                ignoreRowClick: true,
                allowOverflow: true,
            },
            {
                name: "Tên học viên",
                selector: (row) => row.studentName || "N/A",
                sortable: true,
            },
            {
                name: "Mã khóa sinh",
                selector: (row) => row.studentCode || "N/A",
                sortable: true,
                cell: (row) => <span>{row.studentCode || "N/A"}</span>,
            },
            {
                name: "Trạng thái",
                selector: (row) => row.status,
                sortable: true,
                cell: (row) => {
                    const status =
                        row.status === 1 ? { text: "Có mặt", color: "success" } : { text: "Vắng", color: "danger" }
                    return (
                        <OverlayTrigger
                            placement="top"
                            overlay={<Tooltip id={`tooltip-status-${row.studentId}`}>Trạng thái của học viên</Tooltip>}
                        >
                            {isEditable ? (
                                <Badge
                                    bg={status.color}
                                    style={{
                                        cursor: "pointer",
                                        width: "100px",
                                        height: "30px",
                                        display: "flex",
                                        alignItems: "center",
                                        justifyContent: "center",
                                        fontSize: "13px",
                                        fontWeight: "bold",
                                    }}
                                    onClick={() => handleStatusToggle(row.studentId)}
                                >
                                    {status.text === "Có mặt" ? (
                                        <>
                                            <FaCheckCircle className="me-1" />
                                            {status.text}
                                        </>
                                    ) : (
                                        <>
                                            <FaTimesCircle className="me-1" />
                                            {status.text}
                                        </>
                                    )}
                                </Badge>
                            ) : (
                                <Badge bg={status.color} style={{ cursor: "default" }}>
                                    {status.text === "Có mặt" ? (
                                        <>
                                            <FaCheckCircle className="me-1" />
                                            {status.text}
                                        </>
                                    ) : (
                                        <>
                                            <FaTimesCircle className="me-1" />
                                            {status.text}
                                        </>
                                    )}
                                </Badge>
                            )}
                        </OverlayTrigger>
                    )
                },
            },
            {
                name: "Ghi chú",
                selector: (row) => row.comment || "N/A",
                cell: (row) =>
                    isEditable ? (
                        <Form.Control
                            as="textarea"
                            value={row.comment || ""}
                            onChange={(e) => handleCommentChange(row.studentId, e.target.value)}
                            placeholder="Nhập ghi chú..."
                            rows={2}
                            style={{ resize: "vertical" }}
                        />
                    ) : (
                        <div>
                            {row.comment || (
                                <Badge bg="light" text="dark">
                                    Không có ghi chú
                                </Badge>
                            )}
                        </div>
                    ),
            },
        ],
        [isEditable, handleStatusToggle, handleCommentChange],
    )

    // Tùy chỉnh giao diện của DataTable
    const customStyles = useMemo(
        () => ({
            headCells: {
                style: {
                    fontSize: "16px",
                    fontWeight: "bold",
                    backgroundColor: "#f1f3f5",
                    color: "#343a40",
                    borderBottom: "2px solid #dee2e6",
                    padding: "12px 15px",
                },
            },
            cells: {
                style: {
                    fontSize: "14px",
                    color: "#495057",
                    borderBottom: "1px solid #dee2e6",
                    padding: "10px 15px",
                },
            },
            tableWrapper: {
                style: {
                    border: "1px solid #dee2e6",
                    borderRadius: "5px",
                },
            },
        }),
        [],
    )

    if (isLoading) {
        // Hiển thị spinner khi đang tải dữ liệu
        return (
            <div className="d-flex justify-content-center align-items-center" style={{ height: "80vh" }}>
                <Spinner animation="border" variant="primary" />
                <span className="ms-2">Đang tải...</span>
            </div>
        )
    }

    if (isError) {
        // Hiển thị thông báo lỗi
        return (
            <Alert variant="danger" className="mt-4">
                {`Có lỗi xảy ra khi tải báo cáo. Vui lòng thử lại sau.`}
            </Alert>
        )
    }

    if (!report) {
        // Hiển thị thông báo khi không tìm thấy báo cáo
        return (
            <Alert variant="warning" className="mt-4">
                Báo cáo không tồn tại hoặc bạn không có quyền truy cập.
            </Alert>
        )
    }

    return (
        <div className="container mt-4 position-relative">
            <Card className="shadow-sm p-4">
                {/* Header với các thông tin trên cùng một dòng */}
                <div className="d-flex flex-wrap justify-content-between align-items-center mb-4">
                    <h3 className="fw-bold text-primary mb-3 mb-md-0">
                        Chi tiết báo cáo{" "}
                        {report.reportType === SD_ReportType.DailyReport && (
                            <strong>Chánh {report.studentGroup?.groupName || "N/A"}</strong>
                        )}{" "}
                        {report.reportType === SD_ReportType.NightShift && (
                            <>
                                <strong>
                                    {formatTime(report.nightShift?.startTime)} –{" "}
                                    {formatTime(report.nightShift?.endTime)}
                                </strong>{" "}
                                <strong>Phòng {report.room?.name || "N/A"}</strong>
                            </>
                        )}{" "}
                        <strong>
                            ngày {report?.reportDate ? format(new Date(report.reportDate), "dd/MM/yyyy") : "N/A"}
                        </strong>{" "}
                    </h3>
                    <div className="d-flex flex-wrap gap-3">
                        {report.submittedByUser && (
                            <div>
                                <strong className="mb-2">Người báo cáo:</strong>{" "}
                                {`${report.submittedByUser.userName} - ${report.submittedByUser.fullName}`}
                                <br />
                                <strong>Thời gian báo cáo:</strong>{" "}
                                {report.submissionDate
                                    ? format(new Date(report.submissionDate), " HH:mm '-' dd/MM/yyyy")
                                    : "N/A"}
                            </div>
                        )}
                    </div>
                </div>

                <hr />
                <DataTable
                    columns={columns}
                    data={studentReports}
                    customStyles={customStyles}
                    pagination
                    highlightOnHover
                    striped
                    responsive
                    noDataComponent="Không có dữ liệu học viên."
                    keyField="studentId"
                />
                {/* Phần nội dung báo cáo */}
                <Form.Group controlId="reportContent" className="mt-4">
                    <Form.Label>
                        <strong>Nội dung báo cáo:</strong>
                    </Form.Label>
                    <Form.Control
                        as="textarea"
                        rows={4}
                        value={reportContent}
                        onChange={(e) => setReportContent(e.target.value)}
                        placeholder="Nhập nội dung báo cáo..."
                        style={{ resize: "vertical" }}
                        readOnly={!isEditable}
                    />
                </Form.Group>
                {/* Nút hành động */}
                <div className="text-end mt-3">
                    {!isPastReport && (
                        <>
                            {isEditable && (
                                <Button
                                    variant="success"
                                    onClick={handleSubmit}
                                    disabled={isSubmittingAttendance || isSubmittingNightShift}
                                    className="me-2"
                                >
                                    {isSubmittingAttendance || isSubmittingNightShift ? (
                                        <>
                                            <Spinner
                                                as="span"
                                                animation="border"
                                                size="sm"
                                                role="status"
                                                aria-hidden="true"
                                            />
                                            <span className="ms-2">Đang mở...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaPaperPlane className="me-2" />
                                            Nộp báo cáo
                                        </>
                                    )}
                                </Button>
                            )}
                            {canRequestReopen && (
                                <Button
                                    variant="warning"
                                    onClick={handleRequestReopenClick}
                                    disabled={isRequestingReopen}
                                    className="me-2"
                                >
                                    {isRequestingReopen ? (
                                        <>
                                            <Spinner
                                                as="span"
                                                animation="border"
                                                size="sm"
                                                role="status"
                                                aria-hidden="true"
                                            />
                                            <span className="ms-2">Đang gửi...</span>
                                        </>
                                    ) : (
                                        "Yêu cầu mở lại"
                                    )}
                                </Button>
                            )}
                            {canReopenReport && (
                                <Button variant="warning" onClick={handleReopenReportClick} className="me-2">
                                    Mở lại
                                </Button>
                            )}
                            {canMarkAsRead && (
                                <Button variant="success" onClick={handleMarkAsRead}>
                                    Đánh dấu đã đọc
                                </Button>
                            )}
                        </>
                    )}
                    {/* Nút "Quay lại" luôn hiển thị */}
                    <Button variant="secondary" className="ms-2" onClick={() => navigate(-1)}>
                        <FaArrowLeft className="me-2" />
                        Quay lại
                    </Button>
                </div>
            </Card>

            {/* Confirmation Popup */}
            <ConfirmationPopup
                isOpen={isConfirmOpen}
                onClose={() => setIsConfirmOpen(false)}
                onConfirm={() => {
                    confirmAction()
                    setIsConfirmOpen(false)
                }}
                message={confirmMessage}
                title="Xác nhận"
            />
        </div>
    )
}

export default ReportDetail
