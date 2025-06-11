import React, { useState, useEffect, useMemo } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { Form, Button, Modal, Spinner } from "react-bootstrap"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { parseISO, format } from "date-fns"
import {
    useGetAllNightShiftQuery,
    useReassignStaffToShiftMutation,
    useGetAssignmentByIdQuery,
} from "../../apis/nightShiftAssignmentApi"
import { useLazyGetAllStaffFreeTimesQuery } from "../../apis/staffFreeTimeApi"
import { SD_NightShiftAssignmentStatus, SD_Gender } from "../../utility/SD"
import { formatInTimeZone } from "date-fns-tz"
import Select from "react-select"
import { toastNotify } from "../../helper"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { error } from "../../utility/Message"
import { MainLoader } from "../../components/Page"
import { apiResponse } from "../../interfaces"
import { useLocation, Navigate } from "react-router-dom"
import { useGetUsersQuery } from "../../apis/userApi"

const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

interface RejectedAssignment {
    id: number
    user: {
        userId: number
        userName: string
        fullName: string
        phoneNumber: string
        email: string
    }
    room: {
        id: number
        name: string
        gender: SD_Gender // Added gender property
    }
    nightShift: {
        id: number
        startTime: string
        endTime: string
    }
    rejectionReason: string
    date: string
    status: SD_NightShiftAssignmentStatus
    dateModified: string
    updatedBy: string
}

