import React, { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { useNavigate } from "react-router-dom"
import jwtDecode from "jwt-decode"
import { useGetUserByIdQuery, useUpdateUserMutation } from "../../apis/userApi"
import { userModel } from "../../interfaces/userModel"
import { apiResponse } from "../../interfaces"
import "bootstrap/dist/css/bootstrap.min.css"
import { SD_Gender } from "../../utility/SD"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toastNotify } from "../../helper"
import { error } from "../../utility/Message"

// Định nghĩa schema xác thực bằng zod
const userProfileSchema = z.object({
    fullName: z.string().trim().min(1, "Họ và tên là bắt buộc").max(100, { message: error.fullNameTooLong }),
    email: z.string().email("Email không hợp lệ").max(50, { message: error.emailTooLong }),
    phoneNumber: z.string().regex(/^\d{10}$/, "Số điện thoại phải có 10 chữ số"),
    nationalId: z.string().regex(/^(?:\d{9}|\d{12})$/, "Số CMND phải có 9 hoặc 12 số."),
    address: z.string().trim().min(1, "Địa chỉ là bắt buộc").max(300, { message: error.addressTooLong }),
    gender: z.number().min(0, "Giới tính là bắt buộc").max(2, "Giới tính không hợp lệ"),
    dateOfBirth: z
        .string()
        .min(0, "Ngày sinh là bắt buộc")
        .refine(
            (date) => {
                const parsedDate = Date.parse(date)
                return !isNaN(parsedDate) && new Date(parsedDate) < new Date()
            },
            { message: "Ngày sinh không được ở tương lai." },
        ),
    roleId: z.number().optional(),
    userName: z.string().optional(),
    status: z.number().optional(),
})

