// src/components/User/CreateUser.tsx

import React, { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { button, fieldLabels } from "../../utility/Label"
import { error } from "../../utility/Message"
import { useCreateUserMutation } from "../../apis/userApi"
import apiResponse from "../../interfaces/apiResponse"
import { toastNotify } from "../../helper"
import { MainLoader } from "../../components/Page"
import jwtDecode from "jwt-decode"
import { SD_Role, SD_Role_Name } from "../../utility/SD"

const allowedRoles = [
    { id: SD_Role.SECRETARY, name: "Thư ký" },
    { id: SD_Role.STAFF, name: "Nhân viên" },
    { id: SD_Role.TEAM_LEADER, name: "Trưởng ban" },
    { id: SD_Role.SUPERVISOR, name: "Huynh trưởng" },
]

const formSchema = z.object({
    fullName: z.string().trim().min(1, { message: error.required }).max(100, { message: error.fullNameTooLong }),
    dateOfBirth: z.string().refine(
        (date) => {
            const parsedDate = new Date(date)
            return parsedDate <= new Date()
        },
        { message: "Ngày sinh không được ở tương lai" },
    ),
    phoneNumber: z.string().regex(/^\d{10,11}$/, { message: "Số điện thoại không hợp lệ, phải là 10 hoặc 11 số" }),
    nationalId: z.string().regex(/^\d{9}(\d{3})?$/, { message: "Số CMND phải là 9 hoặc 12 số" }),
    email: z.string().email({ message: "Email không hợp lệ" }).max(50, { message: error.emailTooLong }),
    gender: z.number().min(0, { message: error.required }),
    address: z.string().trim().min(1, { message: error.required }).max(300, { message: error.addressTooLong }),
    roleId: z
        .string()
        .min(1, { message: "Vui lòng chọn vai trò" }) // Custom message for empty selection
        .refine((val) => ["3", "4", "5", "6"].includes(val), { message: "Vai trò không hợp lệ" }) // Custom message for invalid selection
        .transform((val) => Number(val)), // Convert to number
    createdBy: z.number().optional(),
})

type FormData = z.infer<typeof formSchema>

interface CreateUserProps {
    onClose: () => void // Prop để đóng Modal
}

function CreateUser({ onClose }: CreateUserProps) {
    const [loading, setLoading] = useState(false)
    const [createUser] = useCreateUserMutation()
    const [creatorId, setCreatorId] = useState<number | undefined>()

    useEffect(() => {
        const token = localStorage.getItem("token")
        if (token) {
            try {
                const decoded: { role: string; userId: number } = jwtDecode(token)
                setCreatorId(decoded.userId)
            } catch (error) {
                console.error("Lỗi khi giải mã token:", error)
            }
        }
    }, [])

    const {
        register,
        handleSubmit,
        formState: { errors },
        setError, // Thêm setError vào đây
        clearErrors, // Thêm clearErrors để xóa lỗi trước khi submit lại
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    const onSubmit = async (data: FormData) => {
        setLoading(true)
        clearErrors() // Xóa tất cả lỗi trước khi submit lại

        data.createdBy = creatorId || 0
        data.gender = Number(data.gender)

        try {
            const response: apiResponse = await createUser(data)
            if (response.data?.isSuccess) {
                toastNotify("Tạo người dùng thành công!", "success")
                onClose() // Đóng Modal sau khi tạo thành công
            } else {
                const errorMessages = response.error.data?.errorMessages || []
                // Đặt lỗi cho từng trường dựa trên thông báo lỗi từ backend
                errorMessages.forEach((message: string) => {
                    if (message.includes("Email đã tồn tại")) {
                        setError("email", { type: "manual", message: "Email đã tồn tại." })
                    }
                    if (message.includes("Số điện thoại đã tồn tại")) {
                        setError("phoneNumber", { type: "manual", message: "Số điện thoại đã tồn tại." })
                    }
                    if (message.includes("Mã định danh đã tồn tại")) {
                        setError("nationalId", { type: "manual", message: "Mã định danh đã tồn tại." })
                    }
                    if (message.includes("RoleId không tồn tại")) {
                        setError("roleId", { type: "manual", message: "RoleId không tồn tại." })
                    }
                })
                // Nếu có thông báo lỗi khác không liên quan đến các trường cụ thể
                const genericErrors = errorMessages?.filter((msg: string) => {
                    return !(
                        msg.includes("Email đã tồn tại") ||
                        msg.includes("Số điện thoại đã tồn tại") ||
                        msg.includes("Mã định danh đã tồn tại") ||
                        msg.includes("RoleId không tồn tại")
                    )
                })
                if (genericErrors.length > 0) {
                    toastNotify(genericErrors.join(", "), "error")
                }
            }
        } catch (error) {
            toastNotify("Đã có lỗi xảy ra", "error")
        } finally {
            setLoading(false)
        }
    }

    if (loading) return <MainLoader />

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.fullName}</label>
                <input {...register("fullName")} className={`form-control ${errors.fullName ? "is-invalid" : ""}`} />
                {errors.fullName && <div className="invalid-feedback">{errors.fullName.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.dateOfBirth}</label>
                <input
                    {...register("dateOfBirth")}
                    type="date"
                    className={`form-control ${errors.dateOfBirth ? "is-invalid" : ""}`}
                />
                {errors.dateOfBirth && <div className="invalid-feedback">{errors.dateOfBirth.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.phoneNumber}</label>
                <input
                    {...register("phoneNumber")}
                    className={`form-control ${errors.phoneNumber ? "is-invalid" : ""}`}
                />
                {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.nationalId}</label>
                <input
                    {...register("nationalId")}
                    className={`form-control ${errors.nationalId ? "is-invalid" : ""}`}
                />
                {errors.nationalId && <div className="invalid-feedback">{errors.nationalId.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.email}</label>
                <input
                    {...register("email")}
                    type="email"
                    className={`form-control ${errors.email ? "is-invalid" : ""}`}
                />
                {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">{fieldLabels.gender}</label>
                <select
                    {...register("gender", { valueAsNumber: true })}
                    className={`form-select ${errors.gender ? "is-invalid" : ""}`}
                >
                    <option value={0}>Nam</option>
                    <option value={1}>Nữ</option>
                </select>
                {errors.gender && <div className="invalid-feedback">{errors.gender.message}</div>}
            </div>

            <div className="col-md-4">
                <label className="form-label fw-medium">Vai Trò</label>
                <select {...register("roleId")} className={`form-select ${errors.roleId ? "is-invalid" : ""}`}>
                    <option value="">-- Chọn Vai Trò --</option>
                    {allowedRoles.map((role) => (
                        <option key={role.id} value={role.id}>
                            {role.name.charAt(0).toUpperCase() + role.name.slice(1).replace("_", " ")}
                        </option>
                    ))}
                </select>
                {errors.roleId && <div className="invalid-feedback">{errors.roleId.message}</div>}
            </div>

            <div className="col-md-8">
                <label className="form-label fw-medium">{fieldLabels.address}</label>
                <input {...register("address")} className={`form-control ${errors.address ? "is-invalid" : ""}`} />
                {errors.address && <div className="invalid-feedback">{errors.address.message}</div>}
            </div>

            <div className="col-12 text-end">
                <button type="button" className="btn btn-secondary me-3" onClick={onClose}>
                    Hủy
                </button>
                <button type="submit" className="btn btn-primary">
                    Tạo Người Dùng
                </button>
            </div>
        </form>
    )
}

export default CreateUser
