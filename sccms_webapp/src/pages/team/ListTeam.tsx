import React, { useEffect, useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { Button, Modal, Tooltip, OverlayTrigger, Form } from "react-bootstrap"
import Select from "react-select"
import { FaEye, FaEdit, FaTrash } from "react-icons/fa"
import { useSelector } from "react-redux"
import { useNavigate } from "react-router-dom"
import { RootState } from "../../store/store"
import {
    useGetTeamsByCourseIdQuery,
    useDeleteTeamMutation,
    useCreateTeamMutation,
    useUpdateTeamMutation,
} from "../../apis/teamApi"
import { useGetUsersQuery } from "../../apis/userApi"
import { SD_Gender, SD_Role } from "../../utility/SD"
import teamModel from "../../interfaces/teamModel"
import { userModel } from "../../interfaces/userModel"
import { toast } from "react-toastify"
import { toastNotify } from "../../helper"
import { set } from "date-fns"
import { courseModel } from "../../interfaces"

const TeamList: React.FC = () => {
    const navigate = useNavigate()
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const [selectedCourse, setSelectedCourse] = useState(currentCourse?.id)
    const [showConfirmModal, setShowConfirmModal] = useState(false)
    const [showCreateModal, setShowCreateModal] = useState(false)
    const [showEditModal, setShowEditModal] = useState(false)
    const [currentRow, setCurrentRow] = useState<teamModel | null>(null)
    const [deleteTeam] = useDeleteTeamMutation()
    const [createTeam] = useCreateTeamMutation()
    const [updateTeam] = useUpdateTeamMutation()
    const [teamName, setTeamName] = useState("")
    const [gender, setGender] = useState<SD_Gender | null>(null)
    const [expectedVolunteers, setExpectedVolunteers] = useState<number | undefined>(undefined)
    const [leader, setLeader] = useState<{ value: number; label: string } | null>(null)

    const [errors, setErrors] = useState<{ [key: string]: string }>({})

    const {
        data: teamData,
        isLoading: teamLoading,
        error,
    } = useGetTeamsByCourseIdQuery(Number(selectedCourse), {
        skip: !selectedCourse,
    })
    const { data: userData, isLoading: userLoading } = useGetUsersQuery({})
    // Tìm khóa tu được chọn
    const selectedCourseDetails = listCourseFromStore?.find((course: courseModel) => course.id === selectedCourse)
    // Kiểm tra xem khóa tu đã kết thúc chưa
    const isCourseInPast =
        selectedCourseDetails && selectedCourseDetails.endDate
            ? new Date(selectedCourseDetails.endDate) < new Date()
            : false
    useEffect(() => {
        if (currentCourse) {
            setSelectedCourse(currentCourse.id)
        }
    }, [currentCourse])

    const handleCourseChange = (selectedOption: any) => {
        setSelectedCourse(selectedOption ? selectedOption.value : null)
    }
    const handleView = (row: teamModel) => {
        navigate(`/team/${row.id}`)
    }

    const handleDelete = (row: teamModel) => {
        setCurrentRow(row)
        setShowConfirmModal(true)
    }

    const confirmDelete = async () => {
        if (currentRow) {
            try {
                await deleteTeam(currentRow.id).unwrap()
                setShowConfirmModal(false)
            } catch (error) {
                console.error("Error deleting team:", error)
            }
        }
    }

    const validateForm = () => {
        const newErrors: { [key: string]: string } = {}
        const trimmedTeamName = teamName.trim()
        if (!trimmedTeamName) newErrors.teamName = "Tên ban là bắt buộc"
        else if (trimmedTeamName.length > 100) newErrors.teamName = "Tên ban phải nhỏ hơn 100 kí tự"
        if (!expectedVolunteers || expectedVolunteers > 1000) {
            newErrors.expectedVolunteers = "Số lượng dự kiến là bắt buộc và phải nhỏ hơn 1000"
        }
        if (!leader) newErrors.leader = "Trưởng ban là bắt buộc"

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const handleCreate = async () => {
        if (!validateForm()) return

        try {
            await createTeam({
                teamName,
                courseId: selectedCourse,
                gender,
                expectedVolunteers,
                leaderId: leader.value,
            }).unwrap()
            setShowCreateModal(false)
            resetForm()
        } catch (error) {
            toastNotify(error.data.errorMessages.join(", "), "error")
        }
    }

    const handleEdit = (row: teamModel) => {
        setCurrentRow(row)
        setTeamName(row.teamName)
        setGender(row.gender)
        setExpectedVolunteers(row.expectedVolunteers)
        const leaderUser = userData?.result?.find((user) => user.id === row.leaderId)
        setLeader(leaderUser ? { value: leaderUser.id, label: `${leaderUser.userName}: ${leaderUser.fullName}` } : null)
        setShowEditModal(true)
    }

    const handleUpdate = async () => {
        if (!validateForm()) return

        if (currentRow) {
            try {
                await updateTeam({
                    id: currentRow.id,
                    teamName,
                    courseId: selectedCourse,
                    gender,
                    expectedVolunteers,
                    leaderId: leader.value,
                }).unwrap()
                setShowEditModal(false)
                resetForm()
                toast.success("Cập nhật ban thành công")
            } catch (error) {
                toastNotify(error.data.errorMessages?.[0], "error")
            }
        }
    }

    const resetForm = () => {
        setTeamName("")
        setGender(null)
        setExpectedVolunteers(undefined)
        setLeader(null)
        setErrors({})
    }

    const filteredLeaders =
        userData?.result
            ?.filter((user: userModel) => user.roleId === SD_Role.STAFF && (gender === null || user.gender === gender))
            ?.map((user: userModel) => ({
                value: user.id,
                label: `${user.userName}: ${user.fullName}`,
            })) ?? []

    const columns: TableColumn<teamModel>[] = [
        {
            name: "Tên ban",
            selector: (row) => row.teamName,
            sortable: true,
            minWidth: "12rem",
        },
        {
            name: "Giới tính",
            selector: (row) => (row.gender === null ? "Tất cả" : row.gender === SD_Gender.Male ? "Nam" : "Nữ"),
            sortable: true,
            minWidth: "8rem",
        },
        {
            name: "Số lượng dự kiến",
            selector: (row) => (row.expectedVolunteers ? row.expectedVolunteers : "Chưa cập nhật"),
            sortable: true,
            width: "14rem",
        },
        {
            name: "Số lượng",
            selector: (row) => row.volunteers.length,
            sortable: true,
            minWidth: "8rem",
        },
        {
            name: "Trưởng ban",
            selector: (row) => {
                const leader = userData?.result?.find((user) => user.id === row.leaderId)
                return leader ? `${leader.userName}: ${leader.fullName}` : "Chưa có"
            },
            sortable: true,
            minWidth: "19rem",
        },
        {
            name: "Thao Tác",
            cell: (row) => (
                <div className="d-flex">
                    <OverlayTrigger
                        placement="top"
                        overlay={<Tooltip id={`tooltip-view-${row.id}`}>Xem chi tiết</Tooltip>}
                    >
                        <button onClick={() => handleView(row)} className="btn btn-outline-primary btn-sm m-1">
                            <i className="bi bi-eye"></i>
                        </button>
                    </OverlayTrigger>

                    {currentUserRole === "manager" && (
                        <>
                            <OverlayTrigger
                                placement="top"
                                overlay={<Tooltip id={`tooltip-edit-${row.id}`}>Chỉnh sửa</Tooltip>}
                            >
                                <button
                                    onClick={() => handleEdit(row)}
                                    disabled={isCourseInPast}
                                    className="btn btn-outline-warning btn-sm m-1"
                                >
                                    <i className="bi bi-pencil"></i>
                                </button>
                            </OverlayTrigger>

                            <OverlayTrigger
                                placement="top"
                                overlay={<Tooltip id={`tooltip-delete-${row.id}`}>Xóa</Tooltip>}
                            >
                                <button
                                    onClick={() => handleDelete(row)}
                                    disabled={isCourseInPast}
                                    className="btn btn-outline-danger btn-sm m-1"
                                >
                                    <i className="bi bi-trash"></i>
                                </button>
                            </OverlayTrigger>
                        </>
                    )}
                </div>
            ),
            center: true,
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
            minWidth: "12rem",
        },
    ]

    if (teamLoading || userLoading) {
        return <p>Đang tải dữ liệu...</p>
    }

    const courseOptions = listCourseFromStore.map((course: any) => ({
        value: course.id,
        label: course.courseName,
    }))

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách ban</h3>
            </div>
            <div className="d-flex align-items-center justify-content-between mb-3">
                <div style={{ width: "50%" }}>
                    <Select
                        options={courseOptions}
                        onChange={handleCourseChange}
                        placeholder="Chọn khóa tu"
                        value={courseOptions?.find((option) => option.value === selectedCourse) || null}
                    />
                </div>
                {currentUserRole === "manager" && !isCourseInPast && (
                    <Button variant="primary" onClick={() => setShowCreateModal(true)}>
                        + Thêm mới
                    </Button>
                )}
            </div>

            {error ? (
                <p>Lỗi khi tải dữ liệu đội nhóm.</p>
            ) : (
                <div className="card">
                    <div className="card-body">
                        <DataTable
                            columns={columns}
                            data={teamData?.result || []}
                            pagination
                            striped
                            responsive
                            noDataComponent="Không có dữ liệu"
                            customStyles={{
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
                            }}
                        />
                    </div>
                </div>
            )}

            {/* Modal Create Team */}
            <Modal
                show={showCreateModal}
                onHide={() => {
                    setShowCreateModal(false)
                    setTeamName("")
                    setGender(null)
                    setExpectedVolunteers(undefined)
                    setLeader(null)
                }}
            >
                <Modal.Header closeButton>
                    <Modal.Title>Tạo Ban Mới</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Tên ban <span className="text-danger">*</span>
                            </Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="Nhập tên ban"
                                value={teamName}
                                onChange={(e) => setTeamName(e.target.value)}
                                isInvalid={!!errors.teamName}
                            />
                            <Form.Control.Feedback type="invalid">{errors.teamName}</Form.Control.Feedback>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Giới tính</Form.Label>
                            <Select
                                options={[
                                    { value: SD_Gender.Male, label: "Nam" },
                                    { value: SD_Gender.Female, label: "Nữ" },
                                    { value: null, label: "Tất cả" },
                                ]}
                                onChange={(option) => setGender(option ? option.value : null)}
                                placeholder="Chọn giới tính"
                                value={
                                    gender === null
                                        ? { value: null, label: "Tất cả" }
                                        : gender === SD_Gender.Male
                                        ? { value: SD_Gender.Male, label: "Nam" }
                                        : { value: SD_Gender.Female, label: "Nữ" }
                                }
                            />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Số lượng dự kiến <span className="text-danger">*</span>
                            </Form.Label>
                            <Form.Control
                                type="number"
                                placeholder="Nhập số lượng dự kiến"
                                value={expectedVolunteers ?? ""}
                                onChange={(e) => setExpectedVolunteers(Number(e.target.value))}
                                isInvalid={!!errors.expectedVolunteers}
                            />
                            <Form.Control.Feedback type="invalid">{errors.expectedVolunteers}</Form.Control.Feedback>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Trưởng ban <span className="text-danger">*</span>
                            </Form.Label>
                            <Select
                                options={filteredLeaders}
                                onChange={(option) => setLeader(option)}
                                placeholder="Chọn trưởng ban"
                                value={leader}
                                className={errors.leader ? "is-invalid" : ""}
                            />
                            {errors.leader && <div className="invalid-feedback d-block">{errors.leader}</div>}
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button
                        variant="secondary"
                        onClick={() => {
                            setShowCreateModal(false)
                            setTeamName("")
                            setGender(null)
                            setExpectedVolunteers(undefined)
                            setLeader(null)
                        }}
                    >
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleCreate}>
                        Tạo
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Modal Edit Team */}
            <Modal
                show={showEditModal}
                onHide={() => {
                    setShowEditModal(false)
                    setShowCreateModal(false)
                    setTeamName("")
                    setGender(null)
                    setExpectedVolunteers(undefined)
                    setLeader(null)
                }}
            >
                <Modal.Header closeButton>
                    <Modal.Title>Chỉnh Sửa Ban</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Tên ban <span className="text-danger">*</span>
                            </Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="Nhập tên ban"
                                value={teamName}
                                onChange={(e) => setTeamName(e.target.value)}
                                isInvalid={!!errors.teamName}
                            />
                            <Form.Control.Feedback type="invalid">{errors.teamName}</Form.Control.Feedback>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Giới tính</Form.Label>
                            <Select
                                isDisabled={true}
                                options={[
                                    { value: SD_Gender.Male, label: "Nam" },
                                    { value: SD_Gender.Female, label: "Nữ" },
                                    { value: null, label: "Tất cả" },
                                ]}
                                onChange={(option) => {
                                    setGender(option ? option.value : null)
                                    setLeader(null) // Reset trưởng ban khi giới tính thay đổi
                                }}
                                placeholder="Chọn giới tính"
                                value={
                                    gender === null
                                        ? { value: null, label: "Tất cả" }
                                        : gender === SD_Gender.Male
                                        ? { value: SD_Gender.Male, label: "Nam" }
                                        : { value: SD_Gender.Female, label: "Nữ" }
                                }
                            />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Số lượng dự kiến <span className="text-danger">*</span>
                            </Form.Label>
                            <Form.Control
                                type="number"
                                placeholder="Nhập số lượng dự kiến"
                                value={expectedVolunteers ?? ""}
                                onChange={(e) => setExpectedVolunteers(Number(e.target.value))}
                                isInvalid={!!errors.expectedVolunteers}
                            />
                            <Form.Control.Feedback type="invalid">{errors.expectedVolunteers}</Form.Control.Feedback>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>
                                Trưởng ban <span className="text-danger">*</span>
                            </Form.Label>
                            <Select
                                options={filteredLeaders}
                                onChange={(option) => setLeader(option)}
                                placeholder="Chọn trưởng ban"
                                value={leader}
                                className={errors.leader ? "is-invalid" : ""}
                            />
                            {errors.leader && <div className="invalid-feedback d-block">{errors.leader}</div>}
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button
                        variant="secondary"
                        onClick={() => {
                            setShowEditModal(false)
                            setShowCreateModal(false)
                            setTeamName("")
                            setGender(null)
                            setExpectedVolunteers(undefined)
                            setLeader(null)
                        }}
                    >
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleUpdate}>
                        Cập nhật
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Popup xác nhận xóa */}
            <Modal show={showConfirmModal} onHide={() => setShowConfirmModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Xác nhận xóa</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    Bạn có chắc chắn muốn xóa ban <strong>{currentRow?.teamName}</strong> không?
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowConfirmModal(false)}>
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={confirmDelete}>
                        Xác nhận
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default TeamList
