import React, { useState, useEffect } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { Link, useLocation } from "react-router-dom"
import { useGetTeamByIdQuery, useGetVolunteersInTeamQuery } from "../../../apis/teamApi"
import { format } from "date-fns"
import { SD_EmployeeProcessStatus_Name, SD_Gender, SD_ProcessStatus, SD_ProcessStatus_Name } from "../../../utility/SD"
import { Button, OverlayTrigger, Tooltip } from "react-bootstrap"
import { MainLoader } from ".."

const VolunteerListByTeam = ({ teamId, onSelectRows }) => {
    const location = useLocation()
    const searchParams = Object.fromEntries(new URLSearchParams(location.search))
    const { data: team, isLoading: teamLoading } = useGetTeamByIdQuery(Number(teamId))

    // Lấy dữ liệu từ API với tham số tìm kiếm từ URL
    const { data: volunteers, isLoading } = useGetVolunteersInTeamQuery({ teamId, ...searchParams })
    const [selectedRows, setSelectedRows] = useState<number[]>([])

    const handleRowSelected = (state: any) => {
        const selectedIds = state.selectedRows?.map((row: any) => row.id)
        setSelectedRows(selectedIds)
        onSelectRows(selectedIds)
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

    const getVolunteerStatusName = (status: SD_ProcessStatus) => {
        return status === SD_ProcessStatus.Approved
            ? SD_EmployeeProcessStatus_Name.WaitingForEnroll
            : status === SD_ProcessStatus.Enrolled
            ? SD_EmployeeProcessStatus_Name.Enrolled
            : status === SD_ProcessStatus.Graduated
            ? SD_EmployeeProcessStatus_Name.Graduated
            : SD_EmployeeProcessStatus_Name.DropOut
    }

    const columns = [
        { name: "Mã TNV", minWidth: "150px", selector: (row) => row.volunteerCode, sortable: true },
        { name: "Họ và tên", minWidth: "200px", selector: (row) => row.fullName, sortable: true },
        { name: "Điện thoại", minWidth: "150px", selector: (row) => row.phoneNumber, sortable: true },
        { name: "Ngày sinh", minWidth: "150px", selector: (row) => format(new Date(row.dateOfBirth), "dd/MM/yyyy") },
        { name: "Giới tính", minWidth: "150px", selector: (row) => (row.gender === SD_Gender.Male ? "Nam" : "Nữ") },
        { name: "Trạng thái", minWidth: "150px", selector: (row) => getVolunteerStatusName(row.status) },
        {
            name: "Thao tác",
            minWidth: "10rem",
            cell: (row) => (
                <div>
                    <Link to={`/volunteer/${row?.id}/course/${team?.result?.courseId}`} className="me-2 fs-3">
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
        },
    ]

    if (isLoading || teamLoading) return <MainLoader />
    if (!volunteers || !volunteers.result) return <div>Không có dữ liệu tình nguyện viên</div>
    return (
        <div className="card">
            <div className="card-body">
                <DataTable
                    columns={columns}
                    data={volunteers.result}
                    customStyles={customStyles}
                    pagination
                    selectableRows
                    onSelectedRowsChange={handleRowSelected}
                    highlightOnHover
                />
            </div>
        </div>
    )
}

export default VolunteerListByTeam
