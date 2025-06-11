import React, { useEffect, useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import { SD_ProcessStatus, SD_Gender, SD_Role_Name, SD_ProcessStatus_Name, SD_CourseStatus } from "../../../utility/SD"
import { apiResponse, studentApplicationModel, userModel } from "../../../interfaces"
import { Link } from "react-router-dom"
import { format } from "date-fns"
import { OverlayTrigger, Tooltip } from "react-bootstrap"
import Select from "react-select"
import { useUpdateStudentApplicationMutation } from "../../../apis/studentApplicationApi"
import { toastNotify } from "../../../helper"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import studentModel from "../../../interfaces/studentModel"
import { useAddStudentsIntoGroupMutation } from "../../../apis/studentGroupApi"

interface Application extends studentApplicationModel {
    id: number
    courseId: number
    studentId: number
    student: studentModel
    applicationDate: string
    status: SD_ProcessStatus
    note: string
    reviewerId?: number
    reviewer?: userModel
    reviewDate?: string
}

const getApplicationStatusText = (status: SD_ProcessStatus) => {
    switch (status) {
        case SD_ProcessStatus.Pending:
            return <span className="badge bg-warning text-white">{SD_ProcessStatus_Name.Pending}</span>
        case SD_ProcessStatus.Rejected:
            return <span className="badge bg-danger text-white">{SD_ProcessStatus_Name.Rejected}</span>
        default:
            return <span className="badge bg-success text-white">{SD_ProcessStatus_Name.Approved}</span>
    }
}

function StudentApplicationList({
    applications,
    secretaryList,
    studentGroupList,
    currentCourse,
    onSelectUser,
    clearSelectedRows,
    refetch, // Add this prop
}: {
    applications: Application[]
    secretaryList: any
    studentGroupList: any
    currentCourse: any
    onSelectUser: (selected: any) => void
    clearSelectedRows: boolean
    refetch: () => void // Define the prop type
}) {
    const [selectedRows, setSelectedRows] = useState<Application[]>([])
    const [updateStudentApplication] = useUpdateStudentApplicationMutation()
    const [addStudentsIntoGroup] = useAddStudentsIntoGroupMutation()
    const [clearRows, setClearRows] = useState(false)
    const [currentPage, setCurrentPage] = useState(1)
    const [rowsPerPage, setRowsPerPage] = useState(10) // Số dòng mỗi trang mặc định

    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const reviewerOptions = secretaryList?.map((secretary: any) => ({
        value: secretary.id,
        label: secretary.userName,
        fullDisplay: `${secretary.userName} - ${secretary.fullName}`,
    }))

    const studentGroupOptions = studentGroupList?.map((group: any) => ({
        value: group.id,
        label: group.groupName,
        gender: group.gender,
    }))

    useEffect(() => {
        setClearRows(!clearRows) // Đổi trạng thái để DataTable xóa các hàng
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

    const handleDropdownChange = async (option: any) => {
        const selectedOption = { ...option }
        const idsToUpdate = selectedRows.length > 0 ? selectedRows?.map((row) => row.id) : [selectedOption.rowId]

        const studentApplicationUpdate = {
            Ids: idsToUpdate,
            ReviewerId: selectedOption?.value || null,
        }

        try {
            const response: apiResponse = await updateStudentApplication(studentApplicationUpdate)
            if (response.data?.isSuccess) {
                toastNotify("Cập nhật thư ký thành công", "success")
                setClearRows(!clearRows)
                setSelectedRows([])
            } else {
                toastNotify("Cập nhật thư ký thất bại", "error")
            }
        } catch (error) {
            toastNotify("Cập nhật thư ký thất bại", "error")
        }
    }

    const handleGroupChange = async (selectedOption: any, studentId: number) => {
        try {
            const response: apiResponse = await addStudentsIntoGroup({
                studentIds: [studentId],
                studentGroupId: selectedOption?.value || null,
                courseId: currentCourse?.id ?? null,
            })
            if (response.data?.isSuccess) {
                toastNotify("Cập nhật nhóm thành công", "success")
                refetch()
                setClearRows(!clearRows)
                setSelectedRows([])
            } else {
                toastNotify("Cập nhật nhóm thất bại", "error")
            }
        } catch (error) {
            toastNotify("Cập nhật nhóm thất bại", "error")
        }
    }

    const handleSelectRows = (selectedRowsChange: any) => {
        setSelectedRows(selectedRowsChange.selectedRows)
        onSelectUser(selectedRowsChange)
    }

    const columns: TableColumn<Application>[] = [
        {
            name: "#",
            minWidth: "50px",
            center: true,
            cell: (row: Application, rowIndex: number) => {
                const index = (currentPage - 1) * rowsPerPage + rowIndex + 1
                return index
            },
        },
        // Cột Họ và tên
        {
            name: "Họ và tên",
            selector: (row) => row.student?.fullName ?? "", // Cung cấp giá trị mặc định
            minWidth: "12rem",
            sortable: true,
            cell: (row) => (
                <Link
                    to={`/student-applications/${row.id}`}
                    style={{ textDecoration: "none", color: "inherit" }}
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                    title={row.student?.fullName || "N/A"}
                >
                    {(row.student?.fullName?.length || 0) > 50
                        ? `${row.student.fullName.substring(0, 47)}...`
                        : row.student.fullName || "N/A"}
                </Link>
            ),
        },

        {
            name: "Phụ huynh",
            minWidth: "12rem",
            selector: (row) => row.student?.parentName ?? "",
            sortable: true,
        },
        {
            name: "Điện thoại",
            minWidth: "10rem",
            selector: (row) => row.student?.emergencyContact ?? "",
            sortable: true,
        },
        {
            name: "Giới tính",
            minWidth: "9rem",
            selector: (row) => (row.student?.gender == SD_Gender.Female ? "Nữ" : "Nam"),
            sortable: true,
        },
        {
            name: "Ngày sinh",
            minWidth: "9rem",
            selector: (row) => row.student?.dateOfBirth?.toString() ?? "",
            sortable: true,
            cell: (row) => {
                const date = new Date(row.student?.dateOfBirth ?? "")
                return isNaN(date.getTime()) ? "Không hợp lệ" : format(date, "dd/MM/yyyy")
            },
        },
        {
            name: "Chánh",
            minWidth: "11rem",
            selector: (row: Application) => {
                if (row.student && Array.isArray(row.student.studentGroups) && row.student.studentGroups.length > 0) {
                    return row.student.studentGroups[0].groupName
                }
                return "Chưa phân"
            },
            sortable: true,
            center: true,
            cell: (row: Application) => {
                if (
                    currentCourse?.status == SD_CourseStatus.notStarted ||
                    currentCourse?.status == SD_CourseStatus.recruiting
                ) {
                    // Filter the studentGroupOptions based on the student's gender
                    const filteredStudentGroupOptions = studentGroupOptions?.filter(
                        (group: any) => group.gender === row.student.gender,
                    )

                    return (
                        <Select
                            name="studentGroup"
                            className="w-100"
                            placeholder="Chọn..."
                            options={filteredStudentGroupOptions}
                            onChange={(selectedOption) => handleGroupChange(selectedOption, row.studentId)}
                            value={filteredStudentGroupOptions?.find(
                                (option: any) => option.value === row.student?.studentGroups?.[0]?.id,
                            )}
                            styles={selectStyles}
                            menuPortalTarget={document.body}
                            menuPosition="fixed"
                            isClearable={false}
                            isDisabled={row.status != SD_ProcessStatus.Approved}
                        />
                    )
                }
                return Array.isArray(row.student?.studentGroups) && row.student.studentGroups.length > 0
                    ? row.student.studentGroups[0].groupName
                    : "Chưa phân"
            },
        },
        {
            name: "Trạng thái",
            minWidth: "10rem",
            selector: (row) => row.status,
            cell: (row: Application) => getApplicationStatusText(row.status),
            sortable: true,
        },
        {
            name: "Người duyệt",
            minWidth: "13rem",
            selector: (row: any) => row.reviewer?.userName ?? "",
            sortable: true,
            cell: (row: any) => {
                const isDisabled =
                    (row.status !== SD_ProcessStatus.Approved &&
                        row.status !== SD_ProcessStatus.Rejected &&
                        row.status !== SD_ProcessStatus.Pending) ||
                    currentUserRole == SD_Role_Name.SECRETARY ||
                    row.status == SD_ProcessStatus.Approved ||
                    row.status == SD_ProcessStatus.Rejected ||
                    (currentCourse.status != SD_CourseStatus.notStarted &&
                        currentCourse.status != SD_CourseStatus.recruiting)

                return isDisabled ? (
                    <span>{row.reviewer?.userName || "Chưa có"}</span>
                ) : (
                    <Select
                        name="reviewer"
                        placeholder="Chọn..."
                        className="w-100"
                        options={reviewerOptions}
                        onChange={(selectedOption) => handleDropdownChange({ ...selectedOption, rowId: row.id })}
                        value={reviewerOptions?.find((option: any) => option.value === row.reviewerId)}
                        isClearable
                        styles={selectStyles}
                        menuPortalTarget={document.body}
                        menuPosition="fixed"
                        formatOptionLabel={(option: any, { context }) => {
                            return context === "menu" ? option.fullDisplay : option.label
                        }}
                    />
                )
            },
            ignoreRowClick: true,
        },
        {
            name: "Hành động",
            cell: (row) => (
                <div>
                    <Link to={`/student-applications/${row.id}`} className="me-2 fs-3">
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
            width: "6rem",
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
        </div>
    )
}

export default StudentApplicationList
