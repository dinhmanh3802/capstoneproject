import { useState } from "react"
import { Controller, useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { fieldLabels } from "../../utility/Label"
import { error } from "../../utility/Message"
import { useCreateCourseMutation } from "../../apis/courseApi"
import apiResponse from "../../interfaces/apiResponse"
import { toastNotify } from "../../helper"
import { useNavigate } from "react-router-dom"
import { MainLoader } from "../../components/Page"
import DatePicker from "react-datepicker"
import "react-datepicker/dist/react-datepicker.css"
import { toZonedTime, format } from "date-fns-tz"
import React from "react"

const VIETNAM_TZ = "Asia/Ho_Chi_Minh"

const formSchema = z
    .object({
        courseName: z
            .string()
            .trim()
            .min(1, { message: `${error.courseNameRequired}` })
            .max(100),
        expectedStudents: z
            .number({ message: `${error.invalidValue}` })
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
        // Helper function to convert dates to Vietnam timezone
        const toVietnamDate = (date: string | Date) => {
            if (!date) return null
            const d = new Date(date)
            if (isNaN(d.getTime())) return null
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

        // Ensure volunteer application end date is after start date
        if (volunteerAppStart > volunteerAppEnd) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.VolunteerStartBeforeEnd}`,
                path: ["volunteerApplicationEndDate"],
            })
        }

        // Ensure student application end date is after start date
        if (studentAppStart > studentAppEnd) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: `${error.StudentStartBeforeEnd}`,
                path: ["studentApplicationEndDate"],
            })
        }
    })

type FormData = z.infer<typeof formSchema>
function CreateCourse() {
    const [loading, setLoading] = useState(false)
    const [createCourse] = useCreateCourseMutation()
    const navigate = useNavigate()

    const {
        register,
        control,
        handleSubmit,
        formState: { errors },
        watch,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            courseName: "",
            expectedStudents: undefined,
            description: "",
            startDate: undefined,
            endDate: undefined,
            studentApplicationStartDate: undefined,
            studentApplicationEndDate: undefined,
            volunteerApplicationStartDate: undefined,
            volunteerApplicationEndDate: undefined,
        },
    })

    // Watch for date fields to dynamically set minDate and maxDate
    const studentAppStartDate = watch("studentApplicationStartDate")
    const studentAppEndDate = watch("studentApplicationEndDate")
    const volunteerAppStartDate = watch("volunteerApplicationStartDate")
    const volunteerAppEndDate = watch("volunteerApplicationEndDate")
    const startDate = watch("startDate")

    const onSubmit = async (data: FormData) => {
        setLoading(true)

        // Helper function to format dates in Vietnam timezone
        const formatDateToVietnamTimezone = (date) => {
            return format(toZonedTime(date, VIETNAM_TZ), "yyyy-MM-dd'T'HH:mm:ss", { timeZone: VIETNAM_TZ })
        }

        // Convert Date objects to formatted strings in Vietnam timezone
        const formattedData = {
            ...data,
            startDate: formatDateToVietnamTimezone(data.startDate),
            endDate: formatDateToVietnamTimezone(data.endDate),
            studentApplicationStartDate: formatDateToVietnamTimezone(data.studentApplicationStartDate),
            studentApplicationEndDate: formatDateToVietnamTimezone(data.studentApplicationEndDate),
            volunteerApplicationStartDate: formatDateToVietnamTimezone(data.volunteerApplicationStartDate),
            volunteerApplicationEndDate: formatDateToVietnamTimezone(data.volunteerApplicationEndDate),
        }

        const response: apiResponse = await createCourse(formattedData)
        if (response.data?.isSuccess) {
            toastNotify("Tạo khóa tu thành công", "success")
            navigate("/course")
        } else {
            // Handle errors returned by the API
            const errorMessage = response.error.data?.errorMessages?.join(", ") || "Có lỗi trong quá trình tạo"
            toastNotify(errorMessage, "error")
        }
        setLoading(false)
    }
    if (loading) return <MainLoader />

    // Helper function to get today's date in Vietnam timezone
    const getVietnamToday = () => {
        const now = new Date()
        const vietnamNow = toZonedTime(now, VIETNAM_TZ)
        vietnamNow.setHours(0, 0, 0, 0)
        return vietnamNow
    }

    // Helper function to get minDate for startDate field
    const getStartDateMinDate = () => {
        if (studentAppEndDate && volunteerAppEndDate) {
            const latestAppEndDate = new Date(Math.max(studentAppEndDate.getTime(), volunteerAppEndDate.getTime()))
            return latestAppEndDate
        } else {
            return null
        }
    }

    // Helper function to get the later of two dates
    const getLaterDate = (date1, date2) => {
        if (!date1) return date2
        if (!date2) return date1
        return date1 > date2 ? date1 : date2
    }

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Tạo Khóa Tu</h3>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
                <div className="col-md-12">
                    <label className="form-label fw-medium">{fieldLabels.courseName}</label>
                    <input
                        {...register("courseName")}
                        className={`form-control ${errors.courseName ? "is-invalid" : ""}`}
                        type="text"
                        placeholder="Nhập tên khóa tu..."
                    />
                    {errors.courseName && <div className="invalid-feedback">{errors.courseName.message}</div>}
                </div>
                <div className="col-md-12">
                    <label className="form-label fw-medium">{fieldLabels.description}</label>
                    <textarea
                        {...register("description")}
                        className={`form-control ${errors.description ? "is-invalid" : ""}`}
                        placeholder="Viết mô tả cho khóa tu..."
                        rows={5}
                    />
                    {errors.description && <div className="invalid-feedback">{errors.description.message}</div>}
                </div>
                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian đăng ký khóa sinh</label>
                    <div className="row">
                        <div className="col-md-5">
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
                                    // @ts-ignore
                                    <div>
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
                                        />
                                        {errors.studentApplicationEndDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.studentApplicationEndDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>
                    </div>
                </div>
                <div className="col-md-2"></div>
                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian đăng ký tình nguyện viên</label>
                    <div className="row">
                        <div className="col-md-5">
                            <Controller
                                control={control}
                                name="volunteerApplicationStartDate"
                                render={({ field }) => (
                                    <div>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày bắt đầu"
                                            className={`form-control ${
                                                errors.volunteerApplicationStartDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={getVietnamToday()}
                                            maxDate={volunteerAppEndDate || null}
                                        />
                                        {errors.volunteerApplicationStartDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.volunteerApplicationStartDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>
                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="volunteerApplicationEndDate"
                                render={({ field }) => (
                                    <div>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày kết thúc"
                                            className={`form-control ${
                                                errors.volunteerApplicationEndDate ? "is-invalid" : ""
                                            }`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={volunteerAppStartDate || getVietnamToday()}
                                        />
                                        {errors.volunteerApplicationEndDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.volunteerApplicationEndDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>
                    </div>
                </div>

                <div className="col-md-5">
                    <label className="form-label fw-medium">Thời gian khóa tu</label>
                    <div className="row">
                        <div className="col-md-5">
                            <Controller
                                control={control}
                                name="startDate"
                                render={({ field }) => (
                                    <div>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày bắt đầu"
                                            className={`form-control ${errors.startDate ? "is-invalid" : ""}`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={getStartDateMinDate()}
                                        />
                                        {errors.startDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.startDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>

                        <div className="col-md-6">
                            <Controller
                                control={control}
                                name="endDate"
                                render={({ field }) => (
                                    <div>
                                        {/* @ts-ignore */}
                                        <DatePicker
                                            placeholderText="Ngày kết thúc"
                                            className={`form-control ${errors.endDate ? "is-invalid" : ""}`}
                                            selected={field.value}
                                            onChange={(date) => field.onChange(date)}
                                            dateFormat="dd/MM/yyyy"
                                            minDate={startDate || null}
                                        />
                                        {errors.endDate && (
                                            <div className="invalid-feedback" style={{ display: "block" }}>
                                                {errors.endDate.message}
                                            </div>
                                        )}
                                    </div>
                                )}
                            />
                        </div>
                    </div>
                </div>
                <div className="col-md-2"></div>
                <div className="col-md-2">
                    <label className="form-label fw-medium">Số lượng khóa sinh</label>
                    <input
                        {...register("expectedStudents", {
                            valueAsNumber: true,
                        })}
                        className={`form-control ${errors.expectedStudents ? "is-invalid" : ""}`}
                        type="number"
                        placeholder="Số lượng..."
                    />
                    {errors.expectedStudents && (
                        <div className="invalid-feedback">{errors.expectedStudents.message}</div>
                    )}
                </div>

                <div className="col-12 text-end">
                    <button type="button" className="btn btn-secondary me-3" onClick={() => navigate(-1)}>
                        Quay lại
                    </button>
                    <button type="submit" className="btn btn-primary">
                        Tạo khóa tu
                    </button>
                </div>
            </form>
        </div>
    )
}

export default CreateCourse
