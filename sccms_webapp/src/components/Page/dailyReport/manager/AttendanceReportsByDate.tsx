// src/components/Page/dailyReport/manager/AttendanceReportsByDate.tsx

import React, { useEffect, useState, useMemo } from "react"
import { useSelector } from "react-redux"
import { useNavigate, useSearchParams } from "react-router-dom" // Import useSearchParams
import { RootState } from "../../../../store/store"
import { useGetAttendanceReportsByDateQuery } from "../../../../apis/reportApi"
import DataTable, { TableColumn } from "react-data-table-component"
import { parseISO, isWithinInterval, format } from "date-fns"
import { SD_ReportStatus } from "../../../../utility/SD"
import { ReportDto, courseModel } from "../../../../interfaces"
import { Button, Spinner, Card, Form, Badge, Tooltip, OverlayTrigger } from "react-bootstrap"
import Select from "react-select"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { toastNotify } from "../../../../helper"
import { FaEye } from "react-icons/fa"
import { useGetStudentGroupsQuery } from "../../../../apis/studentGroupApi"
import MainLoader from "../../common/MainLoader"
import { useGetCourseByIdQuery } from "../../../../apis/courseApi"

const isValidDate = (date: Date | null): boolean => date !== null && !isNaN(date.getTime())

function AttendanceReportsByDate() {
    const navigate = useNavigate()
    const [searchParams, setSearchParams] = useSearchParams()

    // Lấy danh sách courses từ store
    const listCourseFromStore: courseModel[] = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)

    // Local state để quản lý khóa tu và ngày
    const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null)
    const [selectedDate, setSelectedDate] = useState<Date | null>(null)
    const {
        data: courseData,
        isLoading: courseLoading,
        error: courseError,
    } = useGetCourseByIdQuery(selectedCourseId || 0, { skip: !selectedCourseId })
    // Tìm khóa tu được chọn
    const selectedCourse = useMemo(() => {
        return listCourseFromStore?.find((course) => course.id === selectedCourseId) || null
    }, [listCourseFromStore, selectedCourseId])

    // Tự động chọn khóa tu mặc định khi danh sách khóa tu được tải về
    useEffect(() => {
        // Nếu đã có courseId trong URL, sử dụng nó
        const courseIdFromParams = searchParams.get("courseId")
        const dateFromParams = searchParams.get("date")

        if (courseIdFromParams) {
            const parsedCourseId = parseInt(courseIdFromParams, 10)
            const courseExists = listCourseFromStore.some((course) => course.id === parsedCourseId)
            if (courseExists) {
                setSelectedCourseId(parsedCourseId)
            } else {
                // Nếu courseId trong URL không tồn tại, chọn khóa tu đầu tiên
                setSelectedCourseId(
                    currentCourse?.id || (listCourseFromStore.length > 0 ? listCourseFromStore[0].id : null),
                )
            }
        } else if (listCourseFromStore.length > 0) {
            // Nếu không có courseId trong URL, chọn khóa tu hiện tại hoặc khóa tu đầu tiên
            setSelectedCourseId(currentCourse?.id || listCourseFromStore[0].id)
        }
    }, [listCourseFromStore, currentCourse, searchParams])

    // Cập nhật selectedDate dựa trên khóa tu khi component mount hoặc khi khóa tu thay đổi
    useEffect(() => {
        if (selectedCourse) {
            const courseIdFromParams = searchParams.get("courseId")
            const dateFromParams = searchParams.get("date")

            let initialDate: Date | null = null

            if (dateFromParams) {
                const parsedDate = new Date(dateFromParams)
                if (isValidDate(parsedDate)) {
                    // Kiểm tra nếu ngày từ params nằm trong khoảng thời gian của khóa tu
                    const startDate = selectedCourse.startDate ? new Date(selectedCourse.startDate) : null
                    const endDate = selectedCourse.endDate ? new Date(selectedCourse.endDate) : null

                    if (
                        startDate &&
                        endDate &&
                        parsedDate >= startDate &&
                        parsedDate <= (endDate < new Date() ? endDate : new Date())
                    ) {
                        initialDate = parsedDate
                    }
                }
            }

            if (!initialDate) {
                const today = new Date()
                const startDate = selectedCourse.startDate ? new Date(selectedCourse.startDate) : null
                const endDate = selectedCourse.endDate ? new Date(selectedCourse.endDate) : null

                if (startDate && endDate) {
                    if (isValidDate(today) && isValidDate(startDate) && isValidDate(endDate)) {
                        if (today >= startDate && today <= (endDate < today ? endDate : today)) {
                            initialDate = today
                        } else {
                            initialDate = startDate
                        }
                    }
                }
            }

            setSelectedDate(initialDate)

            // Cập nhật URL với ngày đã chọn
            if (initialDate) {
                setSearchParams({
                    courseId: selectedCourseId ? selectedCourseId.toString() : "",
                    date: format(initialDate, "yyyy-MM-dd"),
                })
            }
        } else {
            setSelectedDate(null)
        }
    }, [selectedCourse, searchParams, selectedCourseId, setSearchParams])

    // Định dạng ngày theo định dạng yêu cầu
    const dateString = selectedDate && isValidDate(selectedDate) ? format(selectedDate, "yyyy-MM-dd") : ""

    // Hook để gọi API với các bộ lọc từ `selectedCourseId` và `dateString`
    const { data, isLoading, error } = useGetAttendanceReportsByDateQuery(
        {
            courseId: selectedCourseId || 0,
            reportDate: dateString || "",
        },
        { skip: !selectedCourseId || !isValidDate(selectedDate) },
    )

    //laasy Group theo userId
    const {
        data: studentGroupsData,
        isLoading: studentGroupsLoading,
        refetch,
    } = useGetStudentGroupsQuery(selectedCourseId || 0, { skip: !selectedCourseId || !isValidDate(selectedDate) })
    var myStudentGroups = studentGroupsData?.result?.filter((group) => {
        return group.supervisors.some((s) => s.id == currentUserId)
    })

    // Mapping trạng thái đến văn bản và màu sắc
    const statusMappings: { [key: number]: { text: string; color: string } } = {
        [SD_ReportStatus.NotYet]: { text: "Chưa nộp", color: "secondary" },
        [SD_ReportStatus.Attending]: { text: "Đang mở", color: "warning" },
        [SD_ReportStatus.Attended]: { text: "Đã nộp", color: "success" },
        [SD_ReportStatus.Late]: { text: "Muộn", color: "danger" },
        [SD_ReportStatus.Reopened]: { text: "Mở lại", color: "info" },
        [SD_ReportStatus.Read]: { text: "Đã xem", color: "dark" },
    }

    // Định nghĩa các cột cho DataTable
    const columns: TableColumn<ReportDto>[] = useMemo(
        () => [
            {
                name: "Chánh",
                selector: (row) => row.studentGroup?.groupName || "N/A",
                sortable: true,
            },
            {
                name: "Trạng thái",
                cell: (row) => {
                    const status = statusMappings[row.status] || {
                        text: "N/A",
                        color: "secondary",
                    }
                    return <Badge bg={status.color}>{status.text}</Badge>
                },
                sortable: true,
            },
            {
                name: "Sĩ số",
                cell: (row) => {
                    const presentCount = row.studentReports.filter((sr) => sr.status === 1).length
                    const totalCount = row.studentReports.length
                    return <Badge bg="info">{`${presentCount}/${totalCount}`}</Badge>
                },
            },
            {
                name: "Thao tác",
                cell: (row) => (
                    <div className="d-flex justify-content-around">
                        {/* Nút "Xem" với Tooltip */}
                        <OverlayTrigger
                            placement="top"
                            overlay={<Tooltip id={`tooltip-view-${row.id}`}>Xem báo cáo</Tooltip>}
                        >
                            <Button
                                variant="outline-primary"
                                size="sm"
                                onClick={() => navigate(`/report/${row.id}`)}
                                title="Xem báo cáo"
                            >
                                <FaEye />
                            </Button>
                        </OverlayTrigger>
                    </div>
                ),
                ignoreRowClick: true,
                allowOverflow: true,
                button: true,
            },
        ],
        [navigate],
    )

    // Tùy chỉnh styles cho DataTable
    const customStyles = {
        headCells: {
            style: {
                fontSize: "16px",
                fontWeight: "bold",
            },
        },
        cells: {
            style: {
                fontSize: "16px",
                color: "#495057",
            },
        },
        rows: {
            style: {
                minHeight: "50px",
            },
            highlightOnHoverStyle: {
                backgroundColor: "#f1f3f5",
                borderBottomColor: "#dee2e6",
                borderRadius: "5px",
            },
        },
        pagination: {
            style: {
                borderTop: "1px solid #dee2e6",
            },
        },
    }

    // Handle lỗi từ API
    useEffect(() => {
        if (error) {
            toastNotify("Đã có lỗi xảy ra khi lấy dữ liệu báo cáo.", "error")
        }
    }, [error])

    // Tính toán ngày tối đa (maxSelectableDate)
    const today = new Date()
    const maxSelectableDate = selectedCourse?.endDate
        ? new Date(selectedCourse.endDate) < today
            ? new Date(selectedCourse.endDate)
            : today
        : today

    // Hàm để cập nhật URL khi chọn khóa tu hoặc ngày
    useEffect(() => {
        if (selectedCourseId && selectedDate && isValidDate(selectedDate)) {
            setSearchParams({
                courseId: selectedCourseId.toString(),
                date: format(selectedDate, "yyyy-MM-dd"),
            })
        }
    }, [selectedCourseId, selectedDate, setSearchParams])

    if (studentGroupsLoading) return <MainLoader />
    return (
        <div className="container mt-4">
            {/* Tiêu đề */}
            <div className="mb-4">
                <h3 className="fw-bold primary-color">
                    {selectedCourseId
                        ? `Báo cáo hằng ngày - Khóa tu ${selectedCourse?.courseName}`
                        : "Báo cáo hằng ngày"}
                </h3>
            </div>

            {/* Bộ lọc: Chọn khóa tu và ngày */}
            <Card className="mb-4">
                <Card.Body>
                    <Form className="row align-items-end">
                        {/* Chọn khóa tu */}
                        <div className="col-md-3 mb-3">
                            <Form.Group controlId="courseSelect">
                                <Form.Label>Khóa tu</Form.Label>
                                <Select
                                    options={listCourseFromStore?.map((course) => ({
                                        value: course.id,
                                        label: course.courseName,
                                    }))}
                                    value={
                                        selectedCourseId
                                            ? {
                                                  value: selectedCourseId,
                                                  label: selectedCourse?.courseName || "",
                                              }
                                            : null
                                    }
                                    onChange={(option) => setSelectedCourseId(option?.value || null)}
                                    placeholder="Chọn khóa tu..."
                                    isClearable={false}
                                />
                            </Form.Group>
                        </div>

                        {/* Chọn ngày */}
                        <div className="col-md-6 mb-3">
                            <Form.Group controlId="dateSelect">
                                <Form.Label className="me-2">Chọn ngày </Form.Label>
                                <DatePicker
                                    selected={selectedDate}
                                    onChange={(date: Date | null) => setSelectedDate(date)}
                                    dateFormat="dd/MM/yyyy"
                                    className="form-control"
                                    placeholderText="Chọn ngày..."
                                    maxDate={maxSelectableDate}
                                    minDate={
                                        courseData?.result.startDate
                                            ? parseISO(courseData?.result.startDate)
                                            : undefined
                                    }
                                    disabled={!selectedCourseId}
                                />
                            </Form.Group>
                        </div>
                    </Form>
                </Card.Body>
            </Card>

            {/* Hiển thị bảng dữ liệu */}
            <Card>
                <Card.Body>
                    {isLoading ? (
                        <div className="d-flex justify-content-center align-items-center" style={{ height: "200px" }}>
                            <Spinner animation="border" variant="primary" />
                            <span className="ms-2">Đang tải...</span>
                        </div>
                    ) : (
                        <DataTable
                            columns={columns}
                            data={data?.result || []}
                            customStyles={customStyles}
                            pagination
                            noDataComponent="Không có dữ liệu"
                            conditionalRowStyles={[
                                {
                                    when: (row) => row.studentGroupId === myStudentGroups[0]?.id,
                                    style: {
                                        fontWeight: "bold", // Đặt kiểu in đậm cho dòng có id === 3
                                        backgroundColor: "#f0f8ff",
                                    },
                                },
                            ]}
                        />
                    )}
                </Card.Body>
            </Card>
        </div>
    )
}

export default AttendanceReportsByDate
