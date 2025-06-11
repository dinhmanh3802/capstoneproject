import DataTable, { TableColumn } from "react-data-table-component"
import { error } from "../../../utility/Message"
import { getCourseStatusText } from "../../../helper/getCourseStatusText"
import { SD_CourseStatus } from "../../../utility/SD"
import { courseModel } from "../../../interfaces"
import { useNavigate } from "react-router-dom"
import { format } from "date-fns"

interface Course extends courseModel {
    id: number
    courseName: string
    startDate: string
    endDate: string
    status: SD_CourseStatus
}

const displayStatus = (status: SD_CourseStatus): JSX.Element => {
    switch (status) {
        case SD_CourseStatus.notStarted:
            return <span className="badge bg-secondary">Chưa bắt đầu</span>
        case SD_CourseStatus.recruiting:
            return <span className="badge bg-info text-dark">Đang tuyển sinh</span>
        case SD_CourseStatus.inProgress:
            return <span className="badge bg-danger">Đang diễn ra</span>
        case SD_CourseStatus.closed:
            return <span className="badge bg-success">kết thúc</span>
        case SD_CourseStatus.deleted:
            return <span className="badge bg-danger">Đã xóa</span>
        default:
            return <span className="badge bg-dark">Trạng thái không xác định</span>
    }
}

function CourseList({ course }: { course: any }) {
    const navigate = useNavigate()

    const handleAccess = (row: courseModel) => {
        navigate(`/course/${row.id}`)
    }

    const customStyles = {
        headCells: {
            style: {
                fontSize: "15px",
                fontWeight: "bold",
                whiteSpace: "nowrap",
                overflow: "visible",
                textOverflow: "unset",
            },
        },
        rows: {
            style: {
                fontSize: "15px",
            },
        },
    }

    const columns: TableColumn<Course>[] = [
        {
            name: "Tên khóa tu",
            selector: (row) => row.courseName,
            minWidth: "20rem",
            sortable: true,
            cell: (row) => (
                <span
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                    title={row.courseName} // Tooltip with full course name
                >
                    {row.courseName.length > 50 ? `${row.courseName.substring(0, 47)}...` : row.courseName}
                </span>
            ),
        },
        {
            name: "Ngày bắt đầu",
            minWidth: "12rem",
            selector: (row) => row.startDate,
            sortable: true,
            cell: (row) => format(new Date(row.startDate), "dd/MM/yyyy"),
        },
        {
            name: "Ngày kết thúc",
            minWidth: "12rem",
            selector: (row) => row.endDate,
            sortable: true,
            cell: (row) => format(new Date(row.endDate), "dd/MM/yyyy"),
        },
        {
            name: "Trạng thái",
            selector: (row) => row.status,
            cell: (row: Course) => displayStatus(row.status),
            sortable: true,
        },
        {
            name: "Hành động",
            cell: (row) => (
                <div>
                    <button className="btn btn-primary btn-sm me-2" onClick={() => handleAccess(row)}>
                        Chi tiết
                    </button>
                </div>
            ),
            width: "150px",
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
                    data={course}
                    customStyles={customStyles}
                    pagination
                    noDataComponent={error.NoData}
                ></DataTable>
            </div>
        </div>
    )
}

export default CourseList
