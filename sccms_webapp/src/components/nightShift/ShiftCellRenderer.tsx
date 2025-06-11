import React, { useState, useMemo } from "react"
import { Button, Spinner, Modal, Badge } from "react-bootstrap"
import Select from "react-select"
import nightShiftAssignmentModel from "../../interfaces/nightShiftAssignmentModel"
import { SD_NightShiftAssignmentStatus, SD_ReportStatus, SD_Role, SD_Role_Name } from "../../utility/SD"
import { useNavigate } from "react-router-dom"
import {
    useAssignStaffToShiftMutation,
    useReassignStaffToShiftMutation, // Thêm mutation mới
    useDeleteAssignmentMutation,
    useSuggestStaffForShiftQuery,
} from "../../apis/nightShiftAssignmentApi"
import { toastNotify } from "../../helper"
import { parseISO, format } from "date-fns"
import { apiResponse } from "../../interfaces"
import { useGetReportQuery } from "../../apis/reportApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"

interface ShiftCellRendererProps {
    assignments: nightShiftAssignmentModel[]
    roomData: any
    shiftId: number
    dateString: string | null
    isPastDate: boolean
    isFuture: boolean
    courseId: number
}

const getStatusBadge = (status: SD_ReportStatus): JSX.Element => {
    const statusMap: { [key in SD_ReportStatus]: { color: string; text: string } } = {
        [SD_ReportStatus.NotYet]: { color: "secondary", text: "Chưa nộp" },
        [SD_ReportStatus.Attending]: { color: "warning", text: "Đang mở" },
        [SD_ReportStatus.Attended]: { color: "success", text: "Đã nộp" },
        [SD_ReportStatus.Late]: { color: "danger", text: "Muộn" },
        [SD_ReportStatus.Reopened]: { color: "info", text: "Mở Lại" },
        [SD_ReportStatus.Read]: { color: "dark", text: "Đã xem" },
    }

    const currentStatus = statusMap[status] || { color: "light", text: "Không Xác Định" }

    return <Badge bg={currentStatus.color}>{currentStatus.text}</Badge>
}

