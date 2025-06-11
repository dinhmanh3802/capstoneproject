// src/components/Page/Supervisor/SupervisorList.tsx

import React, { useState, useEffect } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import { SD_Gender, SD_UserStatus, SD_Role, SD_Role_Name } from "../../../utility/SD"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import { useNavigate } from "react-router-dom"
import Select from "react-select"

import { Tooltip, OverlayTrigger } from "react-bootstrap"
import { toastNotify } from "../../../helper"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import { supervisorModel } from "../../../interfaces/supervisorModel"
import { useGetStudentGroupsQuery } from "../../../apis/studentGroupApi"
import { skipToken } from "@reduxjs/toolkit/query"
import { useChangeSupervisorsGroupMutation } from "../../../apis/supervisorApi"
import { useChangeUserRoleMutation } from "../../../apis/userApi"

interface Supervisor extends supervisorModel {
    id: number
    fullName: string
    phoneNumber: string
    email: string
    status: SD_UserStatus
    gender: SD_Gender
    group?: {
        groupId: number
        groupName: string
        gender: SD_Gender // Đảm bảo rằng group cũng có trường gender
    }
}

interface SupervisorListProps {
    supervisors: Supervisor[]
    refetch: () => void // Thêm prop refetch
}

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

function SupervisorList({ supervisors = [], refetch }: SupervisorListProps) {
    const [currentPage, setCurrentPage] = useState(1)
    const [perPage, setPerPage] = useState(10)
    const navigate = useNavigate()
    const [changeSupervisorsGroup] = useChangeSupervisorsGroupMutation()
    const [selectedRows, setSelectedRows] = useState<Supervisor[]>([])
    const [isConfirmOpen, setIsConfirmOpen] = useState(false)
    const [confirmMessage, setConfirmMessage] = useState("")
    const [confirmAction, setConfirmAction] = useState<() => void>(() => {})
    const [clearRowsFlag, setClearRowsFlag] = useState(false) // State flag để clear selected rows

    // Lấy thông tin khóa tu hiện tại từ Redux store
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)

    // Lấy thông tin người dùng hiện tại từ Redux store
    const currentUser = useSelector((state: RootState) => state.auth.user) // Giả sử có slice auth

    // Lấy danh sách chánh từ API
    const { data: groupData } = useGetStudentGroupsQuery(currentCourse?.id ? currentCourse.id : skipToken)

    const groupOptions = groupData?.result?.map((group: any) => ({
        value: group.id,
        label: group.groupName,
        gender: group.gender, // Thêm trường gender
    }))

    // Lấy mutation để thay đổi vai trò người dùng
    const [changeUserRole] = useChangeUserRoleMutation()

    // Hàm kiểm tra xem khóa tu đã kết thúc hay chưa
    const isCourseEnded = (): boolean => {
        if (!currentCourse || !currentCourse.endDate) {
            return false
        }
        const now = new Date()
        const courseEndDate = new Date(currentCourse.endDate)

        // Kiểm tra xem endDate có phải là một ngày hợp lệ không
        if (isNaN(courseEndDate.getTime())) {
            console.error("Invalid endDate format:", currentCourse.endDate)
            return false
        }

        return now > courseEndDate
    }

    // Hàm xử lý truy cập chi tiết Supervisor
    const handleAccess = (row: supervisorModel) => {
        navigate(`/supervisor/${row.id}`)
    }

    // Hàm xử lý Remove Supervisor
    const handleRemoveSupervisor = (user: Supervisor) => {
        setConfirmMessage(
            `Bạn có chắc chắn muốn loại bỏ Huynh trưởng "${user.fullName}" và chuyển họ trở lại vai trò "Nhân viên"?`,
        )
        setConfirmAction(() => () => removeSupervisor(user.id))
        setIsConfirmOpen(true)
    }

    // Hàm thực hiện thay đổi vai trò
    const removeSupervisor = async (userId: number) => {
        try {
            await changeUserRole({
                userIds: [userId],
                newRoleId: SD_Role.STAFF, // Thay đổi thành vai trò "Nhân viên"
            }).unwrap()
            toastNotify("Xóa Huynh trưởng thành công!", "success")
            setSelectedRows([]) // Clear selected rows nếu cần
            refetch() // Làm mới danh sách sau khi thay đổi
        } catch (error: any) {
            toastNotify(`Có lỗi xảy ra: ${error.data?.errorMessages?.join(", ") || error.message}`, "error")
        }
    }

    // Hàm xử lý thay đổi nhóm
    const handleGroupChange = (row: Supervisor, newGroupId: number | null) => {
        const supervisorIds = selectedRows.length > 0 ? selectedRows?.map((s) => s.id) : [row.id]

        const action = async () => {
            try {
                await changeSupervisorsGroup({ supervisorIds, newGroupId: newGroupId || 0 }).unwrap()
                toastNotify(`Cập nhật nhóm thành công!`, "success")
                setSelectedRows([]) // Xóa lựa chọn sau khi cập nhật thành công
                refetch() // Làm mới danh sách sau khi thay đổi
            } catch (error: any) {
                toastNotify(`Có lỗi xảy ra: ${error.data?.errorMessages?.join(", ") || error.message}`, "error")
            }
        }

        setConfirmMessage(
            `Bạn có chắc chắn muốn thay đổi nhóm cho ${
                supervisorIds.length > 1 ? `${supervisorIds.length} Huynh trưởng đã chọn` : `"${row.fullName}"`
            }?`,
        )
        setConfirmAction(() => action)
        setIsConfirmOpen(true)
    }

    // Hàm xử lý thay đổi các hàng được chọn
    const handleSelectedRowsChange = (state: any) => {
        setSelectedRows(state.selectedRows)
    }

    const isManagerOrSecretary = (role: string | undefined): boolean => {
        return role == SD_Role_Name.MANAGER || role == SD_Role_Name.SECRETARY
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

    // Reset selectedRows và clearRowsFlag khi currentCourse thay đổi
    useEffect(() => {
        setSelectedRows([])
        setClearRowsFlag(true) // Kích hoạt clearSelectedRows prop
    }, [currentCourse])

    // Reset clearRowsFlag sau khi DataTable đã xóa các dòng đã chọn
    useEffect(() => {
        if (clearRowsFlag) {
            setClearRowsFlag(false)
        }
    }, [clearRowsFlag])

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

    const columns: TableColumn<Supervisor>[] = [
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
            minWidth: "12rem",
            selector: (row) => row.fullName || "", // Cung cấp giá trị mặc định
            sortable: true,
            cell: (row) => (
                <span data-bs-toggle="tooltip" data-bs-placement="top" title={row.fullName || "N/A"}>
                    {(row.fullName?.length || 0) > 50 ? `${row.fullName.substring(0, 47)}...` : row.fullName || "N/A"}
                </span>
            ),
        },
        // Cột Số điện thoại
        {
            name: "Số điện thoại",
            minWidth: "12rem",
            selector: (row) => row.phoneNumber || "",
            sortable: true,
            cell: (row) => row.phoneNumber || "N/A",
        },
        // Cột Email
        {
            name: "Email",
            selector: (row) => row.email || "",
            minWidth: "15rem",
            sortable: true,
            cell: (row) => (
                <span data-bs-toggle="tooltip" data-bs-placement="top" title={row.email || "N/A"}>
                    {(row.email?.length || 0) > 20 ? `${row.email.substring(0, 17)}...` : row.email || "N/A"}
                </span>
            ),
        },
        // Cột Giới tính
        {
            name: "Giới tính",
            minWidth: "9rem",
            selector: (row) => row.gender,
            sortable: true,
            cell: (row: Supervisor) => {
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

        // Cột Thao tác (bao gồm View và Remove)
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
                        overlay={<Tooltip id={`tooltip-remove-${row.id}`}>Loại Huynh trưởng</Tooltip>}
                    >
                        <button
                            className="btn btn-outline-danger btn-sm m-1"
                            onClick={() => handleRemoveSupervisor(row)}
                        >
                            <i className="bi bi-trash"></i>
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
                    data={supervisors}
                    customStyles={customStylesTable}
                    pagination
                    paginationTotalRows={supervisors.length}
                    onChangePage={handlePageChange}
                    onChangeRowsPerPage={handlePerRowsChange}
                    noDataComponent={error.NoData}
                    selectableRows={!isCourseEnded()}
                    onSelectedRowsChange={handleSelectedRowsChange}
                    selectableRowsHighlight
                    selectableRowsSingle={false}
                    clearSelectedRows={clearRowsFlag} // Thêm prop này để clear các dòng đã chọn
                />

                {/* Confirmation Popup */}
                <ConfirmationPopup
                    isOpen={isConfirmOpen}
                    onClose={() => setIsConfirmOpen(false)}
                    onConfirm={() => {
                        confirmAction()
                        setIsConfirmOpen(false)
                    }}
                    message={confirmMessage}
                    title="Xác nhận thay đổi"
                />
            </div>
        </div>
    )
}

export default SupervisorList
