import React, { useEffect, useState } from "react"
import { Controller, useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { error } from "../../../utility/Message"
import { useNavigate } from "react-router-dom"
import { toastNotify } from "../../../helper"
import { fieldLabels } from "../../../utility/Label"
import { SD_CourseStatus, SD_Role_Name } from "../../../utility/SD"
import { useUpdateCourseMutation } from "../../../apis/courseApi"
import { getCourseStatusText } from "../../../helper/getCourseStatusText"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { toZonedTime, format } from "date-fns-tz"
import { apiResponse } from "../../../interfaces"
import { MainLoader } from ".."
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"

const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

const formSchema = z
    .object({
        id: z.optional(z.number()),
        courseName: z
            .string()
            .trim()
            .min(1, { message: `${error.courseNameRequired}` })
            .max(100),
        expectedStudents: z
            .number({ message: "Số lượng học sinh dự kiến phải là số và lớn hơn 0." })
            .min(1, { message: `${error.expectedStudentsGreaterThanZero}` }),
        description: z
            .string()
            .trim()
            .max(500)
            .min(1, { message: `${error.required}` }),
        startDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
        endDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
        studentApplicationStartDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
        studentApplicationEndDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
        volunteerApplicationStartDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
        volunteerApplicationEndDate: z.date({
            required_error: `${error.required}`,
            invalid_type_error: `${error.invalidDateFormat}`,
        }),
    })
    .superRefine((data, ctx) => {
        const toVietnamDate = (date: string | Date) => {
            if (!date) return null
            const d = new Date(date)
            if (isNaN(d.getTime())) return null

            // Chỉ lấy ngày tháng năm, bỏ qua giờ phút giây
            return new Date(d.getFullYear(), d.getMonth(), d.getDate())
        }

        const startDate = toVietnamDate(data.startDate)
        const endDate = toVietnamDate(data.endDate)
        const studentAppStart = toVietnamDate(data.studentApplicationStartDate)
        const studentAppEnd = toVietnamDate(data.studentApplicationEndDate)
        const volunteerAppStart = toVietnamDate(data.volunteerApplicationStartDate)
        const volunteerAppEnd = toVietnamDate(data.volunteerApplicationEndDate)

        // Ensure endDate is after startDate
        if (endDate < startDate) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.endDateGreaterThanStartDate}`,
                path: ["endDate"],
            })
        }
        console.log("startDate", volunteerAppStart)
        console.log("endDate", volunteerAppEnd)
        // Volunteer application start date must be before end date
        if (volunteerAppStart > volunteerAppEnd) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.VolunteerStartBeforeEnd}`,
                path: ["volunteerApplicationEndDate"],
            })
        }

        // Student application start date must be before end date
        if (studentAppStart > studentAppEnd) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.StudentStartBeforeEnd}`,
                path: ["studentApplicationEndDate"],
            })
        }

        // StartDate must be after volunteerApplicationStartDate and studentApplicationStartDate
        if (startDate < volunteerAppEnd || startDate < studentAppEnd) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.StartDateAfterApplications}`,
                path: ["startDate"],
            })
        }
    })

type FormData = z.infer<typeof formSchema>

function getButtonUpdateStatus(status: SD_CourseStatus): string {
    switch (status) {
        case SD_CourseStatus.notStarted:
            return "Chưa bắt đầu" // "Not Started" in Vietnamese
        case SD_CourseStatus.recruiting:
            return "Tuyển sinh" // "Recruiting"
        case SD_CourseStatus.inProgress:
            return "Bắt đầu" // "In Progress"
        case SD_CourseStatus.closed:
            return "Đã kết thúc" // "Closed"
        case SD_CourseStatus.deleted:
            return "Đã xóa" // "Deleted"
        default:
            return ""
    }
}

// Function to get the label for the next status
function getNextStatusLabel(currentStatus: SD_CourseStatus): string {
    let nextStatus: SD_CourseStatus | null = null
    switch (currentStatus) {
        case SD_CourseStatus.notStarted:
            nextStatus = SD_CourseStatus.recruiting
            break
        case SD_CourseStatus.recruiting:
            nextStatus = SD_CourseStatus.inProgress
            break
        case SD_CourseStatus.inProgress:
        case SD_CourseStatus.closed:
        case SD_CourseStatus.deleted:
            nextStatus = null // No further status
            break
        default:
            nextStatus = null
    }

    if (nextStatus !== null) {
        return getButtonUpdateStatus(nextStatus)
    } else {
        return "" // No next status
    }
}

