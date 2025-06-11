// VolunteerDetail.tsx
import React, { useState, useEffect } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
    useGetVolunteerCourseByVolunteerIdAndCourseIdQuery,
    useUpdateVolunteerInformationInACourseMutation,
} from "../../apis/volunteerApplicationApi"
import { useGetTeamsByCourseIdQuery } from "../../apis/teamApi"
import { useUpdateVolunteerMutation } from "../../apis/volunteerApi"
import { MainLoader } from "../../components/Page"

// Import các thư viện mới
import { useForm, SubmitHandler } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { toast } from "react-toastify"
import { SD_EmployeeProcessStatus_Name, SD_ProcessStatus, SD_Role_Name } from "../../utility/SD"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
// Định nghĩa các trạng thái và tên trạng thái (nếu chưa định nghĩa ở nơi khác)

const getVolunteerStatusName = (status: SD_ProcessStatus) => {
    return status === SD_ProcessStatus.Approved
        ? SD_EmployeeProcessStatus_Name.WaitingForEnroll
        : status === SD_ProcessStatus.Enrolled
        ? SD_EmployeeProcessStatus_Name.Enrolled
        : status === SD_ProcessStatus.Graduated
        ? SD_EmployeeProcessStatus_Name.Graduated
        : SD_EmployeeProcessStatus_Name.DropOut
}

// Hàm để lấy các tùy chọn trạng thái ứng dụng
const getApplicationStatusOptions = () => {
    const allowedStatuses = [
        SD_ProcessStatus.Approved,
        SD_ProcessStatus.Enrolled,
        SD_ProcessStatus.Graduated,
        SD_ProcessStatus.DropOut,
    ]

    return allowedStatuses?.map((status) => ({
        value: status,
        label:
            status === SD_ProcessStatus.Approved
                ? SD_EmployeeProcessStatus_Name.WaitingForEnroll
                : status === SD_ProcessStatus.Enrolled
                ? SD_EmployeeProcessStatus_Name.Enrolled
                : status === SD_ProcessStatus.Graduated
                ? SD_EmployeeProcessStatus_Name.Graduated
                : SD_EmployeeProcessStatus_Name.DropOut,
    }))
}
// Định nghĩa schema Zod cho thông tin chung
const generalInfoSchema = z.object({
    fullName: z
        .string()
        .trim()
        .min(1, { message: "Họ và tên là bắt buộc" })
        .max(100, { message: "Họ và tên quá dài" })
        .regex(/^[A-Za-zÀ-Ỹà-ỹ\s]+$/, { message: "Họ và tên không hợp lệ" })
        .refine((value) => !/\s{2,}/.test(value), { message: "Không được chứa nhiều khoảng trắng liên tiếp" })
        .refine((value) => value.trim() === value, { message: "Không được có khoảng trắng đầu hoặc cuối" }),
    gender: z.enum(["0", "1"], { errorMap: () => ({ message: "Giới tính là bắt buộc" }) }),
    dateOfBirth: z
        .string()
        .min(1, { message: "Ngày sinh là bắt buộc" })
        .refine(
            (date) => {
                const parsedDate = Date.parse(date)
                if (isNaN(parsedDate)) return false
                const birthDate = new Date(parsedDate)
                const today = new Date()
                const age = today.getFullYear() - birthDate.getFullYear()
                const isEligible =
                    age > 18 || (age === 18 && today >= new Date(birthDate.setFullYear(birthDate.getFullYear() + 18)))
                return isEligible
            },
            { message: "Tình nguyện viên phải trên 18 tuổi" },
        ),
    nationalId: z.string().regex(/^(\d{9}|\d{12})$/, { message: "Số CCCD phải là 9 hoặc 12 chữ số" }),
    email: z.string().email({ message: "Email không hợp lệ" }),
    phoneNumber: z
        .string()
        .regex(/^(0[3|5|7|8|9])+([0-9]{8})$/, { message: "Số điện thoại không hợp lệ" })
        .min(10, { message: "Số điện thoại phải có 10 chữ số" })
        .max(10, { message: "Số điện thoại phải có 10 chữ số" }),
    address: z.string().min(1, { message: "Địa chỉ là bắt buộc" }),
    note: z.string().optional(),
})

