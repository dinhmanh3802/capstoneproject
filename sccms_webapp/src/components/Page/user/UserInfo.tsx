// src/components/Page/user/UserInfo.tsx

import React, { useState, useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useUpdateUserMutation } from "../../../apis/userApi"
import { userModel } from "../../../interfaces/userModel"
import { SD_Gender, SD_Role, SD_UserStatus, SD_Role_Name } from "../../../utility/SD"
import { toastNotify } from "../../../helper"
import { useNavigate } from "react-router-dom"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import { OverlayTrigger, Tooltip } from "react-bootstrap"
import { MainLoader } from ".."
import { error } from "../../../utility/Message"

// Định nghĩa schema xác thực bằng Zod
const userSchema = z.object({
    id: z.number(),
    userName: z.string(),
    email: z.string().email("Email không hợp lệ").max(50, { message: error.emailTooLong }),
    fullName: z.string().trim().min(1, "Họ và tên là bắt buộc").max(100, { message: error.fullNameTooLong }),
    phoneNumber: z.string().regex(/^\d{10,11}$/, "Số điện thoại phải có 10 hoặc 11 chữ số"),
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
    address: z.string().trim().max(200, "Địa chỉ không được vượt quá 200 ký tự").min(1, "Địa chỉ là bắt buộc"),
    nationalId: z
        .string()
        .trim()
        .min(1, "Số CMND là bắt buộc")
        .regex(/^(?:\d{9}|\d{12})$/, "Số CMND phải có 9 hoặc 12 số."),
    status: z.number().refine((val) => Object.values(SD_UserStatus).includes(val), {
        message: "Trạng thái không hợp lệ",
    }),
    roleId: z.number().refine((val) => Object.values(SD_Role).includes(val), {
        message: "Vai trò không hợp lệ",
    }),
})

