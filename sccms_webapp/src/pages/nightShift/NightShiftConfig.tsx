import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useEffect, useMemo, useState } from "react"
import {
    useCreateRoomMutation,
    useDeleteRoomMutation,
    useGetAllRoomsQuery,
    useUpdateRoomMutation,
} from "../../apis/roomApi"
import {
    useCreateNightShiftMutation,
    useDeleteNightShiftMutation,
    useGetAllNightShiftsQuery,
    useUpdateNightShiftMutation,
} from "../../apis/nightShiftApi"
import Select, { SingleValue } from "react-select"
import { roomModel } from "../../interfaces/roomModel"
import { Button, Card, Form, Modal } from "react-bootstrap"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup.tsx"
import { MainLoader } from "../../components/Page/index.ts"
import toastNotify from "../../helper/toastNotify.ts"
import { nightShiftModel } from "../../interfaces/nightShiftModel.ts"
import { useForm, Controller } from "react-hook-form"
import DataTable, { TableColumn } from "react-data-table-component"
import apiResponse from "../../interfaces/apiResponse.ts"
import { useGetCourseByIdQuery, useUpdateCourseMutation } from "../../apis/courseApi"
import { SD_CourseStatus, SD_Gender } from "../../utility/SD.ts"
import { toZonedTime, format } from "date-fns-tz"
import { parseISO } from "date-fns"

const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

// Import react-datepicker và CSS của nó
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { useGetStudentGroupsQuery } from "../../apis/studentGroupApi.ts"

type RoomFormValues = {
    name: string
    gender: number
    numberOfStaff: number
    studentGroups: any[]
}

type NightShiftFormValues = {
    startTime: string
    endTime: string
    note: string
}

type CourseDatesFormValues = {
    freeTimeApplicationStartDate: Date | null
    freeTimeApplicationEndDate: Date | null
    dateRange: [Date | null, Date | null] | null
}