function UserProfile() {
    const token = localStorage.getItem("token")
    let userId: number | undefined
    const navigate = useNavigate()

    // Giải mã token và lấy userId
    if (token) {
        try {
            const decoded: { userId: number } = jwtDecode(token)
            userId = decoded.userId
        } catch (error) {
            console.error("Lỗi khi giải mã token:", error)
        }
    }

    // Lấy thông tin người dùng
    const { data: apiResponse, isLoading, isError } = useGetUserByIdQuery(userId!)
    const userProfile = apiResponse?.result

    const [updateUser] = useUpdateUserMutation()
    const [isEditing, setIsEditing] = useState(false) // Thêm state để kiểm soát chế độ chỉnh sửa
    const {
        register,
        handleSubmit,
        setValue,
        setError,
        reset,
        clearErrors,
        formState: { errors },
    } = useForm<userModel>({
        resolver: zodResolver(userProfileSchema),
    })

    useEffect(() => {
        if (userProfile) {
            setValue("fullName", userProfile.fullName)
            setValue("email", userProfile.email)
            setValue("phoneNumber", userProfile.phoneNumber)
            setValue("address", userProfile.address)
            setValue("gender", userProfile.gender)
            if (userProfile.dateOfBirth) {
                const formattedDate = userProfile.dateOfBirth.split("T")[0]
                setValue("dateOfBirth", formattedDate)
            }
            setValue("nationalId", userProfile.nationalId)
            setValue("roleId", userProfile.roleId)
            setValue("userName", userProfile.userName)
            setValue("status", userProfile.status) // Đặt giá trị cho Status
        }
    }, [userProfile, setValue])

    const resetForm = () => {
        if (userProfile) {
            setValue("fullName", userProfile.fullName)
            setValue("email", userProfile.email)
            setValue("phoneNumber", userProfile.phoneNumber)
            setValue("address", userProfile.address)
            setValue("gender", userProfile.gender)
            if (userProfile.dateOfBirth) {
                const formattedDate = userProfile.dateOfBirth.split("T")[0]
                setValue("dateOfBirth", formattedDate)
            }
            setValue("nationalId", userProfile.nationalId)
            setValue("roleId", userProfile.roleId)
            setValue("userName", userProfile.userName)
            setValue("status", userProfile.status) // Đặt lại giá trị cho Status
        }
        clearErrors() // Xóa bỏ tất cả các thông báo lỗi
    }

    const onSubmit = async (data: userModel) => {
        try {
            data.gender = Number(data.gender) // Chuyển đổi sang số an toàn hơn
            data.status = Number(data.status) // Chuyển đổi sang số an toàn hơn

            // Gọi API để cập nhật thông tin người dùng
            const updateResponse: apiResponse = await updateUser({ id: userId!, body: data })

            if (updateResponse.data?.isSuccess) {
                toastNotify("Cập nhật thông tin thành công!")
                setIsEditing(false) // Đặt lại chế độ chỉnh sửa thành false sau khi cập nhật thành công
            } else {
                // Nếu có lỗi, phân tích thông báo lỗi và gán vào các trường tương ứng
                const errorMessages = updateResponse.error?.data?.errorMessages || []
                errorMessages.forEach((msg: string) => {
                    if (msg.includes("Email đã tồn tại")) {
                        setError("email", { type: "manual", message: msg })
                    }
                    if (msg.includes("Số điện thoại đã tồn tại.")) {
                        setError("phoneNumber", { type: "manual", message: msg })
                    }
                    if (msg.includes("Mã định danh đã tồn tại.")) {
                        setError("nationalId", { type: "manual", message: msg })
                    }
                })
            }
        } catch (error) {
            toastNotify("Đã có lỗi xảy ra khi cập nhật thông tin.", "error")
            console.error("Error updating user:", error)
        }
    }

    if (isLoading) {
        return <p>Đang tải dữ liệu...</p>
    }

    if (isError || !userProfile) {
        return <p>Đã xảy ra lỗi khi tải thông tin người dùng.</p>
    }

    return (
        <div className="container mt-5">
            <h2 className="mb-4 fw-bold">Thông Tin Tài Khoản</h2>

            <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
                {/* Input ẩn cho Status */}
                <input type="hidden" {...register("status")} />

                {/* Họ và tên */}
                <div className="col-md-3">
                    <label htmlFor="fullName" className="form-label">
                        Họ và Tên
                    </label>
                    <input
                        type="text"
                        {...register("fullName")}
                        className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
                </div>

                {/* Ngày sinh */}
                <div className="col-md-3">
                    <label htmlFor="dateOfBirth" className="form-label">
                        Ngày sinh
                    </label>
                    <input
                        type="date"
                        {...register("dateOfBirth")}
                        className={`form-control ${errors.dateOfBirth ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.dateOfBirth && <div className="invalid-feedback">{errors.dateOfBirth.message}</div>}
                </div>

                {/* Số điện thoại */}
                <div className="col-md-3">
                    <label htmlFor="phoneNumber" className="form-label">
                        Số điện thoại
                    </label>
                    <input
                        type="text"
                        {...register("phoneNumber")}
                        className={`form-control ${errors.phoneNumber ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber.message}</div>}
                </div>

                {/* Số CMND */}
                <div className="col-md-3">
                    <label htmlFor="nationalId" className="form-label">
                        Số CMND
                    </label>
                    <input
                        type="text"
                        {...register("nationalId")}
                        className={`form-control ${errors.nationalId ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.nationalId && <div className="invalid-feedback">{errors.nationalId.message}</div>}
                </div>

                {/* Email */}
                <div className="col-md-3">
                    <label htmlFor="email" className="form-label">
                        Email
                    </label>
                    <input
                        type="email"
                        {...register("email")}
                        className={`form-control ${errors.email ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
                </div>

                {/* Giới tính */}
                <div className="col-md-1">
                    <label htmlFor="gender" className="form-label">
                        Giới tính
                    </label>
                    <select
                        {...register("gender", { valueAsNumber: true })}
                        className={`form-select ${errors.gender ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    >
                        <option value="">Chọn giới tính</option>
                        <option value={SD_Gender.Male}>Nam</option>
                        <option value={SD_Gender.Female}>Nữ</option>
                    </select>
                    {errors.gender && <div className="invalid-feedback">{errors.gender.message}</div>}
                </div>

                {/* Vai trò */}
                <div className="col-md-2">
                    <label htmlFor="roleId" className="form-label">
                        Vai trò
                    </label>
                    <select
                        {...register("roleId")}
                        className="form-select"
                        disabled // Disabled vì người dùng không cần chỉnh sửa role
                    >
                        <option value="1">Quản trị viên</option>
                        <option value="2">Quản lý</option>
                        <option value="3">Nhân viên</option>
                        <option value="4">Thư ký</option>
                    </select>
                </div>

                {/* Địa chỉ */}
                <div className="col-md-6">
                    <label htmlFor="address" className="form-label">
                        Địa chỉ
                    </label>
                    <input type="text" {...register("address")} className="form-control" disabled={!isEditing} />
                </div>

                {/* Nút hành động */}
                <div className="col d-flex justify-content-end align-self-end">
                    {isEditing ? (
                        <>
                            <button type="submit" className="btn btn-primary me-3 col-md-1">
                                Cập nhật
                            </button>
                            <button
                                type="button"
                                className="btn btn-secondary me-3 col-md-1"
                                onClick={() => {
                                    resetForm()
                                    setIsEditing(false)
                                }}
                            >
                                Hủy
                            </button>
                        </>
                    ) : (
                        <button
                            type="button"
                            className="btn btn-primary me-3 col-md-1"
                            onClick={() => setIsEditing(true)}
                        >
                            Sửa
                        </button>
                    )}
                    <button type="button" className="btn btn-secondary col-md-1" onClick={() => navigate(-1)}>
                        Quay lại
                    </button>
                </div>
            </form>
        </div>
    )
}

export default UserProfile
