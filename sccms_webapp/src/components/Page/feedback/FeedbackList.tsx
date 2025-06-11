import DataTable, { TableColumn } from "react-data-table-component"
import { format } from "date-fns"
import { OverlayTrigger, Tooltip } from "react-bootstrap"
import { BsEye, BsTrash } from "react-icons/bs"
import { useState } from "react"
import FeedbackDetailPopup from "./FeedbackDetailPopup"

interface Feedback {
    id: number
    studentCode: string
    courseName: string
    content: string
    submissionDate: string
}

function FeedbackList({
    feedbacks,
    onDelete,
    onBulkDelete,
}: {
    feedbacks: Feedback[]
    onDelete: (id: number) => void
    onBulkDelete: (ids: number[]) => void
}) {
    const [selectedRows, setSelectedRows] = useState<Feedback[]>([])
    const [showDetailPopup, setShowDetailPopup] = useState(false)
    const [selectedFeedback, setSelectedFeedback] = useState<Feedback | null>(null)
    const handleRowSelected = (state: any) => {
        setSelectedRows(state.selectedRows)
    }
    const handleBulkDelete = () => {
        const idsToDelete = selectedRows?.map((row) => row.id)
        onBulkDelete(idsToDelete)
    }
    const handleViewDetail = (feedback: Feedback) => {
        setSelectedFeedback(feedback)
        setShowDetailPopup(true)
    }
    const columns: TableColumn<Feedback>[] = [
        {
            name: "Ngày",
            selector: (row) => row.submissionDate,
            sortable: true,
            cell: (row) => format(new Date(row.submissionDate), "dd/MM/yyyy"),
            width: "150px",
        },
        {
            name: "Nội dung",
            selector: (row) => row.content,
            cell: (row) => {
                const maxLength = 100 // Đặt giới hạn số ký tự ở đây
                const content =
                    row.content.length > maxLength ? row.content.substring(0, maxLength) + "..." : row.content
                return (
                    <div style={{ whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{content}</div>
                )
            },
            grow: 2,
        },
        {
            name: "Thao tác",
            cell: (row) => (
                <div className="d-flex">
                    <OverlayTrigger placement="top" overlay={<Tooltip>Xem</Tooltip>}>
                        <button className="btn btn-outline-secondary btn-sm me-2" onClick={() => handleViewDetail(row)}>
                            <BsEye />
                        </button>
                    </OverlayTrigger>
                    <OverlayTrigger placement="top" overlay={<Tooltip>Xóa</Tooltip>}>
                        <button className="btn btn-outline-danger btn-sm" onClick={() => onDelete(row.id)}>
                            <BsTrash />
                        </button>
                    </OverlayTrigger>
                </div>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
            width: "120px",
        },
    ]

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

    return (
        <>
            <div className="container text-end mt-4">
                <button
                    className="btn btn-danger btn-sm me-2 mb-2"
                    onClick={handleBulkDelete}
                    disabled={selectedRows.length === 0}
                >
                    Xóa
                </button>
            </div>
            <div className="card">
                <div className="card-body">
                    <DataTable
                        columns={columns}
                        data={feedbacks}
                        customStyles={customStyles}
                        onSelectedRowsChange={handleRowSelected}
                        pagination
                        selectableRows
                        responsive
                        striped
                        noDataComponent="Không có phản hồi nào"
                    />
                </div>
            </div>
            <FeedbackDetailPopup
                isOpen={showDetailPopup}
                onClose={() => setShowDetailPopup(false)}
                feedback={selectedFeedback}
            />
        </>
    )
}

export default FeedbackList