export const ShiftCellRenderer: React.FC<ShiftCellRendererProps> = ({
    assignments,
    roomData,
    shiftId,
    dateString,
    isPastDate,
    isFuture,
    courseId,
}) => {
    const roomId = roomData.roomId
    const numberOfStaff = roomData.numberOfStaff
    const navigate = useNavigate()

    const [selectedStaff, setSelectedStaff] = useState<Array<{ value: number; label: string }>>([])
    const [showModal, setShowModal] = useState(false)
    const [showConfirmDelete, setShowConfirmDelete] = useState(false) // State cho modal xác nhận xóa
    const [selectedAssignment, setSelectedAssignment] = useState<nightShiftAssignmentModel | null>(null)
    const [isDropdownOpen, setIsDropdownOpen] = useState(false)
    const [isModalDropdownOpen, setIsModalDropdownOpen] = useState(false) // State cho dropdown trong Modal
    const [isEditing, setIsEditing] = useState(false) // State để theo dõi chế độ chỉnh sửa
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    const [assignStaffToShift, { isLoading: isAssigning }] = useAssignStaffToShiftMutation()
    const [reassignStaffToShift, { isLoading: isReassigning }] = useReassignStaffToShiftMutation() // Thêm mutation mới
    const [deleteShiftAssignment, { isLoading: isDeleting }] = useDeleteAssignmentMutation()

    // Điều chỉnh để gọi API khi bất kỳ Select nào Đang mở dropdown
    const { data: suggestedStaffData, isLoading: isSuggestLoading } = useSuggestStaffForShiftQuery(
        {
            date: dateString!,
            shiftId: shiftId,
            roomId: roomId,
            courseId: courseId,
        },
        {
            skip: !(isDropdownOpen || isModalDropdownOpen) || !dateString || isPastDate,
        },
    )
    const staffOptions = useMemo(
        () =>
            suggestedStaffData
                ? suggestedStaffData.result
                      ?.filter((user: any) => user.id !== (selectedAssignment?.user?.id || 0))
                      ?.map((user: any) => ({
                          value: user.id,
                          label: `${user.userName} - ${user.fullName}`,
                      }))
                : [],
        [suggestedStaffData, selectedAssignment],
    )
    // const { data: reportData, isLoading: reportLoading } = useGetReportQuery(
    //     {
    //         reportType: 0,
    //         reportDate: assignments[0]?.date,
    //         roomId: assignments[0]?.roomId,
    //         nightShiftId: assignments[0]?.nightShiftId,
    //     },
    //     { skip: !assignments },
    // )
    const { data: reportData, isLoading: reportLoading } = useGetReportQuery(
        {
            reportType: 0,
            reportDate: dateString,
            roomId: roomId,
            nightShiftId: shiftId,
        },
        { skip: !assignments },
    )
    const handleAccessShift = () => {
        if (!reportLoading && reportData?.result) {
            navigate(`/report/${reportData?.result[0].id}`)
        }
    }

    // State cho Select trong Modal
    const [selectedNewStaff, setSelectedNewStaff] = useState<{ value: number; label: string } | null>(null)

    const handleSave = async () => {
        const payload = {
            NightShiftId: shiftId,
            UserIds: selectedStaff?.map((staff) => staff.value),
            RoomId: roomId,
            Date: dateString!,
        }
        if (payload.UserIds.length === 0) {
            toastNotify("Vui lòng chọn nhân viên!", "error")
            return
        }

        try {
            await assignStaffToShift(payload).unwrap()
            toastNotify("Thêm nhân viên thành công!", "success")
            setSelectedStaff([])
            setIsEditing(false)
        } catch (error) {
            console.error("Failed to assign staff:", error)
            toastNotify("Có lỗi xảy ra khi thêm nhân viên.", "error")
        }
    }

    const handleDelete = async (assignmentId: number) => {
        try {
            await deleteShiftAssignment({ id: assignmentId }).unwrap()
            toastNotify("Xóa ca trực thành công!", "success")
            setShowModal(false)
            setShowConfirmDelete(false)
        } catch (error) {
            console.error("Failed to delete shift:", error)
            toastNotify("Có lỗi xảy ra khi xóa ca trực.", "error")
        }
    }

    const handleUpdate = async () => {
        if (!selectedAssignment || !selectedNewStaff) {
            toastNotify("Vui lòng chọn nhân viên mới để cập nhật!", "error")
            return
        }
        try {
            const response: apiResponse = await reassignStaffToShift({
                Id: selectedAssignment.id,
                newUserId: selectedNewStaff.value,
            })
            if (response.data?.isSuccess) {
                toastNotify("Cập nhật nhân viên thành công!", "success")
                setShowModal(false)
                setSelectedAssignment(null)
                setSelectedNewStaff(null)
            } else {
                const errorMessage = response.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                toastNotify(errorMessage, "error")
            }
        } catch (error) {
            console.error("Failed to reassign staff:", error)
            toastNotify("Có lỗi xảy ra khi cập nhật!", "error")
        }
    }

    const handleOpenModal = (assignment: nightShiftAssignmentModel) => {
        setSelectedAssignment(assignment)
        setShowModal(true)
    }

    const handleOpenConfirmDelete = () => {
        setShowModal(false)
        setShowConfirmDelete(true)
    }

    const handleCloseConfirmDelete = () => {
        setShowConfirmDelete(false)
    }

    const handleEdit = () => {
        setIsEditing(true)
    }

    const handleCancelEdit = () => {
        setIsEditing(false)
        setSelectedStaff([])
    }

    const availableStaffOptions = useMemo(() => {
        const assignedStaffIds = assignments.map((assignment) => assignment?.user?.id)
        return staffOptions.filter((option) => !assignedStaffIds.includes(option.value))
    }, [staffOptions, assignments])

    return (
        <div style={{ padding: "10px", display: "flex", flexDirection: "column", height: "100%" }}>
            {/* Hiển thị danh sách nhân viên đã phân công */}
            <div style={{ flexGrow: 1 }}>
                {assignments.length > 0 ? (
                    <ul style={{ listStyleType: "none", paddingLeft: 0 }}>
                        {assignments?.map((assignment) => (
                            <li
                                key={assignment.id}
                                className={`rounded p-1 m-1 text-center ${
                                    assignment.status === SD_NightShiftAssignmentStatus.rejected
                                        ? "bg-danger text-white"
                                        : "bg-light text-dark"
                                }`}
                                onClick={() => handleOpenModal(assignment)}
                                style={{
                                    display: "inline-block",
                                    cursor: "pointer",
                                    border: "1px solid #ccc",
                                    minWidth: "70px",
                                }}
                            >
                                {assignment.user?.userName ?? "N/A"}
                            </li>
                        ))}
                    </ul>
                ) : (
                    <span className="text-muted d-block">Chưa có nhân viên trong ca</span>
                )}
                {isPastDate && getStatusBadge(reportData?.result[0].status)}
                {!isPastDate &&
                    !isEditing &&
                    assignments.length >= numberOfStaff &&
                    !isFuture &&
                    getStatusBadge(reportData?.result[0].status)}
            </div>

            {/* Hiển thị phần dưới cùng */}
            <div>
                {!isPastDate && (
                    <>
                        <div className={`mt-2 ${assignments.length < numberOfStaff ? "text-danger" : "text-success"}`}>
                            Số lượng: {assignments.length}/{numberOfStaff}
                        </div>
                        {!isEditing && assignments.length >= numberOfStaff ? (
                            <div className="d-flex justify-content-between mt-2">
                                {(currentUserRole == SD_Role_Name.SECRETARY ||
                                    currentUserRole == SD_Role_Name.MANAGER) && (
                                    <Button variant="light" size="sm" onClick={handleEdit} className="me-2">
                                        <i className="bi bi-plus-lg"></i> Thêm
                                    </Button>
                                )}
                                {!isFuture && (
                                    <Button variant="primary" size="sm" onClick={() => handleAccessShift()}>
                                        Báo cáo
                                    </Button>
                                )}
                            </div>
                        ) : (
                            <>
                                {isEditing && (
                                    <div className="mt-2">
                                        <Select
                                            isMulti
                                            options={availableStaffOptions}
                                            value={selectedStaff} // @ts-ignore
                                            onChange={(options) => setSelectedStaff(options || [])}
                                            placeholder="Chọn nhân viên mới..."
                                            isClearable={true}
                                            onMenuOpen={() => setIsDropdownOpen(true)}
                                            onMenuClose={() => setIsDropdownOpen(false)}
                                            isLoading={isSuggestLoading}
                                            noOptionsMessage={() =>
                                                isSuggestLoading ? "Đang tải..." : "Không có nhân viên phù hợp"
                                            }
                                        />
                                        <div className="mt-2 d-flex">
                                            <Button
                                                variant="secondary"
                                                size="sm"
                                                onClick={handleCancelEdit}
                                                disabled={isAssigning}
                                                className="me-2"
                                            >
                                                Hủy
                                            </Button>
                                            <Button
                                                variant="primary"
                                                size="sm"
                                                onClick={handleSave}
                                                disabled={isAssigning}
                                            >
                                                {isAssigning ? (
                                                    <Spinner
                                                        as="span"
                                                        animation="border"
                                                        size="sm"
                                                        role="status"
                                                        aria-hidden="true"
                                                    />
                                                ) : (
                                                    "Thêm nhân viên"
                                                )}
                                            </Button>
                                        </div>
                                    </div>
                                )}
                                {assignments.length < numberOfStaff &&
                                    (currentUserRole == SD_Role_Name.SECRETARY ||
                                        currentUserRole == SD_Role_Name.MANAGER) && (
                                        <div className="mt-2">
                                            <Select
                                                isMulti
                                                options={availableStaffOptions}
                                                value={selectedStaff} // @ts-ignore
                                                onChange={(options) => setSelectedStaff(options || [])}
                                                placeholder="Chọn nhân viên mới..."
                                                isClearable={true}
                                                onMenuOpen={() => setIsDropdownOpen(true)}
                                                onMenuClose={() => setIsDropdownOpen(false)}
                                                isLoading={isSuggestLoading}
                                                noOptionsMessage={() =>
                                                    isSuggestLoading ? "Đang tải..." : "Không có nhân viên phù hợp"
                                                }
                                            />
                                            <div className="mt-2">
                                                <Button
                                                    variant="primary"
                                                    size="sm"
                                                    onClick={handleSave}
                                                    disabled={isAssigning}
                                                >
                                                    {isAssigning ? (
                                                        <Spinner
                                                            as="span"
                                                            animation="border"
                                                            size="sm"
                                                            role="status"
                                                            aria-hidden="true"
                                                        />
                                                    ) : (
                                                        "Thêm nhân viên"
                                                    )}
                                                </Button>
                                            </div>
                                        </div>
                                    )}
                            </>
                        )}
                    </>
                )}
                {isPastDate && (
                    <div className="mt-2">
                        {!isFuture && (
                            <Button variant="primary" size="sm" onClick={() => handleAccessShift()}>
                                Báo cáo
                            </Button>
                        )}
                    </div>
                )}
            </div>

            {/* Modal hiển thị chi tiết ca trực */}
            <Modal show={showModal} onHide={() => setShowModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Chi tiết Ca trực</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {selectedAssignment && (
                        <>
                            <p>
                                <strong>Nhân viên:</strong> {selectedAssignment.user?.fullName}
                            </p>
                            <p>
                                <strong>Tài khoản:</strong> {selectedAssignment.user?.userName}
                            </p>
                            <p>
                                <strong>Điện thoại:</strong> {selectedAssignment.user?.phoneNumber}
                            </p>
                            <p>
                                <strong>Email:</strong> {selectedAssignment.user?.email}
                            </p>
                            <hr />
                            <p>
                                <strong>Phòng:</strong> {selectedAssignment.room?.name}
                            </p>
                            <p>
                                <strong>Ca trực:</strong> {selectedAssignment.nightShift?.startTime.substring(0, 5)} -{" "}
                                {selectedAssignment.nightShift?.endTime.substring(0, 5)}
                            </p>
                            <p>
                                <strong>Trạng thái:</strong>{" "}
                                {selectedAssignment.status === SD_NightShiftAssignmentStatus.rejected
                                    ? "Bị từ chối"
                                    : "Đã phân công"}
                            </p>
                            {selectedAssignment.status === SD_NightShiftAssignmentStatus.rejected && (
                                <>
                                    <p>
                                        <strong>Lý do từ chối:</strong>{" "}
                                        {selectedAssignment.rejectionReason || "Không có"}
                                    </p>
                                    <Select
                                        options={availableStaffOptions}
                                        placeholder="Chọn nhân viên khác..."
                                        isClearable
                                        onMenuOpen={() => setIsModalDropdownOpen(true)}
                                        onMenuClose={() => setIsModalDropdownOpen(false)}
                                        onChange={(option) => {
                                            if (option && selectedAssignment) {
                                                setSelectedNewStaff(option)
                                            } else {
                                                setSelectedNewStaff(null)
                                            }
                                        }}
                                        value={selectedNewStaff}
                                    />
                                </>
                            )}
                        </>
                    )}
                </Modal.Body>
                <Modal.Footer className="d-flex justify-content-between">
                    {!isPastDate &&
                        (currentUserRole == SD_Role_Name.MANAGER || currentUserRole == SD_Role_Name.SECRETARY) && (
                            <button className="btn btn-danger" onClick={handleOpenConfirmDelete} disabled={isDeleting}>
                                Xóa
                            </button>
                        )}

                    <div>
                        {selectedAssignment && selectedAssignment.status === SD_NightShiftAssignmentStatus.rejected && (
                            <Button
                                variant="success"
                                onClick={handleUpdate}
                                disabled={isReassigning || !selectedNewStaff}
                                className="me-2"
                            >
                                {isReassigning ? (
                                    <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                                ) : (
                                    "Cập nhật"
                                )}
                            </Button>
                        )}
                        <Button variant="secondary" onClick={() => setShowModal(false)}>
                            Đóng
                        </Button>
                    </div>
                </Modal.Footer>
            </Modal>

            {/* Modal xác nhận xóa */}
            <Modal show={showConfirmDelete} onHide={handleCloseConfirmDelete}>
                <Modal.Header closeButton>
                    <Modal.Title>Xác nhận xóa</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {selectedAssignment && (
                        <>
                            Nhân viên: <strong>{selectedAssignment.user?.fullName}</strong>
                            <br />
                            Phòng: <strong>{selectedAssignment.room?.name}</strong>
                            <br />
                            Ca trực:{" "}
                            <strong>
                                {selectedAssignment.nightShift?.startTime.substring(0, 5)} -{" "}
                                {selectedAssignment.nightShift?.endTime.substring(0, 5)}
                            </strong>
                            <br />
                        </>
                    )}
                    Bạn có chắc chắn muốn xóa ca trực này không?
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseConfirmDelete}>
                        Hủy
                    </Button>
                    <Button
                        variant="danger"
                        onClick={() => selectedAssignment && handleDelete(selectedAssignment.id)}
                        disabled={isDeleting}
                    >
                        {isDeleting ? "Đang xóa..." : "Xác nhận"}
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}
