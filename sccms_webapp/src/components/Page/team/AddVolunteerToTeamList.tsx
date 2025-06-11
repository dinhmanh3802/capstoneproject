import React from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { useGetVolunteerApplicationsQuery } from "../../../apis/volunteerApplicationApi"
import { SD_Gender } from "../../../utility/SD"
import { format } from "date-fns"

interface AddVolunteerToTeamListProps {
    searchParams: any
    onSelectVolunteers: (volunteerIds: number[]) => void
    teamId: Number
    teamList: Array<{ value: number; label: string }>
}

const AddVolunteerToTeamList: React.FC<AddVolunteerToTeamListProps> = ({
    searchParams,
    onSelectVolunteers,
    teamId,
    teamList,
}) => {
    const { data, isLoading } = useGetVolunteerApplicationsQuery(searchParams)

    const handleSelectRows = (selectedRows: any) => {
        const selectedIds = selectedRows.selectedRows?.map((volunteer: any) => volunteer.volunteer.id)
        onSelectVolunteers(selectedIds)
    }

    const columns: TableColumn<any>[] = [
        { name: "Mã TNV", selector: (row) => row.volunteerCode, sortable: true },
        { name: "Họ và tên", minWidth: "11rem", selector: (row) => row.volunteer.fullName, sortable: true },
        { name: "Điện thoại", minWidth: "9rem", selector: (row) => row.volunteer.phoneNumber, sortable: true },
        {
            name: "Ngày sinh",
            minWidth: "9rem",
            selector: (row) => format(row.volunteer.dateOfBirth, "dd/MM/yyyy"),
            sortable: true,
        },
        {
            name: "Giới tính",
            minWidth: "8rem",
            selector: (row) => (row.volunteer.gender === SD_Gender.Male ? "Nam" : "Nữ"),
            sortable: true,
        },
        {
            name: "Ban",
            minWidth: "12rem",
            selector: (row) => teamList?.filter((tl) => tl.value === row.teamId)[0]?.label || "Chưa phân",
            sortable: true,
        },
    ]
    const customStyles = {
        headCells: {
            style: {
                fontSize: "13px",
                fontWeight: "bold",
            },
        },
        rows: {
            style: {
                fontSize: "13px",
            },
        },
    }

    return (
        <div className="card mt-3">
            <div className="card-body">
                <DataTable
                    columns={columns}
                    data={data?.result?.filter((row: any) => row.teamId !== teamId && row.status == 1) || []}
                    progressPending={isLoading}
                    pagination
                    selectableRows
                    onSelectedRowsChange={handleSelectRows}
                    customStyles={customStyles}
                    paginationPerPage={5}
                />
            </div>
        </div>
    )
}

export default AddVolunteerToTeamList
