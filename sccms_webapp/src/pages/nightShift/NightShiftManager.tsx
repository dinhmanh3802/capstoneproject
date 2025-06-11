// src/components/NightShiftManager.tsx

import React, { useEffect, useState, useMemo } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { Form, Button } from "react-bootstrap"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { parseISO, isWithinInterval, format } from "date-fns"
import { useAutoAssignNightShiftsMutation, useGetAllNightShiftQuery } from "../../apis/nightShiftAssignmentApi"
import { useGetAllRoomsQuery } from "../../apis/roomApi"
import { useGetAllNightShiftsQuery } from "../../apis/nightShiftApi"
import { nightShiftModel } from "../../interfaces/nightShiftModel"
import { roomModel } from "../../interfaces/roomModel"
import { MainLoader } from "../../components/Page"
import { SD_Gender, SD_NightShiftAssignmentStatus, SD_Role_Name } from "../../utility/SD"
import { useGetAllStaffFreeTimesQuery } from "../../apis/staffFreeTimeApi"
import { ShiftCellRenderer } from "../../components/nightShift/ShiftCellRenderer"
import { toZonedTime, formatInTimeZone } from "date-fns-tz"
import "../../components/nightShift/ShiftCellRenderer.css"
import { Link, useNavigate, useSearchParams } from "react-router-dom"
import { toastNotify } from "../../helper"
import { apiResponse } from "../../interfaces"
import { useGetCourseByIdQuery } from "../../apis/courseApi"

const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

interface NightShiftAssignmentModel {
    id: number
    nightShiftId: number
    nightShift?: nightShiftModel
    userId?: number
    user?: { userId: number; userName: string }
    date: string
    roomId?: number
    room: roomModel
    status: SD_NightShiftAssignmentStatus
    rejectionReason?: string
}