// Định nghĩa schema Zod cho thông tin khóa
const courseInfoSchema = z.object({
    teamId: z.string().trim().min(1, { message: "Ban là bắt buộc" }),
    status: z.enum(["1", "2", "3", "4"], { errorMap: () => ({ message: "Trạng thái là bắt buộc" }) }),
    note: z.string().optional(),
})

type GeneralInfoFormData = z.infer<typeof generalInfoSchema>
type CourseInfoFormData = z.infer<typeof courseInfoSchema>

const VolunteerDetail = () => {
    const navigate = useNavigate()
    const { courseId, volunteerId } = useParams()
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    // Destructure refetch từ hook query
    const {
        data: volunteerCourseData,
        isLoading: volunteerLoading,
        refetch,
    } = useGetVolunteerCourseByVolunteerIdAndCourseIdQuery({
        volunteerId,
        courseId,
    })
    const { data: teamData, isLoading: teamLoading } = useGetTeamsByCourseIdQuery(Number(courseId))
    const [updateVolunteer] = useUpdateVolunteerMutation()
    const [updateVolunteerInCourse] = useUpdateVolunteerInformationInACourseMutation()

    const volunteerApplication = volunteerCourseData?.result
    const [isEditingGeneral, setIsEditingGeneral] = useState(false)
    const [isEditingCourse, setIsEditingCourse] = useState(false)
    const [currentStatus, setCurrentStatus] = useState(volunteerCourseData?.result.status)
    // Xử lý hình ảnh
    const [imageFiles, setImageFiles] = useState({
        Image: null as File | null,
        nationalImageFront: null as File | null,
        nationalImageBack: null as File | null,
    })
    const [imagePreviews, setImagePreviews] = useState({
        Image: null as string | null,
        nationalImageFront: null as string | null,
        nationalImageBack: null as string | null,
    })

    // Khai báo editedCourse trước khi sử dụng
    const [editedCourse, setEditedCourse] = useState({
        teamId: "", // string
        status: SD_ProcessStatus.Approved.toString(),
        note: "",
    })

    // Sử dụng React Hook Form cho thông tin chung với giá trị mặc định an toàn
    const {
        register: registerGeneral,
        handleSubmit: handleSubmitGeneral,
        formState: { errors: errorsGeneral },
        reset: resetGeneral,
    } = useForm<GeneralInfoFormData>({
        resolver: zodResolver(generalInfoSchema),
        defaultValues: {
            fullName: "",
            gender: "0",
            dateOfBirth: "",
            nationalId: "",
            email: "",
            phoneNumber: "",
            address: "",
            note: "",
        },
    })
    const handleStatusChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
        setCurrentStatus(event.target.value)
    }

    // Sử dụng React Hook Form cho thông tin khóa với giá trị mặc định an toàn
    const {
        register: registerCourse,
        handleSubmit: handleSubmitCourse,
        formState: { errors: errorsCourse },
        reset: resetCourse,
    } = useForm<CourseInfoFormData>({
        resolver: zodResolver(courseInfoSchema),
        defaultValues: {
            teamId: "",
            status: "1",
            note: "",
        },
    })

    // Dọn dẹp các URL tạo ra để tránh rò rỉ bộ nhớ
    useEffect(() => {
        return () => {
            Object.values(imagePreviews).forEach((url) => {
                if (url) {
                    URL.revokeObjectURL(url)
                }
            })
        }
    }, [imagePreviews])

    // Cập nhật các giá trị form khi dữ liệu từ API được tải về
    useEffect(() => {
        if (volunteerApplication) {
            const { volunteer, status, note } = volunteerApplication
            const teamId = volunteer.teams?.[0]?.id?.toString() ?? ""
            const statusString = status?.toString() ?? SD_ProcessStatus.Approved.toString()
            const noteValue = note ?? ""

            // Cập nhật form thông tin chung
            resetGeneral({
                fullName: volunteer.fullName ?? "",
                gender: volunteer.gender !== undefined ? volunteer.gender.toString() : "0",
                dateOfBirth: volunteer.dateOfBirth ? new Date(volunteer.dateOfBirth).toISOString().split("T")[0] : "",
                nationalId: volunteer.nationalId ?? "",
                email: volunteer.email ?? "",
                phoneNumber: volunteer.phoneNumber ?? "",
                address: volunteer.address ?? "",
                note: volunteer.note ?? "",
            })

            // Cập nhật form thông tin khóa
            resetCourse({
                teamId: teamId,
                status: statusString,
                note: noteValue,
            })

            // Cập nhật editedCourse state
            setEditedCourse({
                teamId: teamId,
                status: statusString,
                note: noteValue,
            })
        }
    }, [volunteerApplication, resetGeneral, resetCourse])

    if (volunteerLoading || teamLoading) {
        return <MainLoader />
    }

    if (!volunteerApplication) {
        return <div className="text-center">Không tìm thấy thông tin tình nguyện viên.</div>
    }

    const { volunteer, course } = volunteerApplication

    // Xử lý thay đổi hình ảnh
    const handleImageChange = (field: keyof typeof imageFiles, file: File | null) => {
        setImageFiles({ ...imageFiles, [field]: file })

        // Tạo URL tạm thời để xem trước hình ảnh
        if (file) {
            const previewUrl = URL.createObjectURL(file)
            setImagePreviews((prev) => ({
                ...prev,
                [field]: previewUrl,
            }))
        } else {
            setImagePreviews((prev) => ({
                ...prev,
                [field]: null,
            }))
        }
    }

    // Xử lý lưu thông tin chung
    const onSubmitGeneral: SubmitHandler<GeneralInfoFormData> = async (data) => {
        try {
            const formData = new FormData()
            for (const key in data) {
                const value = data[key as keyof GeneralInfoFormData]
                if (value !== undefined && value !== null) {
                    formData.append(key, value.toString())
                }
            }
            if (imageFiles.Image) {
                formData.append("Image", imageFiles.Image)
            }
            if (imageFiles.nationalImageFront) {
                formData.append("nationalImageFront", imageFiles.nationalImageFront)
            }
            if (imageFiles.nationalImageBack) {
                formData.append("nationalImageBack", imageFiles.nationalImageBack)
            }

            await updateVolunteer({ volunteerId, volunteerData: formData }).unwrap()
            setIsEditingGeneral(false)
            toast.success("Cập nhật thông tin chung thành công!")
            refetch()
        } catch (error: any) {
            console.error("Error updating volunteer:", error)
            toast.error(error.data.errorMessages[0])
        }
    }

    // Xử lý lưu thông tin khóa
    const onSubmitCourse: SubmitHandler<CourseInfoFormData> = async (data) => {
        try {
            const courseData = {
                teamId: parseInt(data.teamId, 10),
                status: parseInt(data.status, 10),
                note: data.note,
            }

            await updateVolunteerInCourse({
                volunteerId,
                courseId,
                teamId: courseData.teamId,
                status: courseData.status,
                note: courseData.note,
            }).unwrap()

            setIsEditingCourse(false)
            toast.success("Cập nhật thông tin khóa thành công!")
            refetch()
        } catch (error: any) {
            console.error("Error updating course info:", error)
            toast.error("Cập nhật thông tin khóa thất bại. Vui lòng thử lại.")
        }
    }

    const getVolunteerStatusNameDisplay = (status: SD_ProcessStatus) => {
        return getVolunteerStatusName(status)
    }

    return (
        <div className="container">
            <h3 className="fw-bold">Thông tin chi tiết tình nguyện viên</h3>

            {/* Thông tin chung */}
            <div className="card mb-4">
                <div className="card-header d-flex justify-content-between align-items-center">
                    <span>Thông tin chung</span>
                </div>
                <div className="card-body">
                    <form onSubmit={handleSubmitGeneral(onSubmitGeneral)}>
                        <div className="row g-3">
                            <div className="row col-md-9 g-3">
                                {/* Họ và tên */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Họ và tên</label>
                                    <input
                                        type="text"
                                        className={`form-control ${errorsGeneral.fullName ? "is-invalid" : ""}`}
                                        {...registerGeneral("fullName")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.fullName && (
                                        <div className="invalid-feedback">{errorsGeneral.fullName.message}</div>
                                    )}
                                </div>

                                {/* Giới tính */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Giới tính</label>
                                    <select
                                        className={`form-control ${errorsGeneral.gender ? "is-invalid" : ""}`}
                                        {...registerGeneral("gender")}
                                        disabled={!isEditingGeneral}
                                    >
                                        <option value="">Chọn giới tính</option>
                                        <option value="0">Nam</option>
                                        <option value="1">Nữ</option>
                                    </select>
                                    {errorsGeneral.gender && (
                                        <div className="invalid-feedback">{errorsGeneral.gender.message}</div>
                                    )}
                                </div>

                                {/* Ngày sinh */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Ngày sinh</label>
                                    <input
                                        type="date"
                                        className={`form-control ${errorsGeneral.dateOfBirth ? "is-invalid" : ""}`}
                                        {...registerGeneral("dateOfBirth")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.dateOfBirth && (
                                        <div className="invalid-feedback">{errorsGeneral.dateOfBirth.message}</div>
                                    )}
                                </div>

                                {/* Số CCCD */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Số CCCD</label>
                                    <input
                                        type="text"
                                        className={`form-control ${errorsGeneral.nationalId ? "is-invalid" : ""}`}
                                        {...registerGeneral("nationalId")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.nationalId && (
                                        <div className="invalid-feedback">{errorsGeneral.nationalId.message}</div>
                                    )}
                                </div>

                                {/* Email */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Email</label>
                                    <input
                                        type="email"
                                        className={`form-control ${errorsGeneral.email ? "is-invalid" : ""}`}
                                        {...registerGeneral("email")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.email && (
                                        <div className="invalid-feedback">{errorsGeneral.email.message}</div>
                                    )}
                                </div>

                                {/* Điện thoại */}
                                <div className="col-md-4">
                                    <label className="form-label fw-medium">Điện thoại</label>
                                    <input
                                        type="text"
                                        className={`form-control ${errorsGeneral.phoneNumber ? "is-invalid" : ""}`}
                                        {...registerGeneral("phoneNumber")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.phoneNumber && (
                                        <div className="invalid-feedback">{errorsGeneral.phoneNumber.message}</div>
                                    )}
                                </div>

                                {/* Địa chỉ */}
                                <div className="col-md-12">
                                    <label className="form-label fw-medium">Địa chỉ</label>
                                    <input
                                        type="text"
                                        className={`form-control ${errorsGeneral.address ? "is-invalid" : ""}`}
                                        {...registerGeneral("address")}
                                        disabled={!isEditingGeneral}
                                    />
                                    {errorsGeneral.address && (
                                        <div className="invalid-feedback">{errorsGeneral.address.message}</div>
                                    )}
                                </div>

                                {/* Ghi chú */}
                                <div className="col-md-12">
                                    <label className="form-label fw-medium">Ghi chú</label>
                                    <textarea
                                        className="form-control"
                                        {...registerGeneral("note")}
                                        rows={3}
                                        disabled={!isEditingGeneral}
                                    />
                                </div>
                            </div>

                            {/* Ảnh đại diện */}
                            <div className="col-md-3 text-center">
                                <label htmlFor="endDate" className="form-label">
                                    <span style={{ visibility: "hidden" }}>.</span>
                                </label>
                                <img
                                    src={imagePreviews.Image ?? volunteer?.image ?? ""}
                                    alt="Profile"
                                    className="img-fluid mb-2 mt-4"
                                    style={{ maxHeight: "16rem", objectFit: "cover" }}
                                />
                                {isEditingGeneral && (
                                    <input
                                        type="file"
                                        accept="image/jpeg, image/jpg, image/png"
                                        className="form-control"
                                        onChange={(e) => handleImageChange("Image", e.target.files?.[0] ?? null)}
                                    />
                                )}
                            </div>
                        </div>

                        {/* Ảnh CCCD */}
                        <div className="row mt-3 justify-content-center">
                            {/* Ảnh CCCD mặt trước */}
                            <div className="col-md-6 text-center mb-3">
                                <label className="form-label fw-medium">Ảnh CCCD mặt trước</label>
                                <div className="border rounded p-2 d-flex flex-column align-items-center">
                                    <img
                                        src={imagePreviews.nationalImageFront ?? volunteer?.nationalImageFront ?? ""}
                                        alt="Ảnh CCCD mặt trước"
                                        className="img-fluid mb-2"
                                        style={{ maxHeight: "450px", objectFit: "cover" }}
                                    />
                                    {isEditingGeneral && (
                                        <input
                                            type="file"
                                            accept="image/jpeg, image/jpg, image/png"
                                            className="form-control mt-2"
                                            onChange={(e) =>
                                                handleImageChange("nationalImageFront", e.target.files?.[0] ?? null)
                                            }
                                        />
                                    )}
                                </div>
                            </div>

                            {/* Ảnh CCCD mặt sau */}
                            <div className="col-md-6 text-center mb-3">
                                <label className="form-label fw-medium">Ảnh CCCD mặt sau</label>
                                <div className="border rounded p-2 d-flex flex-column align-items-center">
                                    <img
                                        src={imagePreviews.nationalImageBack ?? volunteer?.nationalImageBack ?? ""}
                                        alt="Ảnh CCCD mặt sau"
                                        className="img-fluid mb-2"
                                        style={{ maxHeight: "450px", objectFit: "cover" }}
                                    />
                                    {isEditingGeneral && (
                                        <input
                                            type="file"
                                            accept="image/jpeg, image/jpg, image/png"
                                            className="form-control mt-2"
                                            onChange={(e) =>
                                                handleImageChange("nationalImageBack", e.target.files?.[0] ?? null)
                                            }
                                        />
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Nút lưu thông tin chung */}
                        {isEditingGeneral && (
                            <div className="mt-3 text-end">
                                <button type="submit" className="btn btn-primary me-2">
                                    Lưu
                                </button>
                                <button
                                    type="button"
                                    className="btn btn-secondary"
                                    onClick={() => {
                                        setIsEditingGeneral(false)
                                        resetGeneral()
                                        setImageFiles({
                                            Image: null,
                                            nationalImageFront: null,
                                            nationalImageBack: null,
                                        })
                                        setImagePreviews({
                                            Image: null,
                                            nationalImageFront: null,
                                            nationalImageBack: null,
                                        })
                                    }}
                                >
                                    Hủy
                                </button>
                            </div>
                        )}
                        {!isEditingGeneral &&
                            (currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER) && (
                                <div className="text-end mt-2">
                                    <button
                                        type="button"
                                        className="btn btn-warning"
                                        onClick={() => {
                                            setIsEditingGeneral(true)
                                            resetGeneral()
                                            setImageFiles({
                                                Image: null,
                                                nationalImageFront: null,
                                                nationalImageBack: null,
                                            })
                                            setImagePreviews({
                                                Image: null,
                                                nationalImageFront: null,
                                                nationalImageBack: null,
                                            })
                                        }}
                                    >
                                        Sửa
                                    </button>
                                </div>
                            )}
                    </form>
                </div>
            </div>

            {/* Thông tin khóa */}
            <div className="card mt-4">
                <div className="card-header d-flex justify-content-between align-items-center">
                    <span>Thông tin khóa</span>
                </div>
                <div className="card-body">
                    <form onSubmit={handleSubmitCourse(onSubmitCourse)}>
                        <div className="row g-3">
                            {/* Tên khóa */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Tên khóa</label>
                                <input type="text" className="form-control" value={course?.courseName ?? ""} disabled />
                            </div>

                            {/* Mã tình nguyện viên */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Mã tình nguyện viên</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={volunteerApplication.volunteerCode ?? ""}
                                    disabled
                                />
                            </div>

                            {/* Ban */}
                            {/* Ban */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Ban</label>
                                {isEditingCourse ? (
                                    <>
                                        <select
                                            className={`form-control ${errorsCourse.teamId ? "is-invalid" : ""}`}
                                            {...registerCourse("teamId")}
                                            disabled={
                                                !isEditingCourse ||
                                                currentStatus == SD_ProcessStatus.DropOut ||
                                                currentStatus == SD_ProcessStatus.Graduated
                                            } // Động thay đổi trạng thái
                                            defaultValue={volunteerApplication.volunteer.teams[0]?.id}
                                        >
                                            {teamData?.result
                                                ?.filter(
                                                    (team: any) =>
                                                        team.gender === volunteer.gender || team.gender === null,
                                                )
                                                ?.map((team: any) => (
                                                    <option key={team?.id} value={team?.id}>
                                                        {team.teamName}
                                                    </option>
                                                ))}
                                        </select>
                                        {errorsCourse.teamId && (
                                            <div className="invalid-feedback">{errorsCourse.teamId.message}</div>
                                        )}
                                    </>
                                ) : (
                                    <input
                                        type="text"
                                        className="form-control"
                                        value={volunteerApplication.volunteer.teams?.[0]?.teamName ?? "Chưa phân"}
                                        disabled
                                    />
                                )}
                            </div>

                            {/* Trạng thái */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Trạng thái</label>
                                {isEditingCourse ? (
                                    <>
                                        <select
                                            className={`form-control ${errorsCourse.status ? "is-invalid" : ""}`}
                                            {...registerCourse("status")}
                                            value={currentStatus}
                                            onChange={handleStatusChange} // Ghi nhận thay đổi trạng thái
                                            disabled={!isEditingCourse}
                                        >
                                            {getApplicationStatusOptions().map((option) => (
                                                <option key={option.value} value={option.value}>
                                                    {option.label}
                                                </option>
                                            ))}
                                        </select>
                                        {errorsCourse.status && (
                                            <div className="invalid-feedback">{errorsCourse.status.message}</div>
                                        )}
                                    </>
                                ) : (
                                    <input
                                        type="text"
                                        className="form-control"
                                        value={getVolunteerStatusNameDisplay(volunteerApplication.status)}
                                        disabled
                                    />
                                )}
                            </div>

                            {/* Ghi chú */}
                            <div className="col-md-12">
                                <label className="form-label fw-medium">Ghi chú</label>
                                {isEditingCourse ? (
                                    <>
                                        <textarea
                                            className={`form-control ${errorsCourse.note ? "is-invalid" : ""}`}
                                            {...registerCourse("note")}
                                            rows={3}
                                            disabled={!isEditingCourse}
                                        />
                                        {errorsCourse.note && (
                                            <div className="invalid-feedback">{errorsCourse.note.message}</div>
                                        )}
                                    </>
                                ) : (
                                    <textarea
                                        className="form-control"
                                        value={volunteerApplication.note ?? "Không có ghi chú"}
                                        rows={3}
                                        disabled
                                    />
                                )}
                            </div>

                            {/* Người duyệt */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Người duyệt</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={volunteerApplication.reviewer?.userName ?? "Chưa xác định"}
                                    disabled
                                />
                            </div>

                            {/* Ngày duyệt */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Ngày duyệt</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={
                                        volunteerApplication.reviewDate
                                            ? new Date(volunteerApplication.reviewDate).toLocaleDateString()
                                            : "Chưa duyệt"
                                    }
                                    disabled
                                />
                            </div>

                            {/* Ngày nộp đơn */}
                            <div className="col-md-3">
                                <label className="form-label fw-medium">Ngày nộp đơn</label>
                                <input
                                    type="text"
                                    className="form-control"
                                    value={
                                        volunteerApplication.applicationDate
                                            ? new Date(volunteerApplication.applicationDate).toLocaleDateString()
                                            : ""
                                    }
                                    disabled
                                />
                            </div>
                            <div className="col-md-3">
                                <label className="form-label fw-medium"></label>
                                {/* Nút lưu thông tin khóa */}
                                {isEditingCourse && (
                                    <div className="text-end mt-2">
                                        <button type="submit" className="btn btn-primary me-2">
                                            Lưu
                                        </button>
                                        <button
                                            type="button"
                                            className="btn btn-secondary"
                                            onClick={() => {
                                                setIsEditingCourse(false)
                                                resetCourse()
                                            }}
                                        >
                                            Hủy
                                        </button>
                                    </div>
                                )}
                                {!isEditingCourse &&
                                    (currentUserRole == SD_Role_Name.SECRETARY ||
                                        currentUserRole == SD_Role_Name.MANAGER) && (
                                        <div className="text-end mt-2">
                                            <button
                                                type="button"
                                                className="btn btn-warning"
                                                onClick={() => {
                                                    setIsEditingCourse(true)
                                                    resetCourse()
                                                }}
                                            >
                                                Sửa
                                            </button>
                                        </div>
                                    )}
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            {/* Nút quay lại */}
            <div className="row mt-4">
                <div className="col-12 text-start">
                    <button className="btn btn-secondary" onClick={() => navigate(-1)}>
                        Quay lại
                    </button>
                </div>
            </div>
        </div>
    )
}

export default VolunteerDetail
