import React, { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useUpdateSupervisorMutation } from "../../../apis/supervisorApi"
import { supervisorModel } from "../../../interfaces/supervisorModel"
import { SD_Gender, SD_UserStatus } from "../../../utility/SD"
import { format } from "date-fns"
import { useNavigate } from "react-router-dom"
import { MainLoader } from ".."

const supervisorSchema = z.object({
    id: z.number(),
    userName: z.string(),
    email: z.string().email("Email không hợp lệ"),
    fullName: z.string().trim().min(1, "Họ và tên là bắt buộc"),
    phoneNumber: z
        .string()
        .trim()
        .regex(/^\d{10,11}$/, "Số điện thoại phải có 10 hoặc 11 chữ số"),
    gender: z.number().refine((val) => Object.values(SD_Gender).includes(val), {
        message: "Giới tính không hợp lệ",
    }),
    dateOfBirth: z
        .string()
        .min(1, "Ngày sinh là bắt buộc")
        .refine(
            (date) => {
                if (!date) return true
                const parsedDate = Date.parse(date)
                return !isNaN(parsedDate) && new Date(parsedDate) < new Date()
            },
            { message: "Ngày sinh không được ở tương lai." },
        ),
    address: z.string().max(200, "Địa chỉ không được vượt quá 200 ký tự").min(1, "Địa chỉ là bắt buộc"),
    nationalId: z
        .string()
        .min(1, "Số CMND là bắt buộc")
        .regex(/^(?:\d{9}|\d{12})$/, "Số CMND phải có 9 hoặc 12 số."),
    status: z.number().refine((val) => Object.values(SD_UserStatus).includes(val), {
        message: "Trạng thái không hợp lệ",
    }),
})

function SupervisorInfo({ supervisor }: { supervisor: supervisorModel }) {
    const [updateSupervisor, { isLoading }] = useUpdateSupervisorMutation()
    const navigate = useNavigate()

    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
        reset,
        clearErrors,
        setError,
    } = useForm<supervisorModel>({
        resolver: zodResolver(supervisorSchema),
    })

    useEffect(() => {
        if (supervisor) {
            reset({
                id: supervisor.id,
                userName: supervisor.userName,
                email: supervisor.email,
                fullName: supervisor.fullName,
                phoneNumber: supervisor.phoneNumber || "",
                gender: supervisor.gender !== undefined ? supervisor.gender : SD_Gender.Male,
                dateOfBirth: format(new Date(supervisor.dateOfBirth), "dd-MM-yyyy") || "",
                address: supervisor.address || "",
                nationalId: supervisor.nationalId,
                status: supervisor.status !== undefined ? supervisor.status : SD_UserStatus.ACTIVE,
            })
        }
    }, [supervisor, reset])

    if (isLoading) return <MainLoader />

    return (
        <div className="">
            <form className="row g-3">
                {/* Trường ẩn để đăng ký id */}
                <input type="hidden" {...register("id")} />

                {/* UserName */}
                <div className="col-md-4">
                    <label htmlFor="userName" className="form-label">
                        Tên đăng nhập
                    </label>
                    <input
                        {...register("userName")}
                        className={`form-control ${errors.userName ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.userName && <div className="invalid-feedback">{errors.userName.message}</div>}
                </div>
                {/* FullName */}
                <div className="col-md-4">
                    <label htmlFor="fullName" className="form-label">
                        Họ và Tên
                    </label>
                    <input
                        {...register("fullName")}
                        className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
                </div>
                {/* DateOfBirth */}
                <div className="col-md-4">
                    <label htmlFor="dateOfBirth" className="form-label">
                        Ngày sinh
                    </label>
                    <input
                        type="text"
                        {...register("dateOfBirth")}
                        className={`form-control ${errors.dateOfBirth ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.dateOfBirth && <div className="invalid-feedback">{errors.dateOfBirth.message}</div>}
                </div>
                {/* PhoneNumber */}
                <div className="col-md-4">
                    <label htmlFor="phoneNumber" className="form-label">
                        Số điện thoại
                    </label>
                    <input
                        {...register("phoneNumber")}
                        className={`form-control ${errors.phoneNumber ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber.message}</div>}
                </div>
                {/* NationalId */}
                <div className="col-md-4">
                    <label htmlFor="nationalId" className="form-label">
                        Số CMND/CCCD
                    </label>
                    <input
                        {...register("nationalId")}
                        className={`form-control ${errors.nationalId ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.nationalId && <div className="invalid-feedback">{errors.nationalId.message}</div>}
                </div>
                {/* Email */}
                <div className="col-md-4">
                    <label htmlFor="email" className="form-label">
                        Email
                    </label>
                    <input
                        {...register("email")}
                        className={`form-control ${errors.email ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
                </div>

                {/* Gender */}
                <div className="col-md-4">
                    <label htmlFor="gender" className="form-label">
                        Giới tính
                    </label>
                    <select
                        {...register("gender", { valueAsNumber: true })}
                        className={`form-select ${errors.gender ? "is-invalid" : ""}`}
                        disabled
                    >
                        <option value="">Chọn giới tính</option>
                        <option value={SD_Gender.Male}>Nam</option>
                        <option value={SD_Gender.Female}>Nữ</option>
                    </select>
                    {errors.gender && <div className="invalid-feedback">{errors.gender.message}</div>}
                </div>

                {/* Address */}
                <div className="col-md-4">
                    <label htmlFor="address" className="form-label">
                        Địa chỉ
                    </label>
                    <input
                        {...register("address")}
                        className={`form-control ${errors.address ? "is-invalid" : ""}`}
                        disabled
                    />
                    {errors.address && <div className="invalid-feedback">{errors.address.message}</div>}
                </div>
                {/* Status */}
                <div className="col-md-4">
                    <label htmlFor="status" className="form-label">
                        Trạng thái
                    </label>
                    <select
                        {...register("status", { valueAsNumber: true })}
                        className={`form-select ${errors.status ? "is-invalid" : ""}`}
                        disabled
                    >
                        <option value="">Chọn trạng thái</option>
                        <option value={SD_UserStatus.ACTIVE}>Hoạt động</option>
                        <option value={SD_UserStatus.DEACTIVE}>Không hoạt động</option>
                    </select>
                    {errors.status && <div className="invalid-feedback">{errors.status.message}</div>}
                </div>

                {/* Nút Hành Động */}
                <div className="col-12 text-end">
                    <button type="button" className="btn btn-outline-secondary" onClick={() => navigate(-1)}>
                        Quay lại
                    </button>
                </div>
            </form>
        </div>
    )
}

export default SupervisorInfo