function NightShiftManager() {
    const navigate = useNavigate()
    const [searchParams, setSearchParams] = useSearchParams()

    const [autoAssign] = useAutoAssignNightShiftsMutation()
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)

    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(undefined)
    const [selectedDate, setSelectedDate] = useState<Date | null>(null)
    const {
        data: courseData,
        isLoading: courseLoading,
        error: courseError,
    } = useGetCourseByIdQuery(selectedCourseId || 0, { skip: !selectedCourseId })
    // Kiểm tra xem khóa tu đã kết thúc chưa
    const isCourseInPast =
        courseData?.result && courseData?.result.endDate ? new Date(courseData?.result.endDate) < new Date() : false
    // Helper function để kiểm tra ngày hợp lệ
    const isValidDate = (date: Date | null): boolean => date !== null && !isNaN(date.getTime())

    // Hàm cập nhật selectedDate dựa trên khóa tu
    const updateSelectedDate = (course?: (typeof listCourseFromStore)[0], dateParam?: string) => {
        if (course?.startDate && course?.endDate) {
            const start = parseISO(course.startDate)
            const end = parseISO(course.endDate)
            let initialDate: Date | null = null

            if (dateParam) {
                const parsedDate = parseISO(dateParam)
                if (isValidDate(parsedDate) && isWithinInterval(parsedDate, { start, end })) {
                    initialDate = parsedDate
                }
            }

            if (!initialDate) {
                const today = new Date()
                initialDate = isWithinInterval(today, { start, end }) ? today : start
            }

            setSelectedDate(initialDate)

            // Cập nhật URL với ngày đã chọn
            if (initialDate) {
                setSearchParams({
                    courseId: course.id.toString(),
                    date: format(initialDate, "yyyy-MM-dd"),
                })
            }
        } else {
            setSelectedDate(null)
        }
    }

    // Khởi tạo trạng thái từ URL khi component mount
    useEffect(() => {
        const courseIdFromParams = searchParams.get("courseId")
        const dateFromParams = searchParams.get("date")

        if (courseIdFromParams) {
            const parsedCourseId = parseInt(courseIdFromParams, 10)
            const courseExists = listCourseFromStore.some((course) => course.id === parsedCourseId)
            if (courseExists) {
                setSelectedCourseId(parsedCourseId)
                const selectedCourse = listCourseFromStore.find((course) => course.id === parsedCourseId)
                updateSelectedDate(selectedCourse, dateFromParams || undefined)
            } else {
                // Nếu courseId không tồn tại, chọn khóa tu hiện tại hoặc khóa tu đầu tiên
                const defaultCourse =
                    currentCourse || (listCourseFromStore.length > 0 ? listCourseFromStore[0] : undefined)
                if (defaultCourse) {
                    setSelectedCourseId(defaultCourse.id)
                    updateSelectedDate(defaultCourse)
                }
            }
        } else if (currentCourse) {
            setSelectedCourseId(currentCourse.id)
            updateSelectedDate(currentCourse)
        } else if (listCourseFromStore.length > 0) {
            setSelectedCourseId(listCourseFromStore[0].id)
            updateSelectedDate(listCourseFromStore[0])
        }
    }, [listCourseFromStore, currentCourse, searchParams])

    // Cập nhật selectedDate khi selectedCourseId thay đổi
    useEffect(() => {
        const selectedCourse = listCourseFromStore.find((course) => course.id === selectedCourseId)
        if (selectedCourse) {
            const dateFromParams = searchParams.get("date")
            updateSelectedDate(selectedCourse, dateFromParams || undefined)
        } else {
            setSelectedDate(null)
        }
    }, [selectedCourseId, listCourseFromStore])

    // Cập nhật URL khi selectedDate thay đổi
    useEffect(() => {
        if (selectedCourseId && selectedDate && isValidDate(selectedDate)) {
            setSearchParams({
                courseId: selectedCourseId.toString(),
                date: format(selectedDate, "yyyy-MM-dd"),
            })
        }
    }, [selectedCourseId, selectedDate, setSearchParams])

    const dateString = selectedDate ? formatInTimeZone(new Date(selectedDate), VIETNAM_TZ, "yyyy-MM-dd") : null

    const { data: shiftAssignments, isLoading } = useGetAllNightShiftQuery(
        { courseId: selectedCourseId, dateTime: dateString },
        { skip: !selectedCourseId || !isValidDate(selectedDate) },
    )

    const { data: roomsData, isLoading: roomsLoading } = useGetAllRoomsQuery(selectedCourseId || 0, {
        skip: !selectedCourseId,
    })

    const { data: staffData, isLoading: staffLoading } = useGetAllStaffFreeTimesQuery(
        { dateTime: dateString },
        { skip: !selectedCourseId || !isValidDate(selectedDate) },
    )

    const { data: nightShiftsData, isLoading: nightShiftsLoading } = useGetAllNightShiftsQuery(selectedCourseId || 0, {
        skip: !selectedCourseId,
    })

    const courseOptions = useMemo(
        () =>
            listCourseFromStore?.map((course) => ({
                value: course.id,
                label: course.courseName,
            })),
        [listCourseFromStore],
    )

    const isAnyLoading = isLoading || roomsLoading || nightShiftsLoading || staffLoading || courseLoading

    const gridData = useMemo(() => {
        if (!roomsData?.result || !nightShiftsData?.result || !shiftAssignments?.result) return []

        const assignmentMap: Record<number, Record<number, NightShiftAssignmentModel[]>> = {}

        shiftAssignments.result.forEach((assignment) => {
            if (!assignment.roomId || !assignment.nightShiftId) return

            if (!assignmentMap[assignment.roomId]) {
                assignmentMap[assignment.roomId] = {}
            }

            if (!assignmentMap[assignment.roomId][assignment.nightShiftId]) {
                assignmentMap[assignment.roomId][assignment.nightShiftId] = []
            }

            assignmentMap[assignment.roomId][assignment.nightShiftId].push(assignment)
        })

        return roomsData.result.map((room: roomModel) => {
            const roomShifts: Record<number, NightShiftAssignmentModel[]> = {}

            nightShiftsData.result.forEach((shift: nightShiftModel) => {
                const assignments = assignmentMap[room.id]?.[shift.id] || []
                roomShifts[shift.id] = assignments
            })

            return {
                roomName: room.name,
                gender: room.gender === SD_Gender.Male ? "Nam" : "Nữ",
                numberOfStaff: room.numberOfStaff,
                roomId: room.id,
                shifts: roomShifts,
            }
        })
    }, [roomsData, nightShiftsData, shiftAssignments])

    const isPastDate = useMemo(() => {
        if (!selectedDate) return false
        const select = toZonedTime(new Date(selectedDate), VIETNAM_TZ)
        const today = toZonedTime(new Date(), VIETNAM_TZ)
        select.setHours(0, 0, 0, 0)
        today.setHours(0, 0, 0, 0)
        return select < today
    }, [selectedDate])

    const isFuture = useMemo(() => {
        if (!selectedDate) return false
        const select = toZonedTime(new Date(selectedDate), VIETNAM_TZ)
        const today = toZonedTime(new Date(), VIETNAM_TZ)
        select.setHours(0, 0, 0, 0)
        today.setHours(0, 0, 0, 0)
        return select > today
    }, [selectedDate])

    if (isAnyLoading) {
        return <MainLoader />
    }

    const handlerAutoAssign = async () => {
        try {
            const response: apiResponse = await autoAssign({ courseId: selectedCourseId })
            if (response.data?.isSuccess) {
                toastNotify("Chia lịch thành công!", "success")
                // Refetch dữ liệu sau khi chia lịch thành công nếu cần
            } else {
                const errorMessage = response.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                toastNotify(errorMessage, "error")
            }
        } catch (error) {
            toastNotify("Có lỗi xảy ra khi chia lịch.", "error")
        }
    }

    return (
        <div className="container mt-4">
            <div className="mt-0 mb-4">
                <h3 className="fw-bold primary-color">Quản lý ca trực</h3>
            </div>

            <div className="row mb-4 align-items-end">
                {/* Chọn khóa tu */}
                <div className="col-md-3 mb-3 mb-md-0">
                    <Form.Group controlId="courseSelect">
                        <Form.Label>Khóa tu</Form.Label>
                        <Form.Control
                            as="select"
                            value={selectedCourseId || ""}
                            onChange={(e) => setSelectedCourseId(Number(e.target.value))}
                        >
                            <option value="" disabled>
                                Chọn khóa tu...
                            </option>
                            {courseOptions?.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                </div>

                {/* Chọn ngày */}
                <div className="col-md-3">
                    <Form.Group controlId="dateSelect">
                        <Form.Label className="me-2">Chọn Ngày</Form.Label>
                        <DatePicker
                            selected={selectedDate}
                            onChange={(date: Date | null) => setSelectedDate(date)}
                            minDate={courseData?.result.startDate ? parseISO(courseData?.result.startDate) : undefined}
                            maxDate={courseData?.result.endDate ? parseISO(courseData?.result.endDate) : undefined}
                            dateFormat="dd/MM/yyyy"
                            className="form-control"
                            placeholderText="Chọn ngày..."
                            disabled={!selectedCourseId}
                        />
                    </Form.Group>
                </div>

                {/* Nút chia lịch tự động */}
                {(currentUserRole === SD_Role_Name.SECRETARY || currentUserRole === SD_Role_Name.MANAGER) && (
                    <div className="col-md-6 text-end">
                        <Button variant="primary" onClick={handlerAutoAssign} disabled={isCourseInPast}>
                            Chia nhân viên
                        </Button>
                    </div>
                )}
            </div>

            {/* Hiển thị bảng phân công ca trực */}
            <table className="table table-bordered">
                <thead>
                    <tr>
                        <th style={{ width: "12rem", minWidth: "12rem" }}>Phòng</th>
                        {nightShiftsData?.result.map((shift: nightShiftModel) => (
                            <th key={shift.id}>
                                {shift.startTime.substring(0, 5)} - {shift.endTime.substring(0, 5)}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {gridData.map((rowData, rowIndex) => (
                        <tr key={rowIndex}>
                            <td style={{ width: "12rem", minWidth: "12rem" }} className="room-name ps-3">
                                Phòng: {rowData.roomName}
                                <br />
                                <span className="fw-normal">Giới tính: {rowData.gender}</span>
                            </td>
                            {nightShiftsData?.result.map((shift: nightShiftModel) => (
                                <td key={shift.id} className="shift-cell">
                                    <ShiftCellRenderer
                                        assignments={rowData.shifts[shift.id]}
                                        roomData={rowData}
                                        shiftId={shift.id}
                                        dateString={dateString}
                                        isPastDate={isPastDate}
                                        isFuture={isFuture}
                                        courseId={selectedCourseId!} // Truyền courseId vào
                                    />
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default NightShiftManager