function getNextStatus(currentStatus: SD_CourseStatus) {
    let nextStatus: SD_CourseStatus | null = null
    switch (currentStatus) {
        case SD_CourseStatus.notStarted:
            nextStatus = SD_CourseStatus.recruiting
            break
        case SD_CourseStatus.recruiting:
            nextStatus = SD_CourseStatus.inProgress
            break
        case SD_CourseStatus.inProgress:
            nextStatus = SD_CourseStatus.closed
            break
        case SD_CourseStatus.closed:
        case SD_CourseStatus.deleted:
            nextStatus = null // No further status
            break
        default:
            nextStatus = null
    }
    return nextStatus
}

function CourseInfo({ course }: { course: any }) {
    const navigate = useNavigate()
    const [isEditing, setIsEditing] = useState(false)
    const [loading, setLoading] = useState(false)
    const [updateCourse] = useUpdateCourseMutation()
    const [isDeletePopupOpen, setIsDeletePopupOpen] = useState(false)
    const [isClosePopupOpen, setIsClosePopupOpen] = useState(false)
    const [isStatusPopupOpen, setIsStatusPopupOpen] = useState(false)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const initialFormValues = {
        id: course.id || undefined,
        courseName: course.courseName || "",
        expectedStudents: course.expectedStudents || 0,
        description: course.description || "",
        startDate: course.startDate ? new Date(course.startDate) : undefined,
        endDate: course.endDate ? new Date(course.endDate) : undefined,
        studentApplicationStartDate: course.studentApplicationStartDate
            ? new Date(course.studentApplicationStartDate)
            : undefined,
        studentApplicationEndDate: course.studentApplicationEndDate
            ? new Date(course.studentApplicationEndDate)
            : undefined,
        volunteerApplicationStartDate: course.volunteerApplicationStartDate
            ? new Date(course.volunteerApplicationStartDate)
            : undefined,
        volunteerApplicationEndDate: course.volunteerApplicationEndDate
            ? new Date(course.volunteerApplicationEndDate)
            : undefined,
        secretaryLeaderId: course.secretaryLeaderId || 0,
    }
    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
        clearErrors,
        watch,
        reset,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
        defaultValues: initialFormValues,
    })
    useEffect(() => {
        reset(initialFormValues)
    }, [course])

    // Watch for date fields to dynamically set minDate and maxDate
    const studentAppStartDate = watch("studentApplicationStartDate")
    const studentAppEndDate = watch("studentApplicationEndDate")
    const volunteerAppStartDate = watch("volunteerApplicationStartDate")
    const volunteerAppEndDate = watch("volunteerApplicationEndDate")
    const startDate = watch("startDate")

    // Submit form function
    const onSubmit = async (data: FormData) => {
        setLoading(true)
        try {
            const formatDateToVietnamTimezone = (date: Date) => {
                return format(toZonedTime(date, VIETNAM_TZ), "yyyy-MM-dd'T'HH:mm:ss", {
                    timeZone: VIETNAM_TZ,
                })
            }

            const formattedInitialData = {
                courseName: course.courseName,
                expectedStudents: course.expectedStudents,
                description: course.description,
                startDate: course.startDate ? formatDateToVietnamTimezone(new Date(course.startDate)) : null,
                endDate: course.endDate ? formatDateToVietnamTimezone(new Date(course.endDate)) : null,
                studentApplicationStartDate: course.studentApplicationStartDate
                    ? formatDateToVietnamTimezone(new Date(course.studentApplicationStartDate))
                    : null,
                studentApplicationEndDate: course.studentApplicationEndDate
                    ? formatDateToVietnamTimezone(new Date(course.studentApplicationEndDate))
                    : null,
                volunteerApplicationStartDate: course.volunteerApplicationStartDate
                    ? formatDateToVietnamTimezone(new Date(course.volunteerApplicationStartDate))
                    : null,
                volunteerApplicationEndDate: course.volunteerApplicationEndDate
                    ? formatDateToVietnamTimezone(new Date(course.volunteerApplicationEndDate))
                    : null,
            }

            const formattedUpdatedData = {
                courseName: data.courseName,
                expectedStudents: data.expectedStudents,
                description: data.description,
                startDate: formatDateToVietnamTimezone(data.startDate),
                endDate: formatDateToVietnamTimezone(data.endDate),
                studentApplicationStartDate: formatDateToVietnamTimezone(data.studentApplicationStartDate),
                studentApplicationEndDate: formatDateToVietnamTimezone(data.studentApplicationEndDate),
                volunteerApplicationStartDate: formatDateToVietnamTimezone(data.volunteerApplicationStartDate),
                volunteerApplicationEndDate: formatDateToVietnamTimezone(data.volunteerApplicationEndDate),
            }

            const payload: Partial<typeof formattedUpdatedData> = {}

            ;(Object.keys(formattedUpdatedData) as Array<keyof typeof formattedUpdatedData>).forEach((key) => {
                if (formattedUpdatedData[key] !== formattedInitialData[key]) {
                    // @ts-ignore
                    payload[key] = formattedUpdatedData[key]
                }
            })

            if (Object.keys(payload).length === 0) {
                toastNotify("Không có thay đổi nào để cập nhật.", "info")
                setLoading(false)
                return
            }

            const response: apiResponse = await updateCourse({
                id: course.id,
                body: payload,
            })

            if (response.data?.isSuccess) {
                toastNotify("Cập nhật khóa tu thành công", "success")
                setIsEditing(false)
            } else {
                const errorMessage = response.error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                toastNotify(errorMessage, "error")
            }
        } catch (error) {
            toastNotify("Có lỗi xảy ra khi cập nhật khóa tu", "error")
        } finally {
            setLoading(false)
        }
    }

    const setStatus = async () => {
        const courseUpdate = {
            status: getNextStatus(course.status),
        }
        const response: apiResponse = await updateCourse({
            id: course.id,
            body: courseUpdate,
        })

        if (response.data?.isSuccess) {
            toastNotify("Cập nhật trạng thái thành công", "success")
            setIsEditing(false)
        } else {
            const errorMessage = response.error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
            toastNotify(errorMessage, "error")
        }
    }

    const setClosed = async () => {
        var courseUpdate = { status: SD_CourseStatus.closed }
        const response: apiResponse = await updateCourse({
            id: course.id,
            body: courseUpdate,
        })
        if (response.data?.isSuccess) {
            toastNotify("Kết thúc khóa tu", "success")
            setIsClosePopupOpen(false)
            setIsEditing(false)
        } else {
            const errorMessage = response.error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
            toastNotify(errorMessage, "error")
        }
    }

    const setDeleted = async () => {
        var courseUpdate = { status: SD_CourseStatus.deleted }
        const response = await updateCourse({
            id: course.id,
            body: courseUpdate,
        }).unwrap()
        if (response.isSuccess) {
            toastNotify("Xóa thành công", "success")
            navigate("/")
        } else {
            toastNotify("Có lỗi xảy ra", "error")
        }
    }

    const getStatusColor = (status: number) => {
        return status === SD_CourseStatus.closed || status === SD_CourseStatus.deleted ? "text-danger" : ""
    }

    const handleStatus = () => {
        setIsStatusPopupOpen(true)
    }

    const confirmStatus = async () => {
        setIsStatusPopupOpen(false)
        await setStatus()
    }

    const handleDelete = () => {
        setIsDeletePopupOpen(true)
    }

    const confirmDelete = async () => {
        setIsDeletePopupOpen(false)
        await setDeleted()
    }

    const handleClose = () => {
        setIsClosePopupOpen(true)
    }

    const confirmClose = async () => {
        setIsClosePopupOpen(false)
        await setClosed()
    }

    // Helper function to get today's date in Vietnam timezone
    const getVietnamToday = () => {
        const now = new Date()
        const vietnamNow = toZonedTime(now, VIETNAM_TZ)
        vietnamNow.setHours(0, 0, 0, 0)
        return vietnamNow
    }

    //get minDate for startDate field
    const getStartDateMinDate = () => {
        if (studentAppEndDate && volunteerAppEndDate) {
            const latestAppEndDate = new Date(Math.max(studentAppEndDate.getTime(), volunteerAppEndDate.getTime()))
            return latestAppEndDate
        } else {
            return null
        }
    }

    if (loading) return <MainLoader />

    return (
        <div className="">
            <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
                <div className="col-md-9">
                    <label className="form-label fw-medium">{fieldLabels.courseName}</label>
                    <input
                        {...register("courseName")}
                        className={`form-control ${errors.courseName ? "is-invalid" : ""}`}
                        type="text"
                        disabled={!isEditing}
                    />
                    {errors.courseName && <div className="invalid-feedback">{errors.courseName.message}</div>}
                </div>
                <div className="col-md-3">
                    <label className="form-label fw-medium">Trạng Thái</label>
                    <input
                        className={`form-control ${getStatusColor(course.status)} fw-medium`}
                        value={getCourseStatusText(course.status)}
                        disabled
                    />
                </div>
                {/* Description */}
                <div className="col-md-12">
                    <label className="form-label fw-medium">{fieldLabels.description}</label>
                    <textarea
                        {...register("description")}
                        className={`form-control ${errors.description ? "is-invalid" : ""}`}
                        rows={5}
                        disabled={!isEditing}
                    />
                    {errors.description && <div className="invalid-feedback">{errors.description.message}</div>}
                </div>

                {/* Student Application Dates */}
                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian đăng ký khóa sinh</label>
                    <div className="row">
                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="studentApplicationStartDate"
                                render={({ field }) => (
                                    <div>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Chọn ngày bắt đầu"
                                            className={`form-control ${
                                                errors.studentApplicationStartDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={getVietnamToday()}
                                            maxDate={studentAppEndDate || null}
                                            disabled={
                                                !isEditing ||
                                                course.status == SD_CourseStatus.recruiting ||
                                                course.status == SD_CourseStatus.inProgress
                                            }
                                        />
                                        {errors.studentApplicationStartDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.studentApplicationStartDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>
                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="studentApplicationEndDate"
                                render={({ field }) => (
                                    <>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Chọn ngày kết thúc"
                                            className={`form-control ${
                                                errors.studentApplicationEndDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={studentAppStartDate || getVietnamToday()}
                                            disabled={!isEditing || course.status == SD_CourseStatus.inProgress}
                                        />
                                        {errors.studentApplicationEndDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.studentApplicationEndDate.message}
                                            </div>
                                        )}
                                    </>
                                )}
                            />
                        </div>
                    </div>
                </div>
                <div className="col-md-2"></div>

                {/* Volunteer Application Dates */}
                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian đăng ký tình nguyện viên</label>
                    <div className="row">
                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="volunteerApplicationStartDate"
                                render={({ field }) => (
                                    <>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Chọn ngày bắt đầu"
                                            className={`form-control ${
                                                errors.volunteerApplicationStartDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={getVietnamToday()}
                                            maxDate={volunteerAppEndDate || null}
                                            disabled={
                                                !isEditing ||
                                                course.status == SD_CourseStatus.recruiting ||
                                                course.status == SD_CourseStatus.inProgress
                                            }
                                        />
                                        {errors.volunteerApplicationStartDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.volunteerApplicationStartDate.message}
                                            </div>
                                        )}
                                    </>
                                )}
                            />
                        </div>

                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="volunteerApplicationEndDate"
                                render={({ field }) => (
                                    <>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Chọn ngày kết thúc"
                                            className={`form-control ${
                                                errors.volunteerApplicationEndDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={volunteerAppStartDate || getVietnamToday()}
                                            disabled={!isEditing || course.status == SD_CourseStatus.inProgress}
                                        />
                                        {errors.volunteerApplicationEndDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.volunteerApplicationEndDate.message}
                                            </div>
                                        )}
                                    </>
                                )}
                            />
                        </div>
                    </div>
                </div>

                {/* Course Dates */}
                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian khóa tu</label>
                    <div className="row">
                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="startDate"
                                render={({ field }) => (
                                    <>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày bắt đầu"
                                            className={`form-control ${errors.startDate ? "is-invalid" : ""}`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={getStartDateMinDate()}
                                            disabled={!isEditing || course.status == SD_CourseStatus.inProgress}
                                        />
                                        {errors.startDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.startDate.message}
                                            </div>
                                        )}
                                    </>
                                )}
                            />
                        </div>

                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="endDate"
                                render={({ field }) => (
                                    <>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày kết thúc"
                                            className={`form-control ${errors.endDate ? "is-invalid" : ""}`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={startDate || null}
                                            disabled={!isEditing}
                                        />
                                        {errors.endDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.endDate.message}
                                            </div>
                                        )}
                                    </>
                                )}
                            />
                        </div>
                    </div>
                </div>
                <div className="col-md-2"></div>

                {/* Expected Students */}
                <div className="col-md-2">
                    <label className="form-label fw-medium">Số lượng khóa sinh</label>
                    <input
                        {...register("expectedStudents", { valueAsNumber: true })}
                        className={`form-control ${errors.expectedStudents ? "is-invalid" : ""}`}
                        type="number"
                        disabled={!isEditing}
                    />
                    {errors.expectedStudents && (
                        <div className="invalid-feedback">{errors.expectedStudents.message}</div>
                    )}
                </div>

                {/* Buttons */}
                <div className="row mt-4">
                    <div className="col-3 text-start">
                        <button type="button" className="btn btn-secondary me-2" onClick={() => navigate(-1)}>
                            Quay lại
                        </button>
                    </div>
                    <div className="col-9 text-end">
                        {isEditing ? (
                            <>
                                <button
                                    type="button"
                                    className="btn btn-secondary me-3"
                                    onClick={() => {
                                        setIsEditing(false)
                                        reset(initialFormValues)
                                        clearErrors() // Clear errors when canceling
                                    }}
                                >
                                    Hủy
                                </button>
                                {course.status !== SD_CourseStatus.closed && (
                                    <button type="button" className="btn btn-danger me-3" onClick={handleClose}>
                                        Kết thúc khóa tu
                                    </button>
                                )}
                                {getNextStatusLabel(course.status) != "" && (
                                    <button type="button" className="btn btn-success me-3" onClick={handleStatus}>
                                        {getNextStatusLabel(course.status)}
                                    </button>
                                )}
                                <button type="submit" className="btn btn-primary" onClick={handleSubmit(onSubmit)}>
                                    Cập nhật
                                </button>
                            </>
                        ) : (
                            course.status !== SD_CourseStatus.closed &&
                            currentUserRole == SD_Role_Name.MANAGER && (
                                <button type="button" className="btn btn-primary" onClick={() => setIsEditing(true)}>
                                    <i className="bi bi-pencil-square me-2"></i>Sửa
                                </button>
                            )
                        )}
                        {course.status === SD_CourseStatus.closed && currentUserRole == SD_Role_Name.MANAGER && (
                            <button type="button" className="btn btn-danger me-3" onClick={handleDelete}>
                                Xóa khóa tu
                            </button>
                        )}
                    </div>
                </div>
            </form>

            {/* Confirmation Popups */}
            <ConfirmationPopup
                isOpen={isDeletePopupOpen}
                onClose={() => setIsDeletePopupOpen(false)}
                onConfirm={confirmDelete}
                message="Bạn có chắc chắn muốn xóa khóa tu này không?"
            />

            <ConfirmationPopup
                isOpen={isClosePopupOpen}
                onClose={() => setIsClosePopupOpen(false)}
                onConfirm={confirmClose}
                message="Bạn có chắc chắn muốn kết thúc khóa tu này không?"
                title="Kết thúc khóa tu"
            />

            <ConfirmationPopup
                isOpen={isStatusPopupOpen}
                onClose={() => setIsStatusPopupOpen(false)}
                onConfirm={confirmStatus}
                message="Bạn có chắc chắn muốn thay đổi trạng thái khóa tu không?"
                title="Thay đổi trạng thái"
            />
        </div>
    )
}

export default CourseInfo
