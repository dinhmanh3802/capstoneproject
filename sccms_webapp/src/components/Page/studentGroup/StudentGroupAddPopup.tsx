import React, { useMemo, useState } from "react"
import { useGetStudentCourseQuery } from "../../../apis/studentApplicationApi"
import DataTable, { TableColumn } from "react-data-table-component"
import { SD_Gender, SD_ProcessStatus, SD_ProcessStatus_Name } from "../../../utility/SD"
import { error } from "../../../utility/Message"
import { format } from "date-fns"
import { Accordion, Button, Col, Form, Modal, Row } from "react-bootstrap"

interface StudentGroupAddPopup {
    show: boolean
    onHide: () => void
    onAddStudents: (studentIds: number[]) => void
    currentStudentGroup: any
}

const getStudentStatusName = (status: SD_ProcessStatus) => {
    return status === SD_ProcessStatus.Approved
        ? SD_ProcessStatus_Name.WaitingForEnroll
        : status === SD_ProcessStatus.Enrolled
        ? SD_ProcessStatus_Name.Enrolled
        : status === SD_ProcessStatus.Graduated
        ? SD_ProcessStatus_Name.Graduated
        : SD_ProcessStatus_Name.DropOut
}

function StudentGroupAddPopup({ show, onHide, onAddStudents, currentStudentGroup }: StudentGroupAddPopup) {
    const [currentPage, setCurrentPage] = useState(1)
    const [rowsPerPage, setRowsPerPage] = useState(10)
    const [selectedRows, setSelectedRows] = useState<number[]>([])
    const initParam = {
        studentCode: "",
        name: "",
        phone: "",
        status: "",
        gender: currentStudentGroup?.gender,
        studentGroup: "",
        courseId: currentStudentGroup?.courseId || 0,
        StudentGroupExcept: currentStudentGroup?.id || 0,
        isGetStudentDrop: false,
    }
    const [searchParams, setSearchParams] = useState(initParam)
    const [formParams, setFormParams] = useState(initParam)

    const { data: applicationData, isLoading: applicationLoading } = useGetStudentCourseQuery(searchParams, {
        skip: !currentStudentGroup,
    })

    console.log(applicationData?.result)

    const filteredData = useMemo(() => {
        if (!applicationData) return []
        return applicationData?.result.filter(
            (item) =>
                item?.status !== SD_ProcessStatus.Pending &&
                item?.status !== SD_ProcessStatus.DropOut &&
                item?.status !== SD_ProcessStatus.Deleted &&
                item?.status !== SD_ProcessStatus.Graduated &&
                item?.status !== SD_ProcessStatus.Rejected,
        )
    }, [applicationData])

    const handleSearch = (e: React.MouseEvent<HTMLButtonElement>) => {
        e.preventDefault()
        setSearchParams(formParams)
    }

    const handleReset = () => {
        setFormParams(initParam)
        setSearchParams(initParam)
    }

    const handleInputChange = (e: any) => {
        const { name, value } = e.target
        setFormParams((prev) => ({
            ...prev,
            [name]: value,
        }))
    }

    const handleRowSelected = (selected: any) => {
        const selectedIds = selected.selectedRows?.map((row: any) => row.student.id)
        setSelectedRows(selectedIds)
    }

    const handleAddSelectedStudents = () => {
        if (selectedRows.length === 0) {
            alert("Vui lòng chọn ít nhất một sinh viên.")
            return
        }
        onAddStudents(selectedRows)
        setSelectedRows([]) // Clear selections
        onHide() // Close modal after adding
    }
    if (applicationLoading) return <div>Đang tải...</div>

    const columns: TableColumn<any>[] = [
        {
            name: "#",
            width: "50px",
            center: true,
            cell: (row, rowIndex: number) => {
                const index = (currentPage - 1) * rowsPerPage + rowIndex + 1
                return index
            },
        },
        {
            name: "Mã",
            minWidth: "8rem",
            selector: (row) => row.studentCode ?? "",
            sortable: true,
            center: true,
        },
        {
            name: "Họ và tên",
            selector: (row) => row.student?.fullName ?? "",
            minWidth: "12rem",
            sortable: true,
            cell: (row) => row.student?.fullName,
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
            name: "Giới tính",
            minWidth: "9rem",
            width: "9rem",
            selector: (row) => row.student.gender ?? "",
            sortable: true,
            cell: (row) => (row.student.gender === SD_Gender.Male ? "Nam" : "Nữ"),
        },
        {
            name: "Chánh",
            minWidth: "8rem",
            selector: (row) => {
                if (row.student && Array.isArray(row.student.studentGroups) && row.student.studentGroups.length > 0) {
                    return row.student.studentGroups[0].groupName
                }
                return "Chưa phân"
            },
            sortable: true,
            cell: (row) =>
                Array.isArray(row.student.studentGroups) && row.student.studentGroups.length > 0
                    ? row.student.studentGroups[0].groupName
                    : "Chưa phân",
        },
        {
            name: "Trạng thái",
            minWidth: "15rem",
            selector: (row) => row.status,
            cell: (row) => {
                return <span>{getStudentStatusName(row.status)}</span>
            },
        },
    ]

    return (
        <Modal show={show} onHide={onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Thêm khóa sinh vào chánh</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Accordion defaultActiveKey="1">
                    <Accordion.Item eventKey="0">
                        <Accordion.Header>
                            <i className="bi bi-search me-2"></i> Tìm kiếm
                        </Accordion.Header>
                        <Accordion.Body>
                            <Form>
                                <Row>
                                    <Col md={3}>
                                        <Form.Group className="mb-3">
                                            <Form.Label>Mã sinh viên</Form.Label>
                                            <Form.Control
                                                type="text"
                                                name="studentCode"
                                                value={formParams.studentCode}
                                                onChange={handleInputChange}
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col md={3}>
                                        <Form.Group className="mb-3">
                                            <Form.Label>Họ và tên</Form.Label>
                                            <Form.Control
                                                type="text"
                                                name="name"
                                                value={formParams.name}
                                                onChange={handleInputChange}
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col md={3}>
                                        <Form.Group className="mb-3">
                                            <Form.Label>Số điện thoại</Form.Label>
                                            <Form.Control
                                                type="text"
                                                name="phone"
                                                value={formParams.phone}
                                                onChange={handleInputChange}
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col md={3}>
                                        <Form.Group className="mb-3">
                                            <Form.Label>Trạng thái</Form.Label>
                                            <Form.Select
                                                name="status"
                                                value={formParams.status}
                                                onChange={handleInputChange}
                                            >
                                                <option value="">Tất cả</option>
                                                <option value={SD_ProcessStatus.Approved}>Đã duyệt</option>
                                                <option value={SD_ProcessStatus.Enrolled}>Đã đăng ký</option>
                                                <option value={SD_ProcessStatus.Graduated}>Đã tốt nghiệp</option>
                                                <option value={SD_ProcessStatus.DropOut}>Đã rớt</option>
                                            </Form.Select>
                                        </Form.Group>
                                    </Col>
                                </Row>
                                <div className="text-end ">
                                    <Button className="me-2" onClick={handleSearch}>
                                        Tìm kiếm
                                    </Button>
                                    <Button variant="secondary" onClick={handleReset}>
                                        Xóa
                                    </Button>
                                </div>
                            </Form>
                        </Accordion.Body>
                    </Accordion.Item>
                </Accordion>

                <div className="card mt-3">
                    <div className="card-body">
                        <DataTable
                            columns={columns}
                            data={filteredData}
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
                            onSelectedRowsChange={handleRowSelected}
                        />
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={handleAddSelectedStudents}>
                    Thêm
                </Button>
                <Button variant="secondary" onClick={onHide}>
                    Đóng
                </Button>
            </Modal.Footer>
        </Modal>
    )
}

export default StudentGroupAddPopup
