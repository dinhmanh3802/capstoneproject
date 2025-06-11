import React, { useEffect, useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import {
    SD_ProcessStatus,
    SD_Gender,
    SD_Role_Name,
    SD_ProcessStatus_Name,
    SD_CourseStatus,
    SD_EmployeeProcessStatus_Name,
} from "../../../utility/SD"
import { apiResponse, studentApplicationModel, userModel } from "../../../interfaces"
import { Link } from "react-router-dom"
import { format } from "date-fns"
import { OverlayTrigger, Tooltip, Modal, Button } from "react-bootstrap"
import Select from "react-select"
import { useUpdateVolunteerApplicationMutation } from "../../../apis/volunteerApplicationApi"
import { toastNotify } from "../../../helper"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import volunteerModel from "../../../interfaces/volunteerModel"
import { useGetCourseByIdQuery } from "../../../apis/courseApi"

// Định nghĩa interface cho Application
interface Application extends studentApplicationModel {
    id: number
    volunteerCode: any
    courseId: number
    volunteerId: number
    volunteer: volunteerModel
    applicationDate: string
    status: SD_ProcessStatus
    note: string
    reviewerId?: number
    reviewer?: userModel
    reviewDate?: string
}

// Hàm để lấy tên trạng thái của tình nguyện viên
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
const getApplicationStatusOptions = (currentStatus: SD_ProcessStatus) => {
    let allowedStatuses: SD_ProcessStatus[] = [currentStatus]
    switch (currentStatus) {
        case SD_ProcessStatus.Approved:
            allowedStatuses.push(SD_ProcessStatus.Enrolled, SD_ProcessStatus.DropOut)
            break
        case SD_ProcessStatus.Enrolled:
            allowedStatuses.push(SD_ProcessStatus.Graduated, SD_ProcessStatus.DropOut)
            break
        case SD_ProcessStatus.Graduated:
            allowedStatuses.push(SD_ProcessStatus.DropOut, SD_ProcessStatus.Enrolled)
            break
        case SD_ProcessStatus.DropOut:
            allowedStatuses.push(SD_ProcessStatus.Enrolled, SD_ProcessStatus.Approved)
            break
        default:
            // Giữ nguyên trạng thái hiện tại
            break
    }
    // Loại bỏ các trạng thái trùng lặp
    allowedStatuses = Array.from(new Set(allowedStatuses))

    return allowedStatuses.map((status) => ({
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

const VolunteerList = ({
    applications,
    teamList,
    onSelectUser,
    clearSelectedRows,
    searchParams,
    currentCourse,
}: {
    applications: Application[]
    teamList: any
    onSelectUser: (selected: any) => void
    clearSelectedRows: boolean
    searchParams: any
    currentCourse: any
}) => {
    const [selectedRows, setSelectedRows] = useState<Application[]>([])
    const [updateVolunteerApplication] = useUpdateVolunteerApplicationMutation()
    const [clearRows, setClearRows] = useState(false)
    const [currentPage, setCurrentPage] = useState(1)
    const [rowsPerPage, setRowsPerPage] = useState(10)

    const [showConfirmModal, setShowConfirmModal] = useState(false)
    const [dropOutReason, setDropOutReason] = useState("")
    const [currentRow, setCurrentRow] = useState<Application | null>(null)

    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    const {
        data: courseData,
        isLoading: courseLoading,
        error: courseError,
    } = useGetCourseByIdQuery(applications[0]?.courseId || 0, { skip: !applications })

    const teamOptions = teamList?.map((team: any) => ({
        value: team.id,
        label: team.teamName,
    }))

    useEffect(() => {
        setClearRows(!clearRows)
    }, [clearSelectedRows])

    const selectStyles = {
        control: (provided: any) => ({
            ...provided,
            fontWeight: "normal",
            color: "black",
        }),
        singleValue: (provided: any) => ({
            ...provided,
            fontWeight: "normal",
            color: "black",
        }),
        option: (provided: any, state: any) => ({
            ...provided,
            fontWeight: "normal",
            color: "black",
            backgroundColor: state.isFocused ? "#e6e6e6" : "white",
        }),
    }

    const customStyles = {
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

    const handleStatusChange = (selectedOption: any, row: any) => {
        if (selectedOption.value === SD_ProcessStatus.DropOut) {
            setCurrentRow(row)
            setShowConfirmModal(true)
        } else {
            updateVolunteerStatus(selectedOption.value, row, "")
        }
    }

    const updateVolunteerStatus = async (status: SD_ProcessStatus, row: Application, reason: string) => {
        const volunteerApplicationUpdate = {
            Ids: [row.id],
            Status: status,
            Note: reason || row.note,
            ReviewerId: row.reviewerId,
        }

        try {
            const response: apiResponse = await updateVolunteerApplication(volunteerApplicationUpdate)
            if (response.data?.isSuccess) {
                toastNotify("Cập nhật trạng thái thành công", "success")
                setClearRows(!clearRows)
                setSelectedRows([])
            } else {
                toastNotify("Cập nhật trạng thái thất bại", "error")
            }
        } catch (error) {
            toastNotify("Cập nhật trạng thái thất bại", "error")
        }
    }

    const handleConfirmDropOut = () => {
        if (currentRow) {
            updateVolunteerStatus(SD_ProcessStatus.DropOut, currentRow, dropOutReason)
            setShowConfirmModal(false)
            setDropOutReason("")
        }
    }

    const handleSelectRows = (selectedRowsChange: any) => {
        setSelectedRows(selectedRowsChange.selectedRows)
        onSelectUser(selectedRowsChange)
    }

    const columns: TableColumn<Application>[] = [
        {
            name: "#",
            width: "50px",
            center: true,
            cell: (row: Application, rowIndex: number) => {
                const index = (currentPage - 1) * rowsPerPage + rowIndex + 1
                return index
            },
        },
        {
            name: "Mã",
            minWidth: "8rem",
            selector: (row) => row.volunteerCode ?? "",
            sortable: true,
            center: true,
        },
        {
            name: "Họ và tên",
            selector: (row) => row.volunteer?.fullName ?? "",
            minWidth: "12rem",
            sortable: true,
            cell: (row) => row.volunteer?.fullName,
        },
        {
            name: "Ngày sinh",
            minWidth: "9rem",
            selector: (row) => row.volunteer?.dateOfBirth?.toString() ?? "",
            sortable: true,
            cell: (row) => {
                const date = new Date(row.volunteer?.dateOfBirth ?? "")
                return isNaN(date.getTime()) ? "Không hợp lệ" : format(date, "dd/MM/yyyy")
            },
        },
        {
            name: "Giới tính",
            minWidth: "9rem",
            selector: (row) => row.volunteer.gender ?? "",
            sortable: true,
            cell: (row) => (row.volunteer.gender === SD_Gender.Male ? "Nam" : "Nữ"),
        },
        {
            name: "Ban",
            minWidth: "10rem",
            selector: (row: Application) => {
                if (row.volunteer && Array.isArray(row.volunteer.teams) && row.volunteer.teams.length > 0) {
                    return row.volunteer.teams[0].teamName
                }
                return <span className="text-danger">Chưa phân</span>
            },
            sortable: true,
            cell: (row: Application) =>
                Array.isArray(row.volunteer.teams) && row.volunteer.teams.length > 0 ? (
                    row.volunteer.teams[0].teamName
                ) : (
                    <span className="text-danger">Chưa phân</span>
                ),
        },
        {
            name: "Trạng thái",
            minWidth: "15rem",
            selector: (row) => row.status,
            cell: (row: Application) => {
                if (
                    courseData?.result.status !== SD_CourseStatus.closed &&
                    (currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER)
                ) {
                    return (
                        <Select
                            name="status"
                            className="w-100"
                            options={getApplicationStatusOptions(row.status)}
                            onChange={(selectedOption) => handleStatusChange(selectedOption, row)}
                            value={getApplicationStatusOptions(row.status)?.find(
                                (option: any) => option.value === row.status,
                            )}
                            isClearable={false}
                            styles={selectStyles}
                            menuPortalTarget={document.body}
                            menuPosition="fixed"
                        />
                    )
                }
                return <span>{getVolunteerStatusName(row.status)}</span>
            },
        },
        {
            name: "Hành động",
            cell: (row) => (
                <div>
                    <Link to={`/volunteer/${row.volunteer.id}/course/${row.courseId}`} className="me-2 fs-3">
                        <OverlayTrigger
                            placement="top"
                            overlay={<Tooltip id={`tooltip-view-${row.id}`}>Xem chi tiết</Tooltip>}
                        >
                            <button className="btn btn-outline-primary btn-sm m-1">
                                <i className="bi bi-eye"></i>
                            </button>
                        </OverlayTrigger>
                    </Link>
                </div>
            ),
            width: "8rem",
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
                    data={applications}
                    customStyles={customStyles}
                    pagination
                    onChangePage={(page) => setCurrentPage(page)}
                    onChangeRowsPerPage={(newPerPage, page) => {
                        setRowsPerPage(newPerPage)
                        setCurrentPage(page)
                    }}
                    noDataComponent={error.NoData}
                    responsive={true}
                    striped={true}
                    selectableRows
                    selectableRowsHighlight
                    selectableRowsSingle={false}
                    onSelectedRowsChange={handleSelectRows}
                    clearSelectedRows={clearRows}
                />
            </div>

            {/* Popup xác nhận DropOut */}
            <Modal show={showConfirmModal} onHide={() => setShowConfirmModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Xác nhận</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <p>
                        Xác nhận tình nguyện viên <strong>{currentRow?.volunteer?.fullName}</strong> rời khỏi khóa tu
                    </p>
                    <div>
                        <label>Lý do:</label>
                        <input
                            type="text"
                            className="form-control mt-2"
                            value={dropOutReason}
                            onChange={(e) => setDropOutReason(e.target.value)}
                            placeholder="Nhập lý do"
                        />
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowConfirmModal(false)}>
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleConfirmDropOut} disabled={!dropOutReason.trim()}>
                        Xác nhận
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default VolunteerList