function NightShiftConfig() {
    // Lấy danh sách courses từ store
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)

    // State cho chọn khóa tu
    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(undefined)
    useEffect(() => {
        if (currentCourse?.id) {
            setSelectedCourseId(currentCourse.id)
        }
    }, [currentCourse])

    // Gọi API để lấy thông tin khóa tu theo selectedCourseId
    const {
        data: courseData,
        isLoading: courseLoading,
        error: courseError,
    } = useGetCourseByIdQuery(selectedCourseId || 0, { skip: !selectedCourseId })
    const isCourseClosed = courseData?.result.status === SD_CourseStatus.closed

    // Gọi API để lấy danh sách phòng và ca trực theo courseId
    const {
        data: roomsData,
        isLoading: roomsLoading,
        error: roomsError,
        refetch: refetchRooms,
    } = useGetAllRoomsQuery(selectedCourseId || 0, { skip: !selectedCourseId })

    const {
        data: nightShiftsData,
        isLoading: nightShiftsLoading,
        error: nightShiftsError,
        refetch: refetchNightShifts,
    } = useGetAllNightShiftsQuery(selectedCourseId || 0, { skip: !selectedCourseId })
    const {
        data: studentGroupData,
        isLoading: studentGroupLoading,
        error: studentGroupError,
        refetch: refetchStudentGroups,
    } = useGetStudentGroupsQuery(selectedCourseId || 0, { skip: !selectedCourseId })

    // Mutations cho phòng
    const [createRoom] = useCreateRoomMutation()
    const [updateRoom] = useUpdateRoomMutation()
    const [deleteRoom] = useDeleteRoomMutation()

    // Mutations cho ca trực
    const [createNightShift] = useCreateNightShiftMutation()
    const [updateNightShift] = useUpdateNightShiftMutation()
    const [deleteNightShift] = useDeleteNightShiftMutation()

    // Mutation để cập nhật ngày ứng tuyển thời gian miễn phí
    const [updateCourseDates, { isLoading: isUpdatingDates }] = useUpdateCourseMutation()

    // State cho modals phòng
    const [showCreateRoomModal, setShowCreateRoomModal] = useState(false)
    const [showEditRoomModal, setShowEditRoomModal] = useState(false)
    const [currentRoom, setCurrentRoom] = useState<roomModel | null>(null)

    // State cho modals ca trực
    const [showCreateNightShiftModal, setShowCreateNightShiftModal] = useState(false)
    const [showEditNightShiftModal, setShowEditNightShiftModal] = useState(false)
    const [currentNightShift, setCurrentNightShift] = useState<nightShiftModel | null>(null)

    // State cho popup xác nhận xóa phòng
    const [showDeleteRoomPopup, setShowDeleteRoomPopup] = useState(false)
    const [roomToDelete, setRoomToDelete] = useState<{ id: number; name: string } | null>(null)

    // State cho popup xác nhận xóa ca trực
    const [showDeleteNightShiftPopup, setShowDeleteNightShiftPopup] = useState(false)
    const [nightShiftToDelete, setNightShiftToDelete] = useState<{ id: number; name: string } | null>(null)

    const studentGroupOptions = useMemo(
        () =>
            studentGroupData?.result?.map((group) => ({
                value: group.id,
                label: group.groupName,
                gender: group.gender,
            })) || [],
        [studentGroupData],
    )

    // Handler chọn khóa tu
    const handleCourseChange = (selectedOption: SingleValue<any>) => {
        setSelectedCourseId(selectedOption?.value)
    }

    // Hook form cho tạo phòng mới
    const {
        control: createRoomControl,
        register: registerCreateRoom,
        handleSubmit: handleSubmitCreateRoom,
        formState: { errors: errorsCreateRoom },
        reset: resetCreateRoomForm,
        setValue: setCreateRoomValue,
        watch: watchCreateRoom,
    } = useForm<RoomFormValues>()

    // Hook form cho sửa phòng
    const {
        control: editRoomControl,
        register: registerEditRoom,
        handleSubmit: handleSubmitEditRoom,
        formState: { errors: errorsEditRoom },
        reset: resetEditRoomForm,
        setValue: setEditRoomValue,
        watch,
    } = useForm<RoomFormValues>()
    //-----------------------------------xử lý giới tính chánh khi add vào room--------------------------------------------------
    const gender = watchCreateRoom("gender")
    const filteredStudentGroupOptions = useMemo(
        () => studentGroupOptions?.filter((group) => group.gender == Number(gender)), // So sánh giới tính
        [studentGroupOptions, gender],
    )
    // Xóa giá trị studentGroups khi gender thay đổi
    useEffect(() => {
        setCreateRoomValue("studentGroups", []) // Clear giá trị khi gender thay đổi
    }, [gender, setCreateRoomValue])

    const genderEdit = watch("gender")
    const filteredStudentGroupOptionsEdit = useMemo(
        () => studentGroupOptions?.filter((group) => group.gender == Number(genderEdit)), // So sánh giới tính
        [studentGroupOptions, genderEdit],
    )
    // Xóa giá trị studentGroups khi gender thay đổi
    useEffect(() => {
        setEditRoomValue("studentGroups", []) // Clear giá trị khi gender thay đổi
    }, [genderEdit, setEditRoomValue])
    //-------------------------------------------------------------------------------------
    // Hook form cho tạo ca trực mới
    const {
        register: registerCreateNightShift,
        handleSubmit: handleSubmitCreateNightShift,
        formState: { errors: errorsCreateNightShift },
        reset: resetCreateNightShiftForm,
        getValues: getValuesCreateNightShift,
    } = useForm<NightShiftFormValues>()

    // Hook form cho sửa ca trực
    const {
        register: registerEditNightShift,
        handleSubmit: handleSubmitEditNightShift,
        formState: { errors: errorsEditNightShift },
        reset: resetEditNightShiftForm,
        getValues: getValuesEditNightShift,
    } = useForm<NightShiftFormValues>()

    // Hook form cho cập nhật ngày ứng tuyển thời gian miễn phí sử dụng Controller
    const {
        control,
        handleSubmit: handleSubmitCourseDates,
        formState: { errors: errorsCourseDates },
        reset: resetCourseDatesForm,
        setValue,
    } = useForm<CourseDatesFormValues>({
        defaultValues: {
            freeTimeApplicationStartDate: courseData?.result.freeTimeApplicationStartDate
                ? new Date(courseData.result.freeTimeApplicationStartDate)
                : null,
            freeTimeApplicationEndDate: courseData?.result.freeTimeApplicationEndDate
                ? new Date(courseData.result.freeTimeApplicationEndDate)
                : null,
            dateRange:
                courseData?.result.freeTimeApplicationStartDate && courseData?.result.freeTimeApplicationEndDate
                    ? [
                          new Date(courseData.result.freeTimeApplicationStartDate),
                          new Date(courseData.result.freeTimeApplicationEndDate),
                      ]
                    : null,
        },
    })

    const [isEditDate, setIsEditDate] = useState(false)

    useEffect(() => {
        resetCourseDatesForm({
            freeTimeApplicationStartDate: courseData?.result.freeTimeApplicationStartDate
                ? new Date(courseData.result.freeTimeApplicationStartDate)
                : null,
            freeTimeApplicationEndDate: courseData?.result.freeTimeApplicationEndDate
                ? new Date(courseData.result.freeTimeApplicationEndDate)
                : null,
            dateRange:
                courseData?.result.freeTimeApplicationStartDate && courseData?.result.freeTimeApplicationEndDate
                    ? [
                          new Date(courseData.result.freeTimeApplicationStartDate),
                          new Date(courseData.result.freeTimeApplicationEndDate),
                      ]
                    : null,
        })
    }, [courseData, resetCourseDatesForm])

    // Handler mở modal tạo phòng
    const handleOpenCreateRoomModal = () => {
        resetCreateRoomForm({
            name: "",
            gender: 1,
            numberOfStaff: 1,
            studentGroups: [],
        })
        setShowCreateRoomModal(true)
    }

    // Handler tạo phòng mới
    const onCreateRoom = async (data: RoomFormValues) => {
        const newRoom = {
            courseId: selectedCourseId || 0,
            name: data.name.trim(),
            gender: Number(data.gender),
            numberOfStaff: data.numberOfStaff,
            studentGroupId: data.studentGroups?.map((group) => group.value),
        }
        try {
            const response: apiResponse = await createRoom(newRoom).unwrap()
            toastNotify("Tạo phòng thành công!", "success")
            setShowCreateRoomModal(false)
            resetCreateRoomForm()
            refetchRooms()
        } catch (error: any) {
            const errorMessages = error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra khi tạo phòng."
            toastNotify(errorMessages, "error")
        }
    }

    // Handler mở modal sửa phòng
    const handleOpenEditRoomModal = (room: roomModel) => {
        setCurrentRoom(room)

        // Lấy danh sách studentGroups hiện tại của room
        const selectedGroups = room.studentGroups
            ?.map((groupId) => {
                const matchingGroup = studentGroupOptions?.find((option) => option.value === groupId.id)
                return matchingGroup || null
            })
            ?.filter(Boolean)

        resetEditRoomForm({
            name: room.name,
            gender: Number(room.gender),
            numberOfStaff: room.numberOfStaff,
            studentGroups: selectedGroups,
        })
        setShowEditRoomModal(true)
    }

    useEffect(() => {
        if (currentRoom && showEditRoomModal) {
            const selectedGroups = currentRoom.studentGroups
                ?.map((groupId) => {
                    const matchingGroup = studentGroupOptions?.find((option) => option.value === groupId.id)
                    return matchingGroup || null
                })
                ?.filter(Boolean)

            setEditRoomValue("studentGroups", selectedGroups) // Cập nhật giá trị studentGroups
        }
    }, [currentRoom, showEditRoomModal, studentGroupOptions, setEditRoomValue])

    // Handler sửa phòng
    const onEditRoom = async (data: RoomFormValues) => {
        if (!currentRoom) return

        const editedRoom = {
            courseId: selectedCourseId || 0,
            name: data.name.trim(),
            gender: data.gender,
            numberOfStaff: data.numberOfStaff,
            studentGroupId: data.studentGroups?.map((group) => group.value),
        }

        try {
            await updateRoom({ id: currentRoom.id, roomDto: editedRoom }).unwrap()
            toastNotify("Cập nhật phòng thành công!", "success")
            setShowEditRoomModal(false)
            setCurrentRoom(null)
            resetEditRoomForm()
            refetchRooms()
        } catch (error: any) {
            const errorMsg = error?.data?.result || "Đã xảy ra lỗi khi cập nhật phòng."
            toastNotify(errorMsg, "error")
        }
    }

    // Handler mở popup xóa phòng
    const handleDeleteRoomClick = (room: roomModel) => {
        setRoomToDelete({ id: room.id, name: room.name })
        setShowDeleteRoomPopup(true)
    }

    // Handler xác nhận xóa phòng
    const handleConfirmDeleteRoom = async () => {
        if (!roomToDelete) return

        try {
            await deleteRoom(roomToDelete.id).unwrap()
            toastNotify("Xóa phòng thành công!", "success")
            setShowDeleteRoomPopup(false)
            setRoomToDelete(null)
            refetchRooms()
        } catch (error: any) {
            const errorMsg = error?.data?.result || "Đã xảy ra lỗi khi xóa phòng."
            toastNotify(errorMsg, "error")
        }
    }

    // Handler mở modal tạo ca trực
    const handleOpenCreateNightShiftModal = () => {
        resetCreateNightShiftForm({
            startTime: "00:00",
            endTime: "00:00",
            note: "",
        })
        setShowCreateNightShiftModal(true)
    }

    // Handler tạo ca trực mới
    const onCreateNightShift = async (data: NightShiftFormValues) => {
        const newNightShift = {
            courseId: selectedCourseId || 0,
            startTime: data.startTime + ":00", // Convert to "HH:mm:ss"
            endTime: data.endTime + ":00",
            note: data.note.trim(),
        }

        try {
            const response: apiResponse = await createNightShift(newNightShift).unwrap()
            toastNotify("Tạo ca trực thành công!", "success")
            setShowCreateNightShiftModal(false)
            resetCreateNightShiftForm()
            refetchNightShifts()
        } catch (error: any) {
            const errorMessages = error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra khi tạo ca trực."
            toastNotify(errorMessages, "error")
        }
    }

    // Handler sửa ca trực
    const onEditNightShift = async (data: NightShiftFormValues) => {
        if (!currentNightShift) return

        const editedNightShift = {
            courseId: selectedCourseId || 0,
            startTime: data.startTime + ":00",
            endTime: data.endTime + ":00",
            note: data.note?.trim(),
        }

        try {
            const response: apiResponse = await updateNightShift({
                id: currentNightShift.id,
                nightShiftDto: editedNightShift,
            }).unwrap()
            toastNotify("Cập nhật ca trực thành công!", "success")
            setShowEditNightShiftModal(false)
            setCurrentNightShift(null)
            resetEditNightShiftForm()
            refetchNightShifts()
        } catch (error: any) {
            const errorMessages = error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra khi cập nhật ca trực."
            toastNotify(errorMessages, "error")
        }
    }

    // Handler mở modal sửa ca trực
    const handleOpenEditNightShiftModal = (nightShift: nightShiftModel) => {
        setCurrentNightShift(nightShift)
        resetEditNightShiftForm({
            startTime: nightShift.startTime.substring(0, 5), // "HH:mm"
            endTime: nightShift.endTime.substring(0, 5),
            note: nightShift.note,
        })
        setShowEditNightShiftModal(true)
    }

    // Handler mở popup xóa ca trực
    const handleDeleteNightShiftClick = (nightShift: nightShiftModel) => {
        setNightShiftToDelete({ id: nightShift.id, name: `${nightShift.startTime} - ${nightShift.endTime}` })
        setShowDeleteNightShiftPopup(true)
    }

    // Handler xác nhận xóa ca trực
    const handleConfirmDeleteNightShift = async () => {
        if (!nightShiftToDelete) return

        try {
            await deleteNightShift(nightShiftToDelete.id).unwrap()
            toastNotify("Xóa ca trực thành công!", "success")
            setShowDeleteNightShiftPopup(false)
            setNightShiftToDelete(null)
            refetchNightShifts()
        } catch (error: any) {
            const errorMsg = error?.data?.result || "Đã xảy ra lỗi khi xóa ca trực."
            toastNotify(errorMsg, "error")
        }
    }

    // Định nghĩa cột cho bảng Phòng
    const roomColumns: TableColumn<roomModel>[] = [
        {
            name: "Số thứ tự",
            selector: (row, index) => index + 1,
            sortable: true,
            width: "10rem",
        },
        {
            name: "Tên Phòng",
            selector: (row) => row.name,
            sortable: true,
        },
        {
            name: "Giới Tính",
            selector: (row) => (row.gender === SD_Gender.Male ? "Nam" : "Nữ"),
            sortable: true,
        },
        {
            name: "Số người trực",
            selector: (row) => row.numberOfStaff,
            sortable: true,
        },
        {
            name: "Chánh",
            selector: (row) =>
                row.studentGroups && row.studentGroups.length > 0
                    ? row.studentGroups?.map((group) => group.groupName).join(", ")
                    : "Chưa có",
            sortable: false, // Không cần sắp xếp theo tên nhóm
            wrap: true, // Bọc nội dung nếu danh sách quá dài
        },
        {
            name: "Thao Tác",
            cell: (row) => (
                <>
                    <Button
                        variant="warning"
                        size="sm"
                        className="me-2"
                        onClick={() => handleOpenEditRoomModal(row)}
                        disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                    >
                        <i className="bi bi-pencil"></i>
                    </Button>
                    <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDeleteRoomClick(row)}
                        disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                    >
                        <i className="bi bi-trash"></i>
                    </Button>
                </>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
        },
    ]

    // Định nghĩa cột cho bảng Ca Trực
    const nightShiftColumns: TableColumn<nightShiftModel>[] = [
        {
            name: "Số thứ tự",
            selector: (row, index) => index + 1,
            sortable: true,
            width: "10rem",
        },
        {
            name: "Thời Gian Bắt Đầu",
            selector: (row) => row.startTime,
            sortable: true,
        },
        {
            name: "Thời Gian Kết Thúc",
            selector: (row) => row.endTime,
            sortable: true,
        },
        {
            name: "Thao Tác",
            cell: (row) => (
                <>
                    <Button
                        variant="warning"
                        size="sm"
                        className="me-2"
                        onClick={() => handleOpenEditNightShiftModal(row)}
                        disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                    >
                        <i className="bi bi-pencil"></i>
                    </Button>
                    <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDeleteNightShiftClick(row)}
                        disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                    >
                        <i className="bi bi-trash"></i>
                    </Button>
                </>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
        },
    ]

    // Custom Styles for DataTable
    const customStyles = {
        headCells: {
            style: {
                fontWeight: "bold",
                fontSize: "15px",
            },
        },
        cells: {
            style: {
                fontSize: "14px",
            },
        },
    }

    const courseOptions: any = listCourseFromStore?.map((course) => ({
        value: course.id,
        label: course.courseName,
    }))
    if (courseLoading) return <MainLoader />

    const onSaveCourseDates = async (data: CourseDatesFormValues) => {
        // Kiểm tra xem có ít nhất một trường được cập nhật
        if (!data.freeTimeApplicationStartDate && !data.freeTimeApplicationEndDate) {
            toastNotify("Vui lòng chọn ít nhất một khoảng thời gian để cập nhật.", "error")
            return
        }

        try {
            const payload: any = {}

            if (data.freeTimeApplicationStartDate !== null) {
                // Chuyển đổi sang múi giờ Việt Nam
                const zonedStartDate = toZonedTime(data.freeTimeApplicationStartDate, VIETNAM_TZ)
                const formattedStartDate = format(zonedStartDate, "yyyy-MM-dd'T'HH:mm:ss", { timeZone: VIETNAM_TZ })
                payload.freeTimeApplicationStartDate = formattedStartDate
            }

            if (data.freeTimeApplicationEndDate !== null) {
                // Chuyển đổi sang múi giờ Việt Nam
                const zonedEndDate = toZonedTime(data.freeTimeApplicationEndDate, VIETNAM_TZ)
                const formattedEndDate = format(zonedEndDate, "yyyy-MM-dd'T'HH:mm:ss", { timeZone: VIETNAM_TZ })
                payload.freeTimeApplicationEndDate = formattedEndDate
            }
            if (payload.freeTimeApplicationEndDate < payload.freeTimeApplicationStartDate) {
                toastNotify("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc", "error")
                return
            }
            await updateCourseDates({ id: selectedCourseId, body: payload }).unwrap()
            toastNotify("Cập nhật ngày thành công!", "success")
            setIsEditDate(false)
            refetchRooms()
            refetchNightShifts()
        } catch (error: any) {
            const errorMsg = error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra khi cập nhật ngày."
            toastNotify(errorMsg, "error")
        }
    }

    return (
        <div className="container mt-4">
            <div className="mt-0 mb-4">
                <h3 className="fw-bold primary-color">Cài đặt ca trực</h3>
            </div>

            {/* Dropdown chọn khóa tu */}
            <div className="col-4">
                <div className="mb-3">
                    <Form.Group controlId="courseSelect">
                        <Select
                            options={courseOptions}
                            value={courseOptions?.find((option) => option.value === selectedCourseId)}
                            onChange={handleCourseChange}
                            isClearable={false}
                        />
                    </Form.Group>
                </div>
            </div>
            {/* Form cập nhật ngày ứng tuyển thời gian miễn phí với Date Range Picker */}
            <Card className="mb-4">
                <Card.Header>
                    <h5>Thời gian đăng ký ca trực</h5>
                </Card.Header>
                <Card.Body>
                    <Form onSubmit={handleSubmitCourseDates(onSaveCourseDates)}>
                        <Form.Group controlId="freeTimeApplicationDateRange" className="mb-3">
                            <div className="d-inline me-2">Thời gian mở đơn</div>
                            <Controller
                                control={control}
                                name="dateRange"
                                render={({ field }) => (
                                    // @ts-ignore
                                    <DatePicker
                                        {...field}
                                        selected={field.value ? field.value[0] : null}
                                        onChange={(dates: [Date | null, Date | null] | null) => {
                                            field.onChange(dates)
                                            if (dates) {
                                                const [start, end] = dates
                                                setValue("freeTimeApplicationStartDate", start)
                                                setValue("freeTimeApplicationEndDate", end)
                                            } else {
                                                setValue("freeTimeApplicationStartDate", null)
                                                setValue("freeTimeApplicationEndDate", null)
                                            }
                                        }}
                                        selectsRange
                                        startDate={field.value ? field.value[0] : null}
                                        endDate={field.value ? field.value[1] : null}
                                        dateFormat="dd/MM/yyyy"
                                        className="form-control"
                                        placeholderText="Chọn khoảng thời gian..."
                                        minDate={new Date()}
                                        maxDate={
                                            courseData?.result.endDate
                                                ? parseISO(courseData?.result.endDate)
                                                : undefined
                                        }
                                        disabled={isCourseClosed || !isEditDate}
                                    />
                                )}
                            />
                            {errorsCourseDates.freeTimeApplicationStartDate && (
                                <small className="text-danger">
                                    {errorsCourseDates.freeTimeApplicationStartDate.message}
                                </small>
                            )}
                            {errorsCourseDates.freeTimeApplicationEndDate && (
                                <small className="text-danger">
                                    {errorsCourseDates.freeTimeApplicationEndDate.message}
                                </small>
                            )}
                        </Form.Group>
                        <div className="d-flex justify-content-end mb-3">
                            {isEditDate ? (
                                <button
                                    className="btn btn-primary btn-sm"
                                    type="submit"
                                    disabled={isCourseClosed || isUpdatingDates}
                                >
                                    {isUpdatingDates ? "Đang cập nhật..." : "Lưu Thay Đổi"}
                                </button>
                            ) : (
                                <a
                                    className={`btn btn-primary btn-sm ${
                                        isCourseClosed || isUpdatingDates ? "disabled" : ""
                                    }`}
                                    role="button"
                                    onClick={() => !isCourseClosed && !isUpdatingDates && setIsEditDate(true)}
                                >
                                    Chỉnh sửa
                                </a>
                            )}
                        </div>
                    </Form>
                </Card.Body>
            </Card>

            {/* Bảng Phòng */}
            <Card className="mb-5">
                <Card.Header>
                    <h5>Danh Sách Phòng</h5>
                </Card.Header>
                <Card.Body>
                    <div className="d-flex justify-content-end mb-3">
                        <button
                            className="btn btn-primary btn-sm"
                            onClick={handleOpenCreateRoomModal}
                            disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                        >
                            <i className="bi bi-plus-lg me-2"></i>Thêm mới
                        </button>
                    </div>
                    <DataTable
                        columns={roomColumns}
                        data={roomsData?.result || []}
                        progressPending={roomsLoading}
                        pagination
                        highlightOnHover
                        pointerOnHover
                        customStyles={customStyles}
                        noDataComponent="Không có phòng nào."
                    />
                    {roomsError && (
                        <div className="text-center text-danger mt-2">Đã xảy ra lỗi khi tải dữ liệu phòng.</div>
                    )}
                </Card.Body>
            </Card>

            {/* Bảng Ca Trực */}
            <Card className="mb-5">
                <Card.Header>
                    <h5>Danh Sách Ca Trực</h5>
                </Card.Header>
                <Card.Body>
                    <div className="d-flex justify-content-end mb-3">
                        <button
                            className="btn btn-primary btn-sm"
                            onClick={handleOpenCreateNightShiftModal}
                            disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                        >
                            <i className="bi bi-plus-lg me-2"></i>Thêm Mới
                        </button>
                    </div>
                    <DataTable
                        columns={nightShiftColumns}
                        data={nightShiftsData?.result || []}
                        progressPending={nightShiftsLoading}
                        pagination
                        highlightOnHover
                        pointerOnHover
                        customStyles={customStyles}
                        noDataComponent="Không có ca trực nào."
                    />
                    {nightShiftsError && (
                        <div className="text-center text-danger mt-2">Đã xảy ra lỗi khi tải dữ liệu ca trực.</div>
                    )}
                </Card.Body>
            </Card>

            {/* Modal add room */}
            <Modal show={showCreateRoomModal} onHide={() => setShowCreateRoomModal(false)} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Thêm Phòng Mới</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleSubmitCreateRoom(onCreateRoom)}>
                        <Form.Group controlId="createRoomName" className="mb-3">
                            <Form.Label>Tên Phòng</Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="Nhập tên phòng"
                                {...registerCreateRoom("name", {
                                    required: "Tên phòng không được để trống.",
                                    validate: (value) => value.trim() !== "" || "Tên phòng không được để trống.",
                                })}
                            />
                            {errorsCreateRoom.name && (
                                <small className="text-danger">{errorsCreateRoom.name.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="createRoomGender" className="mb-3">
                            <Form.Label>Giới Tính</Form.Label>
                            <Form.Select {...registerCreateRoom("gender", { required: "Giới tính là bắt buộc." })}>
                                <option value={SD_Gender.Male}>Nam</option>
                                <option value={SD_Gender.Female}>Nữ</option>
                            </Form.Select>
                            {errorsCreateRoom.gender && (
                                <small className="text-danger">{errorsCreateRoom.gender.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="createRoomStudentGroups" className="mb-3">
                            <Form.Label>Chánh</Form.Label>
                            <Controller
                                name="studentGroups"
                                control={createRoomControl}
                                defaultValue={[]}
                                render={({ field }) => (
                                    <Select
                                        {...field}
                                        isMulti
                                        options={filteredStudentGroupOptions} // Sử dụng danh sách đã lọc
                                        onChange={(selected) => field.onChange(selected || [])}
                                        isLoading={studentGroupLoading}
                                        placeholder="Chọn chánh..."
                                    />
                                )}
                            />
                            {errorsCreateRoom.studentGroups && (
                                <small className="text-danger">{errorsCreateRoom.studentGroups.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="createRoomNumberOfStaff" className="mb-3">
                            <Form.Label>Số Lượng Nhân Viên</Form.Label>
                            <Form.Control
                                type="number"
                                min={1}
                                max={10}
                                placeholder="Nhập số lượng nhân viên"
                                {...registerCreateRoom("numberOfStaff", {
                                    required: "Số lượng nhân viên là bắt buộc.",
                                    valueAsNumber: true,
                                    min: { value: 1, message: "Số lượng nhân viên phải ít nhất là 1." },
                                    max: { value: 10, message: "Số lượng nhân viên không được vượt quá 10." },
                                })}
                            />
                            {errorsCreateRoom.numberOfStaff && (
                                <small className="text-danger">{errorsCreateRoom.numberOfStaff.message}</small>
                            )}
                        </Form.Group>
                        <Modal.Footer>
                            <Button variant="secondary" onClick={() => setShowCreateRoomModal(false)}>
                                Hủy
                            </Button>
                            <Button variant="primary" type="submit">
                                Tạo Phòng
                            </Button>
                        </Modal.Footer>
                    </Form>
                </Modal.Body>
            </Modal>

            {/* Modal Sửa Phòng */}
            <Modal show={showEditRoomModal} onHide={() => setShowEditRoomModal(false)} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Sửa Thông Tin Phòng</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleSubmitEditRoom(onEditRoom)}>
                        <Form.Group controlId="editRoomName" className="mb-3">
                            <Form.Label>Tên Phòng</Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="Nhập tên phòng"
                                {...registerEditRoom("name", {
                                    required: "Tên phòng không được để trống.",
                                    validate: (value) => value.trim() !== "" || "Tên phòng không được để trống.",
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsEditRoom.name && (
                                <small className="text-danger">{errorsEditRoom.name.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="editRoomGender" className="mb-3">
                            <Form.Label>Giới Tính</Form.Label>
                            <Form.Select
                                {...registerEditRoom("gender", { required: "Giới tính là bắt buộc." })}
                                disabled={true} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                <option value={SD_Gender.Male}>Nam</option>
                                <option value={SD_Gender.Female}>Nữ</option>
                            </Form.Select>
                            {errorsEditRoom.gender && (
                                <small className="text-danger">{errorsEditRoom.gender.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="editRoomStudentGroups" className="mb-3">
                            <Form.Label>Chánh</Form.Label>
                            <Controller
                                name="studentGroups"
                                control={editRoomControl}
                                defaultValue={[]}
                                render={({ field }) => (
                                    <Select
                                        {...field}
                                        isMulti
                                        options={filteredStudentGroupOptionsEdit} // Sử dụng danh sách đã lọc
                                        onChange={(selected) => field.onChange(selected || [])}
                                        isLoading={studentGroupLoading}
                                        placeholder="Chọn chánh..."
                                        isDisabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                                    />
                                )}
                            />
                            {errorsEditRoom.studentGroups && (
                                <small className="text-danger">{errorsEditRoom.studentGroups.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="editRoomNumberOfStaff" className="mb-3">
                            <Form.Label>Số Lượng Nhân Viên</Form.Label>
                            <Form.Control
                                type="number"
                                min={1}
                                max={10}
                                placeholder="Nhập số lượng nhân viên"
                                {...registerEditRoom("numberOfStaff", {
                                    required: "Số lượng nhân viên là bắt buộc.",
                                    valueAsNumber: true,
                                    min: { value: 1, message: "Số lượng nhân viên phải ít nhất là 1." },
                                    max: { value: 10, message: "Số lượng nhân viên không được vượt quá 10." },
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsEditRoom.numberOfStaff && (
                                <small className="text-danger">{errorsEditRoom.numberOfStaff.message}</small>
                            )}
                        </Form.Group>
                        <Modal.Footer>
                            <Button
                                variant="secondary"
                                onClick={() => setShowEditRoomModal(false)}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Hủy
                            </Button>
                            <Button
                                variant="primary"
                                type="submit"
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Lưu Thay Đổi
                            </Button>
                        </Modal.Footer>
                    </Form>
                </Modal.Body>
            </Modal>

            {/* Popup Xác Nhận Xóa Phòng */}
            <ConfirmationPopup
                isOpen={showDeleteRoomPopup}
                onClose={() => setShowDeleteRoomPopup(false)}
                onConfirm={handleConfirmDeleteRoom}
                message={`Bạn có chắc chắn muốn xóa phòng <strong>${roomToDelete?.name}</strong> không?`}
                title="Xác Nhận Xóa Phòng"
            />

            {/* Modal Tạo Ca Trực Mới */}
            <Modal show={showCreateNightShiftModal} onHide={() => setShowCreateNightShiftModal(false)} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Thêm Ca Trực Mới</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleSubmitCreateNightShift(onCreateNightShift)}>
                        <Form.Group controlId="createNightShiftStartTime" className="mb-3">
                            <Form.Label>Thời Gian Bắt Đầu</Form.Label>
                            <Form.Control
                                type="time"
                                {...registerCreateNightShift("startTime", {
                                    required: "Thời gian bắt đầu là bắt buộc.",
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsCreateNightShift.startTime && (
                                <small className="text-danger">{errorsCreateNightShift.startTime.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="createNightShiftEndTime" className="mb-3">
                            <Form.Label>Thời Gian Kết Thúc</Form.Label>
                            <Form.Control
                                type="time"
                                {...registerCreateNightShift("endTime", {
                                    required: "Thời gian kết thúc là bắt buộc.",
                                    validate: (value) => {
                                        const startTime = getValuesCreateNightShift("startTime")
                                        if (startTime && value === startTime) {
                                            return "Thời gian bắt đầu phải khác thời gian kết thúc."
                                        }
                                        return true
                                    },
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsCreateNightShift.endTime && (
                                <small className="text-danger">{errorsCreateNightShift.endTime.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="createNightShiftNote" className="mb-3">
                            <Form.Label>Ghi Chú</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={3}
                                placeholder="Nhập ghi chú"
                                {...registerCreateNightShift("note")}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                        </Form.Group>
                        <Modal.Footer>
                            <Button
                                variant="secondary"
                                onClick={() => setShowCreateNightShiftModal(false)}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Hủy
                            </Button>
                            <Button
                                variant="primary"
                                type="submit"
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Tạo Ca Trực
                            </Button>
                        </Modal.Footer>
                    </Form>
                </Modal.Body>
            </Modal>

            {/* Modal Sửa Ca Trực */}
            <Modal show={showEditNightShiftModal} onHide={() => setShowEditNightShiftModal(false)} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Sửa Ca Trực</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleSubmitEditNightShift(onEditNightShift)}>
                        <Form.Group controlId="editNightShiftStartTime" className="mb-3">
                            <Form.Label>Thời Gian Bắt Đầu</Form.Label>
                            <Form.Control
                                type="time"
                                {...registerEditNightShift("startTime", {
                                    required: "Thời gian bắt đầu là bắt buộc.",
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsEditNightShift.startTime && (
                                <small className="text-danger">{errorsEditNightShift.startTime.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="editNightShiftEndTime" className="mb-3">
                            <Form.Label>Thời Gian Kết Thúc</Form.Label>
                            <Form.Control
                                type="time"
                                {...registerEditNightShift("endTime", {
                                    required: "Thời gian kết thúc là bắt buộc.",
                                    validate: (value) => {
                                        const startTime = getValuesEditNightShift("startTime")
                                        if (startTime && value === startTime) {
                                            return "Thời gian kết thúc phải khác thời gian bắt đầu."
                                        }
                                        return true
                                    },
                                })}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                            {errorsEditNightShift.endTime && (
                                <small className="text-danger">{errorsEditNightShift.endTime.message}</small>
                            )}
                        </Form.Group>
                        <Form.Group controlId="editNightShiftNote" className="mb-3">
                            <Form.Label>Ghi Chú</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={3}
                                placeholder="Nhập ghi chú"
                                {...registerEditNightShift("note")}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            />
                        </Form.Group>
                        <Modal.Footer>
                            <Button
                                variant="secondary"
                                onClick={() => setShowEditNightShiftModal(false)}
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Hủy
                            </Button>
                            <Button
                                variant="primary"
                                type="submit"
                                disabled={isCourseClosed} // Vô hiệu hóa nếu khóa tu đã đóng
                            >
                                Lưu Thay Đổi
                            </Button>
                        </Modal.Footer>
                    </Form>
                </Modal.Body>
            </Modal>

            {/* Popup Xác Nhận Xóa Ca Trực */}
            <ConfirmationPopup
                isOpen={showDeleteNightShiftPopup}
                onClose={() => setShowDeleteNightShiftPopup(false)}
                onConfirm={handleConfirmDeleteNightShift}
                message={`Bạn có chắc chắn muốn xóa ca trực <strong>${nightShiftToDelete?.name}</strong> không?`}
                title="Xác Nhận Xóa Ca Trực"
            />
        </div>
    )
}

export default NightShiftConfig
