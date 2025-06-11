import React, { useEffect, useState } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import FullCalendar from "@fullcalendar/react"
import dayGridPlugin from "@fullcalendar/daygrid"
import interactionPlugin from "@fullcalendar/interaction"
import { Modal, Button, Badge, Form } from "react-bootstrap"
import toastNotify from "../../helper/toastNotify"
import { useGetMyNightShiftsQuery, useUpdateAssignmentStatusMutation } from "../../apis/nightShiftAssignmentApi"
import nightShiftAssignmentModel from "../../interfaces/nightShiftAssignmentModel"
import { SD_NightShiftAssignmentStatus, SD_NightShiftAssignmentStatus_Name } from "../../utility/SD"
import viLocale from "@fullcalendar/core/locales/vi"
import "../../assets/css/customFullCalendar.css"
import { MainLoader } from "../../components/Page"
import { useNavigate } from "react-router-dom"
import { apiResponse } from "../../interfaces"
import { useGetReportQuery } from "../../apis/reportApi"
import { parseISO, format } from "date-fns"

function MyNightShift() {
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const selectedCourseId = currentCourse?.id
    const [updateAssignmentStatus] = useUpdateAssignmentStatusMutation()

    const {
        data: shiftData,
        isLoading,
        error,
        refetch,
    } = useGetMyNightShiftsQuery(
        { userId: currentUserId || 0 },
        { skip: !selectedCourseId || !currentUserId, refetchOnMountOrArgChange: true },
    )

    const [showShiftDetailModal, setShowShiftDetailModal] = useState(false)
    const [selectedShift, setSelectedShift] = useState<nightShiftAssignmentModel | null>(null)
    const [rejectionReason, setRejectionReason] = useState("")
    const [isRejecting, setIsRejecting] = useState(false)

    const navigate = useNavigate()

    const today = new Date().toISOString().split("T")[0]

    const formatDateOnly = (dateTimeString: string) => dateTimeString.split("T")[0]

    const formatDateTime = (dateTimeString: string, timeString: string) => {
        const datePart = dateTimeString.split("T")[0]
        return `${datePart}T${timeString}`
    }

    const getStatusName = (status: SD_NightShiftAssignmentStatus): string => {
        switch (status) {
            case SD_NightShiftAssignmentStatus.notStarted:
                return SD_NightShiftAssignmentStatus_Name.notStarted
            case SD_NightShiftAssignmentStatus.completed:
                return SD_NightShiftAssignmentStatus_Name.completed
            case SD_NightShiftAssignmentStatus.rejected:
                return SD_NightShiftAssignmentStatus_Name.rejected
            case SD_NightShiftAssignmentStatus.cancelled:
                return SD_NightShiftAssignmentStatus_Name.cancelled
            default:
                return "Không xác định"
        }
    }

    const getEventClassName = (status: SD_NightShiftAssignmentStatus): string => {
        switch (status) {
            case SD_NightShiftAssignmentStatus.notStarted:
                return "event-warning"
            case SD_NightShiftAssignmentStatus.completed:
                return "event-success"
            case SD_NightShiftAssignmentStatus.rejected:
                return "event-danger"
            case SD_NightShiftAssignmentStatus.cancelled:
                return "event-secondary"
            default:
                return ""
        }
    }

    const handleEventClick = (clickInfo: any) => {
        const shiftId = parseInt(clickInfo.event.id, 10)
        const shift = shiftData?.result?.find((s) => s.id === shiftId)
        if (shift) {
            setSelectedShift(shift)
            setShowShiftDetailModal(true)
        }
    }

    const handleCloseModal = () => {
        setShowShiftDetailModal(false)
        setSelectedShift(null)
        setIsRejecting(false)
        setRejectionReason("")
    }

    const handleReject = () => {
        setIsRejecting(true)
    }

    const handleSaveRejection = async () => {
        if (selectedShift) {
            try {
                const response: apiResponse = await updateAssignmentStatus({
                    id: selectedShift.id,
                    status: SD_NightShiftAssignmentStatus.rejected,
                    rejectionReason: rejectionReason,
                })
                if (response.data?.isSuccess) {
                    toastNotify("Hủy ca trực thành công!", "success")
                    handleCloseModal()
                    refetch()
                } else {
                    const errorMessage = response.error.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                    toastNotify(errorMessage, "error")
                }
            } catch {
                toastNotify("Có lỗi xảy ra khi cập nhật!", "error")
            }
        }
    }

    const handleCancelRejection = () => {
        setIsRejecting(false)
        setRejectionReason("")
    }

    const handleUndoRejection = async () => {
        if (selectedShift) {
            try {
                const response: apiResponse = await updateAssignmentStatus({
                    id: selectedShift.id,
                    status: SD_NightShiftAssignmentStatus.notStarted,
                    rejectionReason: rejectionReason,
                })
                if (response.data?.isSuccess) {
                    toastNotify("Đã hủy thành công!", "success")
                    handleCloseModal()
                    refetch()
                } else {
                    const errorMessage = response.error.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                    toastNotify(errorMessage, "error")
                }
            } catch (error: any) {
                const errorMsg = error?.data?.result || "Có lỗi xảy ra khi cập nhật!"
                toastNotify(errorMsg, "error")
            }
        }
    }

    const isAtLeastThreeDaysBefore = (shiftDate: string): boolean => {
        const shift = new Date(shiftDate)
        const today = new Date()
        // Create a date 3 days in the future
        const threeDaysLater = new Date()
        threeDaysLater.setDate(today.getDate() + 3)
        return shift >= threeDaysLater
    }

    const canAccessShift = (shift: nightShiftAssignmentModel): boolean => {
        if (shift.status === SD_NightShiftAssignmentStatus.cancelled) {
            return false
        }

        const shiftDate = new Date(shift.date)
        const todayDate = new Date()

        shiftDate.setHours(0, 0, 0, 0)
        todayDate.setHours(0, 0, 0, 0)

        if (shiftDate > todayDate) {
            return false
        }

        return true
    }

    const { data: reportData, isLoading: reportLoading } = useGetReportQuery(
        {
            reportType: 0,
            reportDate: selectedShift?.date,
            roomId: selectedShift?.roomId,
            nightShiftId: selectedShift?.nightShiftId,
        },
        { skip: !selectedShift },
    )
    const handleAccessShift = () => {
        if (!reportLoading && reportData?.result) {
            navigate(`/report/${reportData?.result[0].id}`)
        }
    }

    const events =
        shiftData?.result?.map((shift) => {
            const shiftDate = shift.date.split("T")[0] // Extract date part
            const startTimeString = shift.nightShift?.startTime || "00:00:00"
            const endTimeString = shift.nightShift?.endTime || "00:00:00"

            // Create start and end Date objects
            const startDateTime = new Date(`${shiftDate}T${startTimeString}`)
            let endDateTime = new Date(`${shiftDate}T${endTimeString}`)

            // If end time is before or equal to start time, add one day to end date
            if (endDateTime <= startDateTime) {
                endDateTime.setDate(endDateTime.getDate() + 1)
            }

            return {
                id: shift.id.toString(),
                title: `Phòng ${shift.room.name}`,
                start: startDateTime.toISOString(),
                end: endDateTime.toISOString(),
                classNames: [getEventClassName(shift.status)],
                extendedProps: {
                    ...shift,
                },
            }
        }) || []

    const renderEventContent = (eventInfo: any) => {
        const statusColor = {
            [SD_NightShiftAssignmentStatus.notStarted]: "warning",
            [SD_NightShiftAssignmentStatus.completed]: "success",
            [SD_NightShiftAssignmentStatus.rejected]: "danger",
            [SD_NightShiftAssignmentStatus.cancelled]: "secondary",
        }
        const startTime = eventInfo.event.start
            ? new Date(eventInfo.event.start).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            : ""
        const endTime = eventInfo.event.end
            ? new Date(eventInfo.event.end).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            : ""
        return (
            <div className="custom-event">
                <span className="event-time">
                    {startTime} - {endTime}
                </span>
                <span className="event-title">{eventInfo.event.title}</span>
                <Badge bg={statusColor[eventInfo.event.extendedProps.status]} className="mt-1">
                    {getStatusName(eventInfo.event.extendedProps.status)}
                </Badge>
            </div>
        )
    }

    if (isLoading || !shiftData) {
        return <MainLoader />
    }

    return (
        <div className="container mt-4">
            {error && <div className="text-danger">Đã xảy ra lỗi khi tải dữ liệu ca trực.</div>}
            {!isLoading && !error && (
                <FullCalendar
                    plugins={[dayGridPlugin, interactionPlugin]}
                    initialView="dayGridMonth"
                    headerToolbar={{
                        left: "prev,next today",
                        center: "title",
                        right: "",
                    }}
                    events={events}
                    eventClick={handleEventClick}
                    locale={viLocale}
                    height="auto"
                    eventContent={renderEventContent}
                />
            )}

            <Modal show={showShiftDetailModal} onHide={handleCloseModal} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Chi Tiết Ca Trực</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {selectedShift ? (
                        <div>
                            <p>
                                <strong>Phòng:</strong> {selectedShift.room.name}
                            </p>
                            <p>
                                <strong>Thời Gian Bắt Đầu:</strong> {selectedShift.nightShift?.startTime}
                            </p>
                            <p>
                                <strong>Thời Gian Kết Thúc:</strong> {selectedShift.nightShift?.endTime}
                            </p>
                            <p>
                                <strong>Ngày:</strong> {format(parseISO(selectedShift.date), "dd/MM/yyyy")}
                            </p>
                            <p>
                                <strong>Trạng Thái:</strong> {getStatusName(selectedShift.status)}
                            </p>
                            {selectedShift.status === SD_NightShiftAssignmentStatus.rejected ||
                                (selectedShift.status == SD_NightShiftAssignmentStatus.cancelled && (
                                    <p>
                                        <strong>Lý Do Từ Chối:</strong> {selectedShift.rejectionReason}
                                    </p>
                                ))}

                            {canAccessShift(selectedShift) && (
                                <Button variant="info" className="mt-2 me-2" onClick={handleAccessShift}>
                                    Truy cập
                                </Button>
                            )}

                            {isRejecting ? (
                                <>
                                    <Form.Group controlId="rejectionReason">
                                        <Form.Label>Lý Do Từ Chối</Form.Label>
                                        <Form.Control
                                            as="textarea"
                                            rows={3}
                                            value={rejectionReason}
                                            onChange={(e) => setRejectionReason(e.target.value)}
                                        />
                                    </Form.Group>
                                    <Button
                                        variant="primary"
                                        className="mt-2 me-2"
                                        onClick={handleSaveRejection}
                                        disabled={!rejectionReason.trim()}
                                    >
                                        Gửi
                                    </Button>
                                    <Button variant="secondary" className="mt-2" onClick={handleCancelRejection}>
                                        Hủy
                                    </Button>
                                </>
                            ) : (
                                <>
                                    {selectedShift.status === SD_NightShiftAssignmentStatus.rejected &&
                                    formatDateOnly(selectedShift.date) !== today &&
                                    !canAccessShift(selectedShift) &&
                                    isAtLeastThreeDaysBefore(selectedShift.date) ? (
                                        <Button variant="warning" onClick={handleUndoRejection}>
                                            Hủy Từ Chối
                                        </Button>
                                    ) : (
                                        formatDateOnly(selectedShift.date) !== today &&
                                        !canAccessShift(selectedShift) &&
                                        selectedShift.status != SD_NightShiftAssignmentStatus.cancelled &&
                                        isAtLeastThreeDaysBefore(selectedShift.date) && (
                                            <Button variant="danger" onClick={handleReject}>
                                                Từ chối
                                            </Button>
                                        )
                                    )}
                                </>
                            )}
                        </div>
                    ) : (
                        <div>Không có dữ liệu.</div>
                    )}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseModal}>
                        Đóng
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default MyNightShift
