import React, { useEffect, useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import { SD_ProcessStatus, SD_Gender, SD_Role_Name, SD_ProcessStatus_Name, SD_CourseStatus } from "../../../utility/SD"
import { apiResponse, userModel } from "../../../interfaces"
import { Link, useParams } from "react-router-dom"
import { format } from "date-fns"
import { OverlayTrigger, Tooltip } from "react-bootstrap"
import Select from "react-select"
import { useUpdateVolunteerApplicationMutation } from "../../../apis/volunteerApplicationApi"
import { toastNotify } from "../../../helper"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import volunteerModel from "../../../interfaces/volunteerModel"
import { useAddVolunteersToTeamMutation, useRemoveVolunteersFromTeamMutation } from "../../../apis/teamApi"
import volunteerApplicationModel from "../../../interfaces/volunteerApplicationModel"

interface Application extends volunteerApplicationModel {
    id: number
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

function VolunteerApplicationList({
    applications,
    secretaryList,
    teamList,
    currentCourse,
    onSelectUser,
    clearSelectedRows,
    refetch,
}: {
    applications: Application[]
    secretaryList: any
    teamList: any
    currentCourse: any
    onSelectUser: (selected: any) => void
    clearSelectedRows: boolean
    refetch: () => void
}) {
    const [selectedRows, setSelectedRows] = useState<Application[]>([])
    const [updateVolunteerApplication] = useUpdateVolunteerApplicationMutation()
    const [addVolunteersIntoTeam] = useAddVolunteersToTeamMutation()
    const [removeVolunteersFromTeam] = useRemoveVolunteersFromTeamMutation()
    const [clearRows, setClearRows] = useState(false)
    const [currentPage, setCurrentPage] = useState(1)
    const [rowsPerPage, setRowsPerPage] = useState(10)
    const [applicationsData, setApplicationsData] = useState(applications)
    const courses = useSelector((state: RootState) => state.courseStore.courses)

    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const reviewerOptions = secretaryList?.map((secretary: any) => ({
        value: secretary.id,
        label: secretary.userName,
        fullDisplay: `${secretary.userName} - ${secretary.fullName}`,
    }))

    const teamOptions = teamList?.map((team: any) => ({
        value: team.id,
        gender: team.gender,
        label: team.teamName,
    }))
    const [dataTableKey, setDataTableKey] = useState(0)

    useEffect(() => {
        setDataTableKey((prevKey) => prevKey + 1) // Thay đổi key để buộc DataTable render lại
    }, [applications])

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
                fontSize: "14px",
                fontWeight: "bold",
            },
        },
        rows: {
            style: {
                fontSize: "14px",
            },
        },
    }

    const handleDropdownChange = async (option: any) => {
        const selectedOption = { ...option }
        const idsToUpdate = selectedRows.length > 0 ? selectedRows?.map((row) => row.id) : [selectedOption.rowId]

        const volunteerApplicationUpdate = {
            Ids: idsToUpdate,
            ReviewerId: selectedOption?.value || null,
        }

        try {
            const response: apiResponse = await updateVolunteerApplication(volunteerApplicationUpdate)
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

    const handleTeamChange = async (selectedOption: any, volunteerId: number, previousValue: number) => {
        if (selectedOption !== null) {
            try {
                const response: apiResponse = await addVolunteersIntoTeam({
                    volunteerIds: [volunteerId],
                    teamId: selectedOption?.value || null,
                    courseId: currentCourse?.id ?? null,
                })
                if (response.data?.isSuccess) {
                    toastNotify("Cập nhật ban thành công", "success")
                    refetch()
                    setClearRows(!clearRows)
                    setSelectedRows([])
                } else {
                    toastNotify("Cập nhật ban thất bại", "error")
                }
            } catch (error) {
                toastNotify("Cập nhật ban thất bại", "error")
            }
        } else {
            try {
                const response: apiResponse = await removeVolunteersFromTeam({
                    volunteerIds: [volunteerId], // @ts-ignore
                    teamId: previousValue?.value || null,
                    courseId: currentCourse?.id ?? null,
                })
                if (response.data?.isSuccess) {
                    toastNotify("Cập nhật ban thành công", "success")
                    refetch()
                    setClearRows(!clearRows)
                    setSelectedRows([])
                } else {
                    toastNotify("Cập nhật ban thất bại", "error")
                }
            } catch (error) {
                toastNotify("Cập nhật ban thất bại", "error")
            }
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
            name: "Họ và tên",
            selector: (row) => row.volunteer?.fullName ?? "",
            minWidth: "12rem",
            sortable: true,
            cell: (row) => (
                <Link
                    to={`/volunteer-applications/${row.id}`}
                    style={{ textDecoration: "none", color: "inherit" }}
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                    title={row.volunteer?.fullName || "N/A"}
                >
                    {(row.volunteer?.fullName?.length || 0) > 50
                        ? `${row.volunteer.fullName.substring(0, 47)}...`
                        : row.volunteer.fullName || "N/A"}
                </Link>
            ),
        },
        {
            name: "Điện thoại",
            width: "10rem",
            selector: (row) => row.volunteer?.phoneNumber ?? "",
            sortable: true,
        },
        {
            name: "Ngày sinh",
            width: "10rem",
            selector: (row) => row.volunteer?.dateOfBirth?.toString() ?? "",
            sortable: true,
            cell: (row) => {
                const date = new Date(row.volunteer?.dateOfBirth ?? "")
                return isNaN(date.getTime()) ? "Không hợp lệ" : format(date, "dd/MM/yyyy")
            },
        },
        {
            name: "Giới tính",
            width: "8rem",
            selector: (row) => row.volunteer?.gender ?? "",
            sortable: true,
            cell: (row) => {
                return row.volunteer?.gender ? "Nữ" : "Nam"
            },
        },
        {
            name: "Ban",
            minWidth: "16rem",
            selector: (row: Application) => {
                if (row.volunteer && Array.isArray(row.volunteer.teams) && row.volunteer.teams.length > 0) {
                    return row.volunteer.teams[0].teamName
                }
                return "Chưa phân"
            },
            sortable: true,
            center: true,

            cell: (row: Application) => {
                // Chỉ cho phép chỉnh sửa nếu trạng thái là Approved
                const isEditableStatus =
                    row.status === SD_ProcessStatus.Approved || currentCourse?.status == SD_CourseStatus.recruiting

                return (
                    <Select
                        name="team"
                        className="w-100"
                        options={teamOptions?.filter(
                            (option: any) => option.gender === row.volunteer?.gender || option.gender === null,
                        )}
                        onChange={(selectedOption) => {
                            const previousValue = teamOptions?.find(
                                (option: any) => option.value === row.volunteer?.teams?.[0]?.id,
                            )
                            handleTeamChange(selectedOption, row.volunteerId, previousValue)
                        }}
                        value={teamOptions?.find((option: any) => option.value === row.volunteer?.teams?.[0]?.id)}
                        styles={selectStyles}
                        menuPortalTarget={document.body}
                        menuPosition="fixed"
                        isDisabled={row.status != SD_ProcessStatus.Approved}
                        // Vô hiệu hóa nếu trạng thái không phải là Approved và course đã đi
                        placeholder="Chọn ban..."
                    />
                )
            },
        },
        {
            name: "Trạng thái",
            width: "9rem",
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
                    row.status == SD_ProcessStatus.Rejected

                return isDisabled ? (
                    <span>{row.reviewer?.userName || "Chưa có người duyệt"}</span>
                ) : (
                    <Select
                        name="reviewer"
                        className="w-100"
                        options={reviewerOptions}
                        onChange={(selectedOption) => handleDropdownChange({ ...selectedOption, rowId: row.id })}
                        value={reviewerOptions?.find((option: any) => option.value === row.reviewerId)}
                        styles={selectStyles}
                        menuPortalTarget={document.body}
                        menuPosition="fixed"
                        placeholder="Chọn thư kí..."
                        isDisabled={courses
                            ?.filter((course) => course.status !== SD_CourseStatus.recruiting)
                            ?.map((course) => course.id)
                            ?.includes(row.courseId)}
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
                    <Link to={`/volunteer-applications/${row.id}`} className="me-2 fs-3">
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

export default VolunteerApplicationList
