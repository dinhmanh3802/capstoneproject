// src/pages/user/UserList.tsx

import React, { useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import { SD_Gender, SD_Role, SD_UserStatus } from "../../../utility/SD"
import { useDispatch, useSelector } from "react-redux"
import { setCurrentUser } from "../../../store/slice/userSlice"
import { apiResponse, userModel } from "../../../interfaces"
import { useNavigate } from "react-router-dom"
import Select from "react-select"
import { useChangeUserStatusMutation, useResetUserPasswordMutation } from "../../../apis/userApi"
import { Button, Modal, Tooltip, OverlayTrigger } from "react-bootstrap"
import { inputHelper, toastNotify } from "../../../helper"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import { RootState } from "../../../store/store"

interface User extends userModel {
    id: number
    fullName: string
    phoneNumber: string
    email: string
    status: SD_UserStatus
    gender: SD_Gender
}

const statusOptions: { value: SD_UserStatus; label: string }[] = [
    { value: SD_UserStatus.ACTIVE, label: "Hoạt động" },
    { value: SD_UserStatus.DEACTIVE, label: "Không hoạt động" },
]

const getStatusColor = (status: SD_UserStatus): string => {
    switch (status) {
        case SD_UserStatus.ACTIVE:
            return "success" // Màu xanh lá (text-success)
        case SD_UserStatus.DEACTIVE:
            return "danger" // Màu đỏ (text-danger)
        default:
            return "secondary" // Màu xám (text-secondary)
    }
}

const customSelectStyles = (status: SD_UserStatus) => ({
    singleValue: (provided: any) => ({
        ...provided,
        color: `var(--bs-${getStatusColor(status)})`,
    }),
    option: (provided: any, state: any) => ({
        ...provided,
        color: `var(--bs-${getStatusColor(state.data.value)})`,
    }),
})

function UserList({ users = [] }: { users: User[] }) {
    const [currentPage, setCurrentPage] = useState(1)
    const [perPage, setPerPage] = useState(10)

    const [showResetModal, setShowResetModal] = useState(false)
    const [selectedUser, setSelectedUser] = useState<User | null>(null)
    const [resetPasswordInput, setResetPasswordInput] = useState({
        newPassword: "",
        confirmPassword: "",
    })
    const [resetPasswordErrors, setResetPasswordErrors] = useState({
        newPassword: "",
        confirmPassword: "",
    })
    const [resetUserPassword] = useResetUserPasswordMutation()

    const dispatch = useDispatch()
    const navigate = useNavigate()
    const [changeUserStatus] = useChangeUserStatusMutation()
    const [selectedRows, setSelectedRows] = useState<User[]>([])
    const [isConfirmOpen, setIsConfirmOpen] = useState(false)
    const [confirmMessage, setConfirmMessage] = useState("")
    const [confirmAction, setConfirmAction] = useState<() => void>(() => {})

    // Lấy người dùng hiện tại từ userSlice
    const currentUser = useSelector((state: RootState) => state.auth?.user)

    // Hàm xử lý truy cập chi tiết người dùng
    const handleAccess = (row: userModel) => {
        dispatch(setCurrentUser(row))
        navigate(`/user/${row.id}`)
    }

    // Hàm mở modal đặt lại mật khẩu
    const handleResetPassword = (user: User) => {
        setSelectedUser(user)
        setShowResetModal(true)
    }

    // Hàm xử lý nhập liệu đặt lại mật khẩu
    const handleResetPasswordInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        const tempData = inputHelper(e, resetPasswordInput)
        setResetPasswordInput(tempData)
        setResetPasswordErrors({ newPassword: "", confirmPassword: "" })
    }

    // Hàm kiểm tra mật khẩu
    const validatePassword = (password: string) => {
        const minLength = 8
        const hasUpperCase = /[A-Z]/.test(password)
        const hasLowerCase = /[a-z]/.test(password)
        const hasNumbers = /\d/.test(password)
        const hasSpecialChars = /[!@#$%^&*(),.?":{}|<>]/.test(password)

        if (password.length < minLength) {
            return "Mật khẩu phải có ít nhất 8 ký tự."
        }
        if (!hasUpperCase) {
            return "Mật khẩu phải có ít nhất 1 chữ hoa."
        }
        if (!hasLowerCase) {
            return "Mật khẩu phải có ít nhất 1 chữ thường."
        }
        if (!hasNumbers) {
            return "Mật khẩu phải có ít nhất 1 số."
        }
        if (!hasSpecialChars) {
            return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt."
        }
        return ""
    }

    // Hàm xác nhận đặt lại mật khẩu
    const confirmResetPassword = async () => {
        // Kiểm tra xác nhận mật khẩu
        if (resetPasswordInput.newPassword !== resetPasswordInput.confirmPassword) {
            setResetPasswordErrors((prev) => ({
                ...prev,
                confirmPassword: "Mật khẩu xác nhận không khớp.",
            }))
            return
        }

        // Kiểm tra chất lượng mật khẩu
        const passwordError = validatePassword(resetPasswordInput.newPassword)
        if (passwordError) {
            setResetPasswordErrors((prev) => ({
                ...prev,
                newPassword: passwordError,
            }))
            return
        }

        if (selectedUser) {
            try {
                const response: apiResponse = await resetUserPassword({
                    userId: selectedUser.id,
                    newPassword: resetPasswordInput.newPassword,
                })

                if (response.data?.isSuccess) {
                    toastNotify(`Đặt lại mật khẩu cho ${selectedUser.fullName} thành công!`, "success")
                    handleCloseResetModal()
                } else {
                    throw new Error(response.error?.errorMessages[0] || "Có lỗi xảy ra.")
                }
            } catch (error: any) {
                toastNotify(`Có lỗi xảy ra: ${error.message}`, "error")
            }
        }
    }

    // Hàm đóng modal đặt lại mật khẩu
    const handleCloseResetModal = () => {
        setShowResetModal(false)
        setSelectedUser(null)
        setResetPasswordInput({ newPassword: "", confirmPassword: "" })
        setResetPasswordErrors({ newPassword: "", confirmPassword: "" })
    }

    // Hàm xử lý thay đổi trạng thái người dùng
    const handleStatusChange = (row: User, newStatus: SD_UserStatus) => {
        // Kiểm tra nếu người dùng đang cố gắng thay đổi trạng thái của chính mình
        if (currentUser?.userId == row.id) {
            toastNotify("Bạn không thể thay đổi trạng thái của chính mình.", "warning")
            return
        }

        if (row.roleId == SD_Role.ADMIN) {
            toastNotify("Không thể thay đổi trạng thái của người dùng Admin.", "warning")
            return
        }

        const isSelected = selectedRows.some((selectedRow) => selectedRow.id === row.id)
        if (isSelected && selectedRows.length > 0) {
            // Thay đổi trạng thái cho các hàng được chọn, loại bỏ Admin và chính mình
            const userIds = selectedRows
                ?.filter((selectedRow) => selectedRow.roleId !== SD_Role.ADMIN && selectedRow.id != currentUser?.userId)
                ?.map((selectedRow) => selectedRow.id)

            if (userIds.length === 0) {
                toastNotify("Không có người dùng hợp lệ để thay đổi trạng thái.", "warning")
                return
            }

            const action = () => {
                changeUserStatusBulk(userIds, newStatus)
            }
            setConfirmMessage(
                `Bạn có chắc chắn muốn thay đổi trạng thái cho ${userIds.length} người dùng đã chọn sang "${
                    statusOptions?.find((option) => option.value === newStatus)?.label
                }"?`,
            )
            setConfirmAction(() => action)
            setIsConfirmOpen(true)
        } else {
            // Thay đổi trạng thái cho một hàng duy nhất
            const action = () => {
                changeUserStatusSingle(row, newStatus)
            }
            setConfirmMessage(
                `Bạn có chắc chắn muốn thay đổi trạng thái cho "${row.fullName}" sang "${
                    statusOptions?.find((option) => option.value === newStatus)?.label
                }"?`,
            )
            setConfirmAction(() => action)
            setIsConfirmOpen(true)
        }
    }

    // Hàm thay đổi trạng thái cho một người dùng duy nhất
    const changeUserStatusSingle = async (user: User, newStatus: SD_UserStatus) => {
        try {
            const response: apiResponse = await changeUserStatus({
                userIds: [user.id],
                newStatus,
            })

            if (!response.data?.isSuccess) {
                throw new Error(`Cập nhật trạng thái cho ${user.fullName} thất bại.`)
            } else {
                toastNotify(`Cập nhật trạng thái cho ${user.fullName} thành công!`, "success")
            }
        } catch (error: any) {
            toastNotify(`Có lỗi xảy ra: ${error.message}`, "error")
        }
    }

    // Hàm thay đổi trạng thái cho nhiều người dùng
    const changeUserStatusBulk = async (userIds: number[], newStatus: SD_UserStatus) => {
        try {
            const response: apiResponse = await changeUserStatus({
                userIds,
                newStatus,
            })

            if (response.data?.isSuccess) {
                toastNotify("Cập nhật trạng thái thành công cho các người dùng đã chọn!", "success")
            } else {
                throw new Error("Cập nhật trạng thái hàng loạt thất bại.")
            }
        } catch (error: any) {
            toastNotify(`Có lỗi xảy ra: ${error.message}`, "error")
        }
    }

    // Hàm xử lý thay đổi các hàng được chọn
    const handleSelectedRowsChange = (state: any) => {
        setSelectedRows(state.selectedRows)
    }

    // Hàm xử lý thay đổi trang
    const handlePageChange = (page: number) => {
        setCurrentPage(page)
    }

    // Hàm xử lý thay đổi số hàng mỗi trang
    const handlePerRowsChange = (newPerPage: number, page: number) => {
        setPerPage(newPerPage)
        setCurrentPage(page)
    }

    const customStylesTable = {
        headCells: {
            style: {
                fontSize: "15px",
                fontWeight: "bold",
            },
        },
        rows: {
            style: {
                fontSize: "15px",
            },
        },
    }

    const columns: TableColumn<User>[] = [
        // Cột STT
        {
            name: "#",
            width: "60px",
            cell: (_row, index) => (currentPage - 1) * perPage + index + 1, // Tính số thứ tự
            ignoreRowClick: true,
            allowOverflow: true,
            button: false,
        },
        // Cột Họ và tên
        {
            name: "Họ và tên",
            selector: (row) => row.fullName || "", // Cung cấp giá trị mặc định
            sortable: true,
            cell: (row) => (
                <button
                    className="btn btn-link p-0"
                    onClick={() => handleAccess(row)}
                    style={{ textDecoration: "none", color: "inherit" }}
                >
                    <span data-bs-toggle="tooltip" data-bs-placement="top" title={row.fullName || "N/A"}>
                        {(row.fullName?.length || 0) > 50
                            ? `${row.fullName.substring(0, 47)}...`
                            : row.fullName || "N/A"}
                    </span>
                </button>
            ),
        },

        // Cột Số điện thoại
        {
            name: "Số điện thoại",
            selector: (row) => row.phoneNumber || "",
            sortable: true,
            cell: (row) => row.phoneNumber || "N/A",
        },
        // Cột Vai trò
        {
            name: "Vai trò",
            selector: (row) => row.roleId,
            sortable: true,
            cell: (row: User) => {
                switch (row.roleId) {
                    case SD_Role.ADMIN:
                        return "Quản trị viên"
                    case SD_Role.MANAGER:
                        return "Quản lý"
                    case SD_Role.SECRETARY:
                        return "Thư ký"
                    case SD_Role.STAFF:
                        return "Nhân viên"
                    case SD_Role.SUPERVISOR:
                        return "Huynh trưởng"
                    case SD_Role.TEAM_LEADER:
                        return "Trưởng ban"
                    default:
                        return "Không xác định"
                }
            },
        },
        // Cột Giới tính
        {
            name: "Giới tính",
            selector: (row) => row.gender,
            sortable: true,
            cell: (row: User) => {
                switch (row.gender) {
                    case SD_Gender.Male:
                        return "Nam"
                    case SD_Gender.Female:
                        return "Nữ"
                    default:
                        return "Không xác định"
                }
            },
        },
        // Cột Trạng thái
        {
            name: "Trạng thái",
            minWidth: "13rem",
            selector: (row) => row.status,
            sortable: true,
            cell: (row: User) => (
                <Select
                    options={statusOptions}
                    className="w-100"
                    value={statusOptions?.find((option) => option.value === row.status)}
                    onChange={(selectedOption) => {
                        if (selectedOption && row.roleId !== SD_Role.ADMIN && currentUser?.userId != row.id) {
                            handleStatusChange(row, selectedOption.value as SD_UserStatus)
                        }
                    }}
                    isClearable={false}
                    menuPortalTarget={document.body}
                    menuPosition="fixed"
                    styles={customSelectStyles(row.status)}
                    isDisabled={row.roleId === SD_Role.ADMIN || currentUser?.userId == row.id} // Disable Select nếu là Admin hoặc chính mình
                />
            ),
        },
        // Cột Thao tác
        {
            name: "Thao tác",
            cell: (row) => (
                <div className="d-flex justify-content-around">
                    <OverlayTrigger
                        placement="top"
                        overlay={<Tooltip id={`tooltip-view-${row.id}`}>Xem chi tiết</Tooltip>}
                    >
                        <button className="btn btn-outline-primary btn-sm m-1" onClick={() => handleAccess(row)}>
                            <i className="bi bi-eye"></i>
                        </button>
                    </OverlayTrigger>
                    <OverlayTrigger
                        placement="top"
                        overlay={<Tooltip id={`tooltip-reset-${row.id}`}>Đặt lại mật khẩu</Tooltip>}
                    >
                        <button
                            className="btn btn-outline-warning btn-sm m-1"
                            onClick={() => handleResetPassword(row)}
                            disabled={currentUser?.userId == row.id} // Disable nếu là chính mình
                        >
                            <i className="bi bi-unlock"></i>
                        </button>
                    </OverlayTrigger>
                </div>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
        },
    ]

    return (
        <div className="card">
            <div className="card-body">
                <DataTable
                    columns={columns}
                    data={users}
                    customStyles={customStylesTable}
                    pagination
                    paginationTotalRows={users.length}
                    onChangePage={handlePageChange}
                    onChangeRowsPerPage={handlePerRowsChange}
                    noDataComponent={error.NoData}
                    selectableRows
                    onSelectedRowsChange={handleSelectedRowsChange}
                    selectableRowsHighlight
                    selectableRowsSingle={false}
                />

                {/* Modal đặt lại mật khẩu */}
                <Modal show={showResetModal} onHide={handleCloseResetModal}>
                    <Modal.Header closeButton>
                        <Modal.Title>Đặt lại mật khẩu cho {selectedUser?.fullName || "Người dùng"}</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <div className="mb-3">
                            <label htmlFor="newPassword" className="form-label">
                                Mật khẩu mới
                            </label>
                            <input
                                type="password"
                                className={`form-control ${resetPasswordErrors.newPassword ? "is-invalid" : ""}`}
                                id="newPassword"
                                name="newPassword"
                                value={resetPasswordInput.newPassword}
                                onChange={handleResetPasswordInput}
                            />
                            {resetPasswordErrors.newPassword && (
                                <div className="invalid-feedback">{resetPasswordErrors.newPassword}</div>
                            )}
                        </div>
                        <div className="mb-3">
                            <label htmlFor="confirmPassword" className="form-label">
                                Xác nhận mật khẩu mới
                            </label>
                            <input
                                type="password"
                                className={`form-control ${resetPasswordErrors.confirmPassword ? "is-invalid" : ""}`}
                                id="confirmPassword"
                                name="confirmPassword"
                                value={resetPasswordInput.confirmPassword}
                                onChange={handleResetPasswordInput}
                            />
                            {resetPasswordErrors.confirmPassword && (
                                <div className="invalid-feedback">{resetPasswordErrors.confirmPassword}</div>
                            )}
                        </div>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={handleCloseResetModal}>
                            Quay lại
                        </Button>
                        <Button variant="primary" onClick={confirmResetPassword}>
                            Xác nhận
                        </Button>
                    </Modal.Footer>
                </Modal>

                {/* Confirmation Popup */}
                <ConfirmationPopup
                    isOpen={isConfirmOpen}
                    onClose={() => setIsConfirmOpen(false)}
                    onConfirm={() => {
                        confirmAction()
                        setIsConfirmOpen(false)
                    }}
                    message={confirmMessage}
                    title="Xác nhận thay đổi trạng thái"
                />
            </div>
        </div>
    )
}

export default UserList