function RejectedNightShiftAssignments() {
    const location = useLocation()
    const queryParams = new URLSearchParams(location.search)
    const urlId = queryParams.get("id")
    const parsedId = urlId ? Number(urlId) : null
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const { data: listUser, isLoading: listUserLoading } = useGetUsersQuery({})
    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(undefined)
    useEffect(() => {
        if (currentCourse?.id) {
            setSelectedCourseId(currentCourse.id)
        }
    }, [currentCourse])
    const [selectedDate, setSelectedDate] = useState<Date | null>(null)

    const [currentPage, setCurrentPage] = useState(1)
    const [perPage, setPerPage] = useState(10)
    const [selectedRows, setSelectedRows] = useState<RejectedAssignment[]>([])
    const [isConfirmOpen, setIsConfirmOpen] = useState(false)
    const [confirmMessage, setConfirmMessage] = useState("")
    const [confirmAction, setConfirmAction] = useState<() => void>(() => {})
    const [clearRowsFlag, setClearRowsFlag] = useState(false)

    const [showModal, setShowModal] = useState(false)
    const [selectedAssignment, setSelectedAssignment] = useState<RejectedAssignment | null>(null)
    const [reassignStaff, setReassignStaff] = useState<{
        value: number
        label: string
    } | null>(null)

    const statusFilterOptions = [
        { value: "all", label: "Tất cả" },
        { value: "approved", label: "Đã duyệt" },
        { value: "not_approved", label: "Chưa duyệt" },
    ]

    const [statusFilter, setStatusFilter] = useState<string>("all")

    // Hook để lấy danh sách nhân viên rảnh
    const [getStaffFreeTimes, { data: staffData, isLoading: staffLoading }] = useLazyGetAllStaffFreeTimesQuery()

    const dateString = selectedDate ? formatInTimeZone(new Date(selectedDate), VIETNAM_TZ, "yyyy-MM-dd") : null

    // Chuẩn bị tham số cho API cho cả hai trạng thái 'rejected' và 'cancelled'
    const queryParametersRejected: any = {
        courseId: selectedCourseId,
        status: SD_NightShiftAssignmentStatus.rejected,
    }

    const queryParametersCancelled: any = {
        courseId: selectedCourseId,
        status: SD_NightShiftAssignmentStatus.cancelled,
    }

    if (selectedDate) {
        queryParametersRejected.dateTime = dateString
        queryParametersCancelled.dateTime = dateString
    }

    // Gọi API để lấy danh sách ca trực bị từ chối
    const {
        data: rejectedAssignmentsData,
        isLoading: isLoadingRejected,
        refetch: refetchRejected,
    } = useGetAllNightShiftQuery(queryParametersRejected, {
        skip: !selectedCourseId,
    })

    // Gọi API để lấy danh sách ca trực bị hủy
    const {
        data: cancelledAssignmentsData,
        isLoading: isLoadingCancelled,
        refetch: refetchCancelled,
    } = useGetAllNightShiftQuery(queryParametersCancelled, {
        skip: !selectedCourseId,
    })

    // Kết hợp dữ liệu từ cả hai trạng thái
    const combinedAssignments = useMemo(() => {
        const rejected = rejectedAssignmentsData?.result ?? []
        const cancelled = cancelledAssignmentsData?.result ?? []
        return [...rejected, ...cancelled]
    }, [rejectedAssignmentsData, cancelledAssignmentsData])
    // Danh sách nhân viên rảnh
    const staffOptions = useMemo(
        () =>
            staffData?.result.map((user) => ({
                value: user.userId,
                label: user.userName + " - " + user.fullName,
            })) || [],
        [staffData],
    )

    // Filtered Staff Options based on Room Gender
    const filteredStaffOptions = useMemo(() => {
        if (!selectedAssignment || !staffData?.result) return []
        return staffData.result
            .filter((staff) => staff.gender === selectedAssignment.room.gender && staff.isCancel != true)
            .map((staff) => ({
                value: staff.userId,
                label: `${staff.userName} - ${staff.fullName}`,
            }))
    }, [selectedAssignment, staffData])

    // Mutation để gán lại ca trực
    const [reassignStaffToShift, { isLoading: isReassigning }] = useReassignStaffToShiftMutation()

    // Hàm kiểm tra xem khóa tu đã kết thúc hay chưa
    const isCourseEnded = (): boolean => {
        if (!currentCourse || !currentCourse.endDate) {
            return false
        }
        const now = new Date()
        const courseEndDate = new Date(currentCourse.endDate)

        if (isNaN(courseEndDate.getTime())) {
            console.error("Invalid endDate format:", currentCourse.endDate)
            return false
        }

        return now > courseEndDate
    }

    // Hàm xử lý gán lại ca trực
    const handleReassign = async () => {
        if (!selectedAssignment || !reassignStaff) return

        const payload = {
            Id: selectedAssignment.id,
            newUserId: reassignStaff.value,
        }

        try {
            const response: apiResponse = await reassignStaffToShift(payload)
            if (response.data?.isSuccess) {
                toastNotify("Cập nhật thành công!", "success")
                setShowModal(false)
                refetchRejected()
                refetchCancelled()
            } else {
                const errorMessage = response.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                toastNotify(errorMessage, "error")
            }
        } catch (error) {
            console.error("Failed to reassign staff:", error)
            toastNotify("Có lỗi xảy ra khi cập nhật!", "error")
        }
    }

    // Hàm xử lý mở modal gán lại ca trực
    const handleOpenReassignModal = (assignment: RejectedAssignment) => {
        setSelectedAssignment(assignment)
        setShowModal(true)

        // Lấy danh sách nhân viên rảnh vào ngày của ca trực bị từ chối
        getStaffFreeTimes({ dateTime: assignment.date })
    }

    // xử lý phân trang và lựa chọn hàng
    const handleSelectedRowsChange = (state: any) => {
        setSelectedRows(state.selectedRows)
    }

    const handlePageChange = (page: number) => {
        setCurrentPage(page)
    }

    const handlePerRowsChange = (newPerPage: number, page: number) => {
        setPerPage(newPerPage)
        setCurrentPage(page)
    }

    useEffect(() => {
        setSelectedRows([])
        setClearRowsFlag(true)
    }, [currentCourse])

    useEffect(() => {
        if (clearRowsFlag) {
            setClearRowsFlag(false)
        }
    }, [clearRowsFlag])

    // Lọc danh sách ca trực theo trạng thái
    const filteredAssignments = useMemo(() => {
        if (statusFilter === "all") {
            return combinedAssignments
        } else if (statusFilter === "approved") {
            return combinedAssignments?.filter(
                (assignment) => assignment.status === SD_NightShiftAssignmentStatus.cancelled,
            )
        } else if (statusFilter === "not_approved") {
            return combinedAssignments?.filter(
                (assignment) => assignment.status === SD_NightShiftAssignmentStatus.rejected,
            )
        }
        return combinedAssignments
    }, [combinedAssignments, statusFilter])

    const customStylesTable = {
        headCells: {
            style: {
                fontSize: "15px",
                fontWeight: "bold",
            },
        },
        rows: {
            style: {
                fontSize: "15px",
            },
        },
    }

    // Cấu hình các cột cho DataTable
    const columns: TableColumn<RejectedAssignment>[] = [
        {
            name: "#",
            width: "60px",
            cell: (_row, index) => (currentPage - 1) * perPage + index + 1,
            ignoreRowClick: true,
            allowOverflow: true,
            button: false,
        },
        {
            name: "Nhân viên",
            selector: (row) => row.user.userName || "",
            sortable: true,
            cell: (row) => (
                <span data-bs-toggle="tooltip" data-bs-placement="top" title={row.user.userName || "N/A"}>
                    {(row.user.userName?.length || 0) > 50
                        ? `${row.user.userName.substring(0, 47)}...`
                        : row.user.userName || "N/A"}
                </span>
            ),
        },
        {
            name: "Ngày",
            selector: (row) => row.date.toString() || "",
            sortable: true,
            cell: (row) => {
                const date = new Date(row.date ?? "")
                return isNaN(date.getTime()) ? "Không hợp lệ" : format(date, "dd/MM/yyyy")
            },
        },
        {
            name: "Phòng",
            selector: (row) => row.room.name || "",
            sortable: true,
            cell: (row) => row.room.name || "N/A",
        },
        {
            name: "Ca trực",
            selector: (row) =>
                `${row.nightShift.startTime.substring(0, 5)} - ${row.nightShift.endTime.substring(0, 5)}`,
            sortable: true,
        },
        {
            name: "Trạng thái",
            selector: (row) => row.rejectionReason || "",
            sortable: false,
            cell: (row) =>
                row.status === SD_NightShiftAssignmentStatus.rejected ? (
                    <span className="badge bg-warning">Chưa duyệt</span>
                ) : (
                    <span className="badge bg-success">Đã duyệt</span>
                ),
            center: true,
        },
        {
            name: "Thao tác",
            width: "12rem",
            cell: (row) => (
                <div className="d-flex justify-content-around">
                    <button
                        className="btn btn-primary m-1"
                        onClick={() => handleOpenReassignModal(row)}
                        disabled={isCourseEnded()}
                    >
                        Chi tiết
                    </button>
                </div>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
        },
    ]

    // Hook để lấy assignment theo ID từ URL
    const {
        data: assignmentByIdData,
        isLoading: isAssignmentByIdLoading,
        error: assignmentByIdError,
    } = useGetAssignmentByIdQuery(parsedId!, {
        skip: !parsedId,
    })

    useEffect(() => {
        if (assignmentByIdData?.result) {
            const assignment: RejectedAssignment = assignmentByIdData.result
            if (
                assignment.status !== SD_NightShiftAssignmentStatus.rejected &&
                assignment.status !== SD_NightShiftAssignmentStatus.cancelled
            ) {
                return
            }
            setSelectedAssignment(assignment)
            setShowModal(true)
            getStaffFreeTimes({ dateTime: assignment.date })
        }
    }, [assignmentByIdData, getStaffFreeTimes])

    // Kiểm tra trạng thái tải dữ liệu
    const isLoadingCombined = isLoadingRejected || isLoadingCancelled || (staffLoading && showModal)

    if (isLoadingCombined || isAssignmentByIdLoading || listUserLoading) {
        return <MainLoader />
    }

    if (parsedId && assignmentByIdError) {
        return <Navigate to="/not-found" replace />
    }

    return (
        <div className="container mt-4">
            <div className="mt-0 mb-4">
                <h3 className="fw-bold primary-color">Danh sách ca trực bị từ chối và bị hủy</h3>
            </div>

            <div className="row mb-4 align-items-end">
                <div className="col-md-3 mb-3 mb-md-0">
                    <Form.Group controlId="courseSelect">
                        <Form.Label>Khóa Tu</Form.Label>
                        <Form.Control
                            as="select"
                            value={selectedCourseId || ""}
                            onChange={(e) => setSelectedCourseId(Number(e.target.value))}
                        >
                            {listCourseFromStore?.map((course) => (
                                <option key={course.id} value={course.id}>
                                    {course.courseName}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                </div>
                <div className="col-md-2">
                    <Form.Group controlId="dateSelect">
                        <div className="mb-2">Ngày</div>
                        <DatePicker
                            selected={selectedDate}
                            onChange={(date: Date | null) => setSelectedDate(date)}
                            minDate={currentCourse?.startDate ? parseISO(currentCourse.startDate) : undefined}
                            maxDate={currentCourse?.endDate ? parseISO(currentCourse.endDate) : undefined}
                            dateFormat="dd/MM/yyyy"
                            className="form-control"
                            placeholderText="Chọn ngày..."
                            disabled={!selectedCourseId}
                        />
                    </Form.Group>
                </div>
                <div className="col-md-3">
                    <Form.Group controlId="statusFilter">
                        <Form.Label>Trạng thái</Form.Label>
                        <Form.Control
                            as="select"
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value)}
                        >
                            {statusFilterOptions?.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                </div>
            </div>

            <div className="card">
                <div className="card-body">
                    <DataTable
                        columns={columns}
                        data={filteredAssignments}
                        customStyles={customStylesTable}
                        pagination
                        paginationTotalRows={filteredAssignments.length}
                        onChangePage={handlePageChange}
                        onChangeRowsPerPage={handlePerRowsChange}
                        noDataComponent={"Không có đơn nào"}
                        onSelectedRowsChange={handleSelectedRowsChange}
                        selectableRowsHighlight
                        selectableRowsSingle={false}
                        clearSelectedRows={clearRowsFlag}
                    />
                </div>
            </div>

            {/* Modal gán lại ca trực */}
            <Modal show={showModal} onHide={() => setShowModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Gán lại ca trực</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {selectedAssignment && (
                        <>
                            <p>
                                <strong>Nhân viên:</strong> {selectedAssignment.user.userName}
                            </p>
                            <p>
                                <strong>Họ tên:</strong> {selectedAssignment.user.fullName}
                            </p>
                            <p>
                                <strong>Điện thoại:</strong> {selectedAssignment.user?.phoneNumber}
                            </p>
                            <p>
                                <strong>Email:</strong> {selectedAssignment.user?.email}
                            </p>
                            <hr />
                            <p>
                                <strong>Phòng:</strong> {selectedAssignment.room.name}
                            </p>
                            <p>
                                <strong>Ca trực:</strong> {selectedAssignment.nightShift.startTime.substring(0, 5)} -{" "}
                                {selectedAssignment.nightShift.endTime.substring(0, 5)}
                            </p>
                            <p>
                                <strong>Ngày:</strong> {format(parseISO(selectedAssignment.date), "dd/MM/yyyy")}
                            </p>
                            <p>
                                <strong>Trạng thái:</strong>{" "}
                                <span
                                    className={`${
                                        selectedAssignment.status === SD_NightShiftAssignmentStatus.rejected
                                            ? "text-danger"
                                            : "text-success"
                                    }`}
                                >
                                    {selectedAssignment.status === SD_NightShiftAssignmentStatus.rejected
                                        ? "Chờ duyệt"
                                        : "Đã duyệt"}
                                </span>
                            </p>
                            <p>
                                <strong>Lý do:</strong> {selectedAssignment.rejectionReason || "Không có"}
                            </p>
                            {selectedAssignment?.status === SD_NightShiftAssignmentStatus.rejected && (
                                <>
                                    <Form.Group controlId="reassignStaffSelect">
                                        <Form.Label>Chọn nhân viên để gán lại</Form.Label>
                                        <Select
                                            options={filteredStaffOptions} // Use filtered options
                                            value={reassignStaff}
                                            onChange={(option) => setReassignStaff(option)}
                                            placeholder="Chọn nhân viên..."
                                            isClearable
                                            isDisabled={filteredStaffOptions.length === 0} // Disable if no staff available
                                            noOptionsMessage={() => "Không có nhân viên phù hợp"} // Custom no options message
                                        />
                                        {filteredStaffOptions.length === 0 && (
                                            <p className="text-danger mt-2">
                                                Không có nhân viên nào trống hoặc phù hợp để gán lại.
                                            </p>
                                        )}
                                    </Form.Group>
                                </>
                            )}
                            {selectedAssignment?.status === SD_NightShiftAssignmentStatus.cancelled && (
                                <footer>
                                    <p>
                                        <strong>Ngày duyệt: </strong>
                                        {format(parseISO(selectedAssignment.dateModified), "dd/MM/yyyy")}
                                    </p>
                                    <p>
                                        <strong>Người duyệt: </strong>
                                        {listUser?.result?.find(
                                            (user) => user.id === Number(selectedAssignment.updatedBy),
                                        )
                                            ? `${
                                                  listUser.result.find(
                                                      (user) => user.id === Number(selectedAssignment.updatedBy),
                                                  )?.userName
                                              } - ${
                                                  listUser.result.find(
                                                      (user) => user.id === Number(selectedAssignment.updatedBy),
                                                  )?.fullName
                                              }`
                                            : ""}
                                    </p>
                                </footer>
                            )}
                        </>
                    )}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowModal(false)}>
                        Đóng
                    </Button>
                    {selectedAssignment?.status === SD_NightShiftAssignmentStatus.rejected && (
                        <Button variant="primary" onClick={handleReassign} disabled={!reassignStaff || isReassigning}>
                            {isReassigning ? (
                                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                            ) : (
                                "Cập nhật"
                            )}
                        </Button>
                    )}
                </Modal.Footer>
            </Modal>

            {/* Confirmation Popup */}
            <ConfirmationPopup
                isOpen={isConfirmOpen}
                onClose={() => setIsConfirmOpen(false)}
                onConfirm={() => {
                    confirmAction()
                    setIsConfirmOpen(false)
                }}
                message={confirmMessage}
                title="Xác nhận thay đổi"
            />
        </div>
    )
}

export default RejectedNightShiftAssignments
