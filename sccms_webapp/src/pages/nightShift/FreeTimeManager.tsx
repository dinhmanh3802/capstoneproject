import React, { useEffect, useState, useMemo } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useGetCourseByIdQuery } from "../../apis/courseApi"
import { MainLoader } from "../../components/Page"
import { Form, OverlayTrigger, Tooltip } from "react-bootstrap"
import Select, { SingleValue } from "react-select"
import { useGetAllStaffFreeTimesQuery } from "../../apis/staffFreeTimeApi"
import { eachDayOfInterval, format, parseISO, isWithinInterval } from "date-fns"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { useGetAllRoomsQuery } from "../../apis/roomApi"
import { useGetAllNightShiftsQuery } from "../../apis/nightShiftApi"
import { SD_Gender } from "../../utility/SD"

// Updated FreeTimeModel interface with isCancel property
interface FreeTimeModel {
    courseId: number
    date: string
    userId: number
    userName: string
    fullName: string
    gender: SD_Gender
    isCancel: boolean // New property to indicate cancellation
}

interface CourseOption {
    value: number
    label: string
}

interface UserData {
    fullName: string
    gender: SD_Gender
    selectedDates: Map<string, boolean> // Map of date to isCancel
}

function FreeTimeManager() {
    // ----------------------------- Chọn khóa tu -----------------------------
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)

    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(undefined)
    useEffect(() => {
        if (currentCourse?.id) {
            setSelectedCourseId(currentCourse.id)
        }
    }, [currentCourse])

    const {
        data: courseData,
        isLoading: courseLoading,
        error: courseError,
    } = useGetCourseByIdQuery(selectedCourseId || 0, { skip: !selectedCourseId })

    const handleCourseChange = (selectedOption: SingleValue<CourseOption>) => {
        setSelectedCourseId(selectedOption?.value)
    }

    const courseOptions: CourseOption[] = useMemo(
        () =>
            listCourseFromStore.map((course) => ({
                value: course.id,
                label: course.courseName,
            })),
        [listCourseFromStore],
    )
    // ----------------------------- Kết thúc chọn khóa tu -----------------------------

    // Lấy danh sách free time
    const {
        data: freeTime,
        isLoading: freeTimeLoading,
        refetch: refetchFreeTime,
        error: freeTimeError,
    } = useGetAllStaffFreeTimesQuery({ courseId: selectedCourseId }, { skip: !selectedCourseId })

    // State cho bộ lọc khoảng thời gian
    const [startDateFilter, setStartDateFilter] = useState<Date | null>(null)
    const [endDateFilter, setEndDateFilter] = useState<Date | null>(null)

    // State cho bộ lọc tên người dùng
    const [userNameFilter, setUserNameFilter] = useState<string>("")

    // Lấy danh sách phòng và ca trực
    const {
        data: roomsData,
        isLoading: roomsLoading,
        refetch: refetchRooms,
        error: roomsError,
    } = useGetAllRoomsQuery(selectedCourseId || 0, { skip: !selectedCourseId })

    const {
        data: nightShiftsData,
        isLoading: nightShiftsLoading,
        refetch: refetchNightShifts,
        error: nightShiftsError,
    } = useGetAllNightShiftsQuery(selectedCourseId || 0, { skip: !selectedCourseId })

    // Tính số lượng nhân viên cần thiết mỗi ngày theo giới tính
    let numberMaleStaffPerDay = 0
    let numberFemaleStaffPerDay = 0
    if (roomsData && nightShiftsData) {
        roomsData.result.forEach((room) => {
            const staffNeeded = room.numberOfStaff * nightShiftsData.result.length
            if (room.gender === SD_Gender.Male) {
                numberMaleStaffPerDay += staffNeeded
            } else if (room.gender === SD_Gender.Female) {
                numberFemaleStaffPerDay += staffNeeded
            }
        })
    }

    // --------------------------------- Xử lý dữ liệu ---------------------------------
    // Handle loading and error states

    const courseStartDate = new Date(courseData?.result.startDate)
    const courseEndDate = new Date(courseData?.result.endDate)

    const filterStartDate = startDateFilter || courseStartDate
    const filterEndDate = endDateFilter || courseEndDate

    // Tạo danh sách các ngày trong khoảng lọc
    const dateList = eachDayOfInterval({
        start: filterStartDate,
        end: filterEndDate,
    })

    // Định dạng ngày thành 'dd-MM-yyyy'
    const formattedDateList = useMemo(() => dateList.map((date) => format(date, "dd-MM-yyyy")), [dateList])

    // Tổ chức lại dữ liệu người dùng và các ngày họ đã chọn
    const userMap: Map<number, UserData> = useMemo(() => {
        const map = new Map<number, UserData>()
        freeTime?.result.forEach((item: FreeTimeModel) => {
            // Chuyển chuỗi ngày từ database thành Date object
            const dateObj = parseISO(item.date)
            const dateOnlyFormatted = format(dateObj, "dd-MM-yyyy")
            // Kiểm tra xem ngày có nằm trong khoảng lọc không
            if (!isWithinInterval(dateObj, { start: filterStartDate, end: filterEndDate })) {
                return
            }
            if (!map.has(item.userId)) {
                map.set(item.userId, {
                    fullName: item.fullName,
                    gender: item.gender,
                    selectedDates: new Map(),
                })
            }
            // Set the isCancel status for the date
            map.get(item.userId)?.selectedDates.set(dateOnlyFormatted, item.isCancel)
        })
        return map
    }, [freeTime, filterStartDate, filterEndDate])

    // Chuyển Map thành mảng để render
    let userList = useMemo(
        () =>
            Array.from(userMap.entries()).map(([userId, data]) => ({
                userId,
                fullName: data.fullName,
                gender: data.gender,
                selectedDates: data.selectedDates,
            })),
        [userMap],
    )

    // Lọc userList theo userNameFilter
    if (userNameFilter.trim() !== "") {
        const lowerCaseFilter = userNameFilter.trim().toLowerCase()
        userList = userList.filter((user) => user.fullName.toLowerCase().includes(lowerCaseFilter))
    }

    // Tính số lượng nhân viên đã đăng ký cho mỗi ngày theo giới tính
    const staffCountPerDayMale: { [date: string]: number } = {}
    const staffCountPerDayFemale: { [date: string]: number } = {}
    formattedDateList.forEach((date) => {
        staffCountPerDayMale[date] = 0
        staffCountPerDayFemale[date] = 0
    })

    userList.forEach((user) => {
        formattedDateList.forEach((date) => {
            const isCancel = user.selectedDates.get(date)
            if (isCancel === undefined) return
            if (!isCancel) {
                // Only count non-canceled shifts
                if (user.gender === SD_Gender.Male) {
                    staffCountPerDayMale[date] += 1
                } else if (user.gender === SD_Gender.Female) {
                    staffCountPerDayFemale[date] += 1
                }
            }
        })
    })
    if (courseLoading || freeTimeLoading || roomsLoading || nightShiftsLoading) return <MainLoader />

    // --------------------------------- Render ---------------------------------
    return (
        <div className="container mt-4">
            <div className="mt-0 mb-4">
                <h3 className="fw-bold primary-color">Số lượng đăng ký ca trực</h3>
            </div>

            {/* Dropdown chọn khóa tu */}
            <div className="row mb-3">
                <div className="col-md-4">
                    <Form.Group controlId="courseSelect">
                        <Select
                            options={courseOptions}
                            value={courseOptions.find((option) => option.value === selectedCourseId)}
                            onChange={handleCourseChange}
                            isClearable={false}
                            placeholder="Chọn khóa tu..."
                        />
                    </Form.Group>
                </div>
            </div>

            {/* Bộ lọc */}
            <div className="row mb-3">
                <div className="col-md-3">
                    <Form.Group controlId="userNameFilter">
                        <Form.Label>Tên nhân viên</Form.Label>
                        <Form.Control
                            type="text"
                            placeholder="Nhập tên..."
                            value={userNameFilter}
                            onChange={(e) => setUserNameFilter(e.target.value)}
                        />
                    </Form.Group>
                </div>
                <div className="col-md-2">
                    <Form.Group controlId="startDateFilter">
                        <Form.Label>Từ ngày</Form.Label>
                        <DatePicker
                            selected={startDateFilter}
                            onChange={(date: Date | null) => setStartDateFilter(date)}
                            selectsStart
                            startDate={startDateFilter}
                            endDate={endDateFilter}
                            minDate={courseStartDate}
                            maxDate={courseEndDate}
                            dateFormat="dd-MM-yyyy"
                            className="form-control"
                            placeholderText="Chọn ngày..."
                        />
                    </Form.Group>
                </div>
                <div className="col-md-2">
                    <Form.Group controlId="endDateFilter">
                        <Form.Label>Đến ngày</Form.Label>
                        <DatePicker
                            selected={endDateFilter}
                            onChange={(date: Date | null) => setEndDateFilter(date)}
                            selectsEnd
                            startDate={startDateFilter}
                            endDate={endDateFilter}
                            minDate={startDateFilter || courseStartDate}
                            maxDate={courseEndDate}
                            dateFormat="dd-MM-yyyy"
                            className="form-control"
                            placeholderText="Chọn ngày..."
                        />
                    </Form.Group>
                </div>
            </div>

            {/* Hiển thị bảng */}
            <div className="table-responsive">
                <table className="table table-bordered">
                    <thead>
                        <tr>
                            <th style={{ width: "200px", minWidth: "200px" }}>Tên nhân viên</th>
                            <th>Giới tính</th>
                            {formattedDateList.map((date) => (
                                <th key={date}>{date}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {userList.map((user) => (
                            <tr key={user.userId}>
                                <td style={{ width: "200px", minWidth: "200px" }}>{user.fullName}</td>
                                <td style={{ width: "60px", minWidth: "60px" }}>
                                    {user.gender === SD_Gender.Male ? "Nam" : "Nữ"}
                                </td>
                                {formattedDateList.map((date) => {
                                    const isCancel = user.selectedDates.get(date)
                                    return (
                                        <td
                                            key={date}
                                            className="text-center"
                                            style={{
                                                border: "2px solid #dee2e6",
                                                color:
                                                    isCancel === true
                                                        ? "#ffffff"
                                                        : isCancel === false
                                                        ? "#000"
                                                        : "#ffffff", // Light red or green,
                                                backgroundColor:
                                                    isCancel === true
                                                        ? "rgba(0, 0, 0, 0.4)"
                                                        : isCancel === false
                                                        ? "#ffffff"
                                                        : "#ffffff", // Light red or green
                                                height: "40px",
                                                width: "80px",
                                            }}
                                        >
                                            {/* {isCancel ? "✗" : "✓"} */}
                                            {isCancel ? (
                                                <OverlayTrigger
                                                    placement="bottom"
                                                    overlay={<Tooltip id="button-tooltip-2">Hủy đăng ký</Tooltip>}
                                                >
                                                    <span>✗</span>
                                                </OverlayTrigger>
                                            ) : (
                                                <OverlayTrigger
                                                    placement="bottom"
                                                    overlay={<Tooltip id="button-tooltip-2">Đăng ký</Tooltip>}
                                                >
                                                    <span>✓</span>
                                                </OverlayTrigger>
                                            )}
                                        </td>
                                    )
                                })}
                            </tr>
                        ))}
                    </tbody>
                    <tfoot>
                        <tr>
                            <th>Số lượng đăng ký (Nam)</th>
                            <th></th>
                            {formattedDateList.map((date) => {
                                const staffCount = staffCountPerDayMale[date] || 0
                                const isUnderRequired = staffCount < numberMaleStaffPerDay
                                return (
                                    <td
                                        key={date}
                                        className="text-center"
                                        style={{ color: isUnderRequired ? "red" : "green", fontWeight: "bold" }}
                                    >
                                        {staffCount}/{numberMaleStaffPerDay}
                                    </td>
                                )
                            })}
                        </tr>
                        <tr>
                            <th>Số lượng đăng ký (Nữ)</th>
                            <th></th>
                            {formattedDateList.map((date) => {
                                const staffCount = staffCountPerDayFemale[date] || 0
                                const isUnderRequired = staffCount < numberFemaleStaffPerDay
                                return (
                                    <td
                                        key={date}
                                        className="text-center"
                                        style={{ color: isUnderRequired ? "red" : "green", fontWeight: "bold" }}
                                    >
                                        {staffCount}/{numberFemaleStaffPerDay}
                                    </td>
                                )
                            })}
                        </tr>
                    </tfoot>
                </table>
            </div>
            <span className="text-danger">
                * Tỉ lệ = <i>Số lượng đăng ký/ Số lượng tối thiểu cần</i>
            </span>
        </div>
    )
}

export default FreeTimeManager