function UserInfo({ user }: { user: any }) {
    const [isEditing, setIsEditing] = useState(false)
    const [updateUser, { isLoading }] = useUpdateUserMutation()
    const navigate = useNavigate()
    const currentUserRoleName = useSelector((state: RootState) => state.auth.user?.role)

    const currentUser = useSelector((state: RootState) => state.auth.user) // Lấy người dùng hiện tại

    const isCurrentUser = currentUser?.userId == user.id // Kiểm tra nếu người dùng đang chỉnh sửa chính mình

    const readonlyRoles = useMemo(() => [SD_Role.ADMIN, SD_Role.MANAGER, SD_Role.SECRETARY], [])

    const isRoleEditable = useMemo(() => {
        if (
            currentUser?.role === SD_Role_Name.MANAGER &&
            (user.roleId === SD_Role.ADMIN || user.roleId === SD_Role.SECRETARY)
        ) {
            return false
        }
        return !readonlyRoles.includes(user.roleId)
    }, [currentUser, user.roleId, readonlyRoles])

    const allRoles = useMemo(
        () => [
            { value: SD_Role.ADMIN, label: "Quản trị viên" },
            { value: SD_Role.MANAGER, label: "Quản lý" },
            { value: SD_Role.SECRETARY, label: "Thư ký" },
            { value: SD_Role.STAFF, label: "Nhân viên" },
            { value: SD_Role.SUPERVISOR, label: "Huynh trưởng" },
            { value: SD_Role.TEAM_LEADER, label: "Trưởng ban" },
        ],
        [],
    )

    const editableRoles = useMemo(
        () => [
            { value: SD_Role.STAFF, label: "Nhân viên" },
            { value: SD_Role.SUPERVISOR, label: "Huynh trưởng" },
            { value: SD_Role.TEAM_LEADER, label: "Trưởng ban" },
        ],
        [],
    )

    const roleOptions = useMemo(
        () => (isEditing && isRoleEditable ? editableRoles : allRoles),
        [isEditing, isRoleEditable, editableRoles, allRoles],
    )

    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
        reset,
        clearErrors,
        setError,
    } = useForm<userModel>({
        resolver: zodResolver(userSchema),
    })

    useEffect(() => {
        if (user) {
            reset({
                id: user.id,
                userName: user.userName,
                email: user.email,
                fullName: user.fullName,
                phoneNumber: user.phoneNumber || "",
                gender: user.gender !== undefined ? user.gender : SD_Gender.Male,
                dateOfBirth: user.dateOfBirth || "",
                address: user.address || "",
                nationalId: user.nationalId,
                status: user.status !== undefined ? user.status : SD_UserStatus.ACTIVE,
                roleId: user.roleId !== undefined ? user.roleId : SD_Role.STAFF,
            })
        }
    }, [user, reset])

    const onSubmit = async (data: userModel) => {
        // Nếu người dùng là Admin hoặc chính mình, không cho phép thay đổi trạng thái
        if (user.roleId === SD_Role.ADMIN || isCurrentUser) {
            data.status = user.status // Giữ nguyên trạng thái hiện tại
        }

        try {
            const response = await updateUser({ id: data.id, body: data }).unwrap()
            if (response.isSuccess) {
                toastNotify("Cập nhật thông tin người dùng thành công!", "success")
                setIsEditing(false)
            } else {
                toastNotify("Cập nhật thông tin người dùng thất bại!", "error")
            }
        } catch (err: any) {
            console.error(err)
            const errorMessages = err?.data?.errorMessages || ["Có lỗi xảy ra khi cập nhật thông tin người dùng"]
            errorMessages.forEach((msg: string) => {
                if (msg.includes("Email đã tồn tại")) {
                    setError("email", { type: "manual", message: msg })
                }
                if (msg.includes("Số điện thoại đã tồn tại")) {
                    setError("phoneNumber", { type: "manual", message: msg })
                }
                if (msg.includes("Mã định danh đã tồn tại")) {
                    setError("nationalId", { type: "manual", message: msg })
                }
            })
        }
    }

    const handleCancel = () => {
        reset({
            id: user.id,
            userName: user.userName,
            email: user.email,
            fullName: user.fullName,
            phoneNumber: user.phoneNumber || "",
            gender: user.gender !== undefined ? user.gender : SD_Gender.Male,
            dateOfBirth: user.dateOfBirth || "",
            address: user.address || "",
            nationalId: user.nationalId,
            status: user.status !== undefined ? user.status : SD_UserStatus.ACTIVE,
            roleId: user.roleId !== undefined ? user.roleId : SD_Role.STAFF,
        })
        clearErrors()
        setIsEditing(false)
    }

    if (isLoading) return <MainLoader />

    return (
        <div className="">
            <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
                {/* Trường ẩn để đăng ký id */}
                <input type="hidden" {...register("id")} />

                {/* UserName */}
                <div className="col-md-3">
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
                <div className="col-md-3">
                    <label htmlFor="fullName" className="form-label">
                        Họ và Tên
                    </label>
                    <input
                        {...register("fullName")}
                        className={`form-control ${errors.fullName ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
                </div>
                {/* DateOfBirth */}
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
                {/* PhoneNumber */}
                <div className="col-md-3">
                    <label htmlFor="phoneNumber" className="form-label">
                        Số điện thoại
                    </label>
                    <input
                        {...register("phoneNumber")}
                        className={`form-control ${errors.phoneNumber ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber.message}</div>}
                </div>
                {/* NationalId */}
                <div className="col-md-3">
                    <label htmlFor="nationalId" className="form-label">
                        Số CMND/CCCD
                    </label>
                    <input
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
                        {...register("email")}
                        className={`form-control ${errors.email ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
                </div>

                {/* Gender */}
                <div className="col-md-3">
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
                {/* Status */}
                <div className="col-md-3">
                    <label htmlFor="status" className="form-label">
                        Trạng thái
                    </label>
                    <OverlayTrigger
                        placement="top"
                        overlay={
                            user.roleId === SD_Role.ADMIN || isCurrentUser ? (
                                <Tooltip id={`tooltip-status-${user.id}`}>
                                    {user.roleId === SD_Role.ADMIN
                                        ? "Không thể thay đổi trạng thái của người dùng Admin."
                                        : "Không thể thay đổi trạng thái của chính bạn."}
                                </Tooltip>
                            ) : (
                                <></>
                            )
                        }
                    >
                        <select
                            {...register("status", { valueAsNumber: true })}
                            className={`form-select ${errors.status ? "is-invalid" : ""}`}
                            disabled={!isEditing || user.roleId === SD_Role.ADMIN || isCurrentUser} // Thêm điều kiện disable nếu là Admin hoặc chính mình
                        >
                            <option value="">Chọn trạng thái</option>
                            <option value={SD_UserStatus.ACTIVE}>Hoạt động</option>
                            <option value={SD_UserStatus.DEACTIVE}>Không hoạt động</option>
                        </select>
                    </OverlayTrigger>
                    {errors.status && <div className="invalid-feedback">{errors.status.message}</div>}
                </div>
                {/* Address */}
                <div className="col-md-6">
                    <label htmlFor="address" className="form-label">
                        Địa chỉ
                    </label>
                    <input
                        {...register("address")}
                        className={`form-control ${errors.address ? "is-invalid" : ""}`}
                        disabled={!isEditing}
                    />
                    {errors.address && <div className="invalid-feedback">{errors.address.message}</div>}
                </div>

                {/* RoleId */}
                <div className="col-md-6">
                    <label htmlFor="roleId" className="form-label">
                        Vai trò
                    </label>
                    <select
                        {...register("roleId", { valueAsNumber: true })}
                        className={`form-select ${errors.roleId ? "is-invalid" : ""}`}
                        disabled={!isEditing || !isRoleEditable}
                    >
                        <option value="">Chọn vai trò</option>
                        {roleOptions?.map((role) => (
                            <option key={role.value} value={role.value}>
                                {role.label}
                            </option>
                        ))}
                    </select>
                    {errors.roleId && <div className="invalid-feedback">{errors.roleId.message}</div>}
                </div>

                {/* Nút Hành Động */}
                <div className="col-12 text-end">
                    {isEditing ? (
                        <>
                            <button type="button" className="btn btn-secondary me-3" onClick={handleCancel}>
                                Hủy
                            </button>
                            <button type="submit" className="btn btn-primary">
                                Cập nhật
                            </button>
                            <button
                                type="button"
                                className="btn btn-outline-secondary ms-3"
                                onClick={() => navigate(-1)}
                            >
                                Quay lại
                            </button>
                        </>
                    ) : (
                        <>
                            {(currentUserRoleName === SD_Role_Name.MANAGER ||
                                currentUserRoleName === SD_Role_Name.ADMIN) && (
                                <button
                                    type="button"
                                    className="btn btn-primary me-3"
                                    onClick={() => setIsEditing(true)}
                                >
                                    <i className="bi bi-pencil-square me-2"></i>Sửa
                                </button>
                            )}

                            <button type="button" className="btn btn-outline-secondary" onClick={() => navigate(-1)}>
                                Quay lại
                            </button>
                        </>
                    )}
                </div>
            </form>
        </div>
    )
}

export default UserInfo
