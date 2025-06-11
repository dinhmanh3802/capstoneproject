// src/pages/Supervisor/ListStudentGroup.tsx

import React, { useState, useEffect } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { MainLoader } from "../../components/Page"
import DataTable, { TableColumn } from "react-data-table-component"
import {
    useGetStudentGroupsQuery,
    useCreateStudentGroupMutation,
    useUpdateStudentGroupMutation,
    useDeleteStudentGroupMutation,
    useAutoAssignSupervisorsMutation,
} from "../../apis/studentGroupApi"
import { Link, useNavigate, useLocation } from "react-router-dom"
import { Button, Form, Modal, OverlayTrigger, Tooltip } from "react-bootstrap"
import { skipToken } from "@reduxjs/toolkit/query"
import { studentGroupModel, supervisorModel, courseModel } from "../../interfaces"
import { SD_Gender, SD_Role_Name } from "../../utility/SD"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { useGetAvailableSupervisorsQuery } from "../../apis/supervisorApi"
import { toast } from "react-toastify"
import Select from "react-select"
import { toastNotify } from "../../helper"

function ListStudentGroup() {
    const navigate = useNavigate()
    const location = useLocation()
    const [currentPage, setCurrentPage] = useState(1)
    const [rowsPerPage, setRowsPerPage] = useState(10)
    const [isEditModalOpen, setIsEditModalOpen] = useState(false)
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
    const [editingGroup, setEditingGroup] = useState<studentGroupModel | null>(null)
    const [editingSupervisorIds, setEditingSupervisorIds] = useState<number[]>([])
    const [editingGender, setEditingGender] = useState<SD_Gender | null>(null)
    const [newGroup, setNewGroup] = useState({
        groupName: "",
        gender: null as SD_Gender | null,
        supervisorIds: [] as number[],
    })
    const [genderOptions] = useState([
        { value: "", label: "Chọn giới tính" },
        { value: SD_Gender.Male.toString(), label: "Nam" },
        { value: SD_Gender.Female.toString(), label: "Nữ" },
    ])
    const [isDeletePopupOpen, setIsDeletePopupOpen] = useState(false)
    const [groupToDelete, setGroupToDelete] = useState<{ id: number; name: string } | null>(null)

    // Lấy danh sách courses từ store
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role) // Lấy role
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)

    // Tạo hàm helper để kiểm tra vai trò
    const isManagerOrSecretary = (): boolean => {
        return currentUserRole === SD_Role_Name.MANAGER || currentUserRole === SD_Role_Name.SECRETARY
    }

    const isManager = (): boolean => {
        return currentUserRole === SD_Role_Name.MANAGER
    }

    // Lấy courseId từ query params và chuyển sang number
    const searchParams = new URLSearchParams(location.search)
    const selectedCourseId = parseInt(searchParams.get("courseId") || currentCourse?.id?.toString() || "0", 10)

    // Kiểm tra nếu courseId là 0 thì không gọi API
    const validCourseId = selectedCourseId !== 0 ? selectedCourseId : undefined

    // Gọi API khi validCourseId có giá trị
    const {
        data: studentGroupsData,
        isLoading: studentGroupsLoading,
        refetch,
    } = useGetStudentGroupsQuery(validCourseId ? validCourseId : skipToken, {
        refetchOnMountOrArgChange: true,
    })
    var myStudentGroups = studentGroupsData?.result?.filter((group) => {
        return group.supervisors.some((s) => s.id == currentUserId)
    })

    const [selectedCourse, setSelectedCourse] = useState<number | undefined>(validCourseId)

    // Lấy danh sách huynh trưởng khả dụng
    const {
        data: availableSupervisorsData,
        isLoading: availableSupervisorsLoading,
        refetch: refetchAvailableSupervisors,
    } = useGetAvailableSupervisorsQuery(validCourseId ?? skipToken)

    const [createStudentGroup, { isLoading: isCreatingGroup }] = useCreateStudentGroupMutation()
    const [updateStudentGroup, { isLoading: isUpdatingGroup }] = useUpdateStudentGroupMutation()
    const [deleteStudentGroup] = useDeleteStudentGroupMutation()

    // Tìm khóa tu được chọn
    const selectedCourseDetails = listCourseFromStore?.find((course: courseModel) => course.id === selectedCourseId)
    // Kiểm tra xem khóa tu đã kết thúc chưa
    const isCourseInPast =
        selectedCourseDetails && selectedCourseDetails.endDate
            ? new Date(selectedCourseDetails.endDate) < new Date()
            : false

    // Cập nhật URL khi người dùng chọn khóa tu khác
    const handleCourseChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const newCourseId = parseInt(e.target.value, 10)
        setSelectedCourse(newCourseId)
        navigate(`/student-groups?courseId=${newCourseId}`)
    }

    const isLoading = !currentCourse || studentGroupsLoading || !listCourseFromStore

    const handleEdit = (group: studentGroupModel) => {
        setEditingGroup(group)
        setEditingSupervisorIds(group.supervisors.map((s) => s.id))
        setEditingGender(group.gender)
        setIsEditModalOpen(true)
    }

    const handleCreate = () => {
        setNewGroup({ groupName: "", gender: null, supervisorIds: [] }) // Reset form khi mở modal tạo mới
        setIsCreateModalOpen(true)
    }

    const handleDeleteClick = (groupId: number, groupName: string) => {
        setGroupToDelete({ id: groupId, name: groupName })
        setIsDeletePopupOpen(true)
    }

    const [isAutoAssignPopupOpen, setIsAutoAssignPopupOpen] = useState(false)
    const [autoAssignSupervisors] = useAutoAssignSupervisorsMutation()

    // Hàm xử lý khi bấm nút "Phân Huynh trưởng tự động"
    const handleAutoAssignClick = () => {
        setIsAutoAssignPopupOpen(true)
    }

    // Hàm xác nhận phân Huynh trưởng tự động
    const handleConfirmAutoAssign = async () => {
        try {
            const response = await autoAssignSupervisors(selectedCourseId).unwrap()
            if (response.isSuccess) {
                if (Array.isArray(response.result) && response.result.length > 0) {
                    // Có chánh chưa được phân Huynh trưởng
                    const groupNames = response.result?.map((group: any) => group.groupName).join(", ")
                    toastNotify(`Phân Huynh trưởng tự động thành công.`, "success")
                    toastNotify(`Các chánh chưa được phân Huynh trưởng: ${groupNames}`, "warning", { autoClose: false })
                } else {
                    // Tất cả chánh đã được phân Huynh trưởng
                    toastNotify("Phân Huynh trưởng tự động thành công. Tất cả chánh đã có Huynh trưởng.", "success")
                }
                refetch() // Refetch danh sách chánh
                refetchAvailableSupervisors() // Refetch danh sách Huynh trưởng khả dụng
            } else {
                const errorMessages = response.errorMessages ?? ["Đã xảy ra lỗi khi phân Huynh trưởng."]
                errorMessages.forEach((msg: string) => toastNotify(msg, "error"))
            }
        } catch (error: any) {
            const errorMessages = error?.data?.errorMessages ?? ["Đã xảy ra lỗi khi phân Huynh trưởng."]
            errorMessages.forEach((msg: string) => toastNotify(msg, "error"))
        } finally {
            setIsAutoAssignPopupOpen(false)
        }
    }

    const handleConfirmDelete = async () => {
        if (groupToDelete !== null) {
            try {
                // Kiểm tra nếu khóa tu đã kết thúc
                if (isCourseInPast) {
                    toastNotify("Không thể xóa chánh của khóa tu đã kết thúc.", "error")
                    setIsDeletePopupOpen(false)
                    setGroupToDelete(null)
                    return
                }
                await deleteStudentGroup(groupToDelete.id).unwrap()
                toastNotify("Xóa chánh thành công!", "success")
                refetch()
                refetchAvailableSupervisors()
            } catch (error: any) {
                const errorMessages = error?.data?.errorMessages ?? ["Đã xảy ra lỗi khi xóa chánh."]
                errorMessages.forEach((msg: string) => toastNotify(msg, "error"))
            }
        }
        setIsDeletePopupOpen(false)
        setGroupToDelete(null)
    }

    const handleCreateGroup = async () => {
        try {
            // Kiểm tra nếu khóa tu đã kết thúc
            if (isCourseInPast) {
                toastNotify("Không thể tạo chánh cho khóa tu đã kết thúc.", "error")
                return
            }
            // Kiểm tra các trường bắt buộc
            if (!newGroup.groupName.trim()) {
                toastNotify("Vui lòng nhập tên chánh.", "error")
                return
            }
            if (newGroup.gender === null) {
                toastNotify("Vui lòng chọn giới tính.", "error")
                return
            }

            const payload = {
                courseId: selectedCourseId,
                groupName: newGroup.groupName.trim(),
                gender: newGroup.gender,
                supervisorIds: newGroup.supervisorIds,
            }

            await createStudentGroup(payload).unwrap()
            // Thông báo thành công, đóng modal và reset form
            toastNotify("Tạo chánh thành công!", "success")
            setIsCreateModalOpen(false)
            setNewGroup({ groupName: "", gender: null, supervisorIds: [] })
            refetch()
            refetchAvailableSupervisors()
        } catch (error: any) {
            // Xử lý lỗi
            const errorMessages = error?.data?.errorMessages ?? ["Đã xảy ra lỗi khi tạo chánh."]
            errorMessages.forEach((msg: string) => toastNotify(msg, "error"))
        }
    }

    const handleUpdateGroup = async () => {
        if (!editingGroup) return

        try {
            // Kiểm tra nếu khóa tu đã kết thúc
            if (isCourseInPast) {
                toastNotify("Không thể cập nhật chánh cho khóa tu đã kết thúc.", "error")
                return
            }
            // Kiểm tra các trường bắt buộc
            if (!editingGroup.groupName.trim()) {
                toastNotify("Vui lòng nhập tên chánh.", "error")
                return
            }
            if (editingGender === null) {
                toastNotify("Vui lòng chọn giới tính.", "error")
                return
            }

            const payload = {
                id: editingGroup.id,
                courseId: editingGroup.courseId,
                groupName: editingGroup.groupName.trim(),
                gender: editingGender,
                supervisorIds: editingSupervisorIds,
            }

            await updateStudentGroup(payload).unwrap()
            // Thông báo thành công, đóng modal và reset form
            toastNotify("Cập nhật chánh thành công!", "success")
            setIsEditModalOpen(false)
            setEditingGroup(null)
            setEditingSupervisorIds([])
            setEditingGender(null)
            refetch()
            refetchAvailableSupervisors()
        } catch (error: any) {
            // Xử lý lỗi
            const errorMessages = error?.data?.errorMessages ?? ["Đã xảy ra lỗi khi cập nhật chánh."]
            errorMessages.forEach((msg: string) => toastNotify(msg, "error"))
        }
    }

    if (isLoading) {
        return <MainLoader />
    }

    const columns: TableColumn<studentGroupModel>[] = [
        {
            name: "#",
            width: "5rem",
            center: true,
            cell: (row: any, rowIndex: number) => {
                const index = (currentPage - 1) * rowsPerPage + rowIndex + 1
                return index
            },
        },
        {
            name: "Tên",
            selector: (row) => row.groupName,
            sortable: true,
        },
        {
            name: "Giới tính",
            selector: (row) => {
                switch (row.gender) {
                    case SD_Gender.Male:
                        return "Nam"
                    case SD_Gender.Female:
                        return "Nữ"
                    default:
                        return "Khác"
                }
            },
            minWidth: "9rem",
            sortable: true,
        },
        {
            name: "Huynh trưởng",
            selector: (row) => {
                return row.supervisors?.map((supervisor) => supervisor.userName).join(", ") || "Chưa có"
            },
            sortable: true,
            minWidth: "30rem",
        },
        {
            name: "Thao Tác",
            minWidth: "12rem",
            cell: (row) => (
                <div>
                    {/* Chỉ hiển thị các nút thao tác nếu người dùng là Manager hoặc Secretary */}
                    {isManagerOrSecretary() && (
                        <>
                            <OverlayTrigger
                                placement="top"
                                overlay={<Tooltip id={`tooltip-delete-${row.id}`}>Xóa</Tooltip>}
                            >
                                <button
                                    onClick={() => handleDeleteClick(row.id, row.groupName)}
                                    className="btn btn-outline-danger btn-sm m-1"
                                    disabled={isCourseInPast}
                                >
                                    <i className="bi bi-trash"></i>
                                </button>
                            </OverlayTrigger>

                            <OverlayTrigger
                                placement="top"
                                overlay={<Tooltip id={`tooltip-edit-${row.id}`}>Chỉnh sửa</Tooltip>}
                            >
                                <button
                                    onClick={() => handleEdit(row)}
                                    className="btn btn-outline-warning btn-sm m-1"
                                    disabled={isCourseInPast}
                                >
                                    <i className="bi bi-pencil"></i>
                                </button>
                            </OverlayTrigger>
                        </>
                    )}
                    <Link to={`/student-groups/${row.id}`} className="me-2 fs-3">
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
            center: true,
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
        },
    ]

    return (
        <div className="container">
            <div className="mt-0 mb-4">
                <h3 className="fw-bold primary-color">Danh sách chánh</h3>
            </div>
            <div className="row">
                <div className="col-4">
                    <div className="mb-3">
                        <select
                            id="course-select"
                            className="form-select"
                            value={selectedCourse}
                            onChange={handleCourseChange}
                        >
                            {listCourseFromStore?.map((course: courseModel) => (
                                <option key={course.id} value={course.id ?? 0}>
                                    {course.courseName}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>
                <div className="col-4"></div>
                <div className="col-4 text-end">
                    {isManager() && (
                        <>
                            <button
                                onClick={handleCreate}
                                className="btn btn-sm btn-primary me-2"
                                disabled={isCourseInPast}
                            >
                                <i className="bi bi-plus-lg me-1"></i>Thêm Chánh
                            </button>
                        </>
                    )}
                    {/* Chỉ hiển thị các nút nếu người dùng là Manager hoặc Secretary */}
                    {isManagerOrSecretary() && (
                        <>
                            <button
                                onClick={handleAutoAssignClick}
                                className="btn btn-sm btn-primary"
                                disabled={isCourseInPast}
                            >
                                <i className="bi bi-person-plus me-1"></i>Phân Huynh trưởng
                            </button>
                        </>
                    )}
                </div>
            </div>

            <div className="card">
                <div className="card-body">
                    <DataTable
                        columns={columns}
                        data={studentGroupsData?.result || []}
                        pagination
                        striped
                        responsive
                        onChangeRowsPerPage={(newPerPage, page) => {
                            setRowsPerPage(newPerPage)
                            setCurrentPage(page)
                        }}
                        customStyles={{
                            tableWrapper: {
                                style: {
                                    overflowX: "auto",
                                },
                            },
                            table: {
                                style: {
                                    tableLayout: "fixed",
                                    width: "100%",
                                },
                            },
                            headCells: {
                                style: {
                                    fontSize: "15px",
                                    fontWeight: "bold",
                                    whiteSpace: "nowrap",
                                },
                            },
                            cells: {
                                style: {
                                    fontSize: "15px",
                                    whiteSpace: "normal",
                                    wordWrap: "break-word",
                                    overflow: "hidden",
                                },
                            },
                            rows: {
                                style: {
                                    fontSize: "15px",
                                },
                            },
                        }}
                        noDataComponent={<div>Không có chánh nào</div>}
                        conditionalRowStyles={[
                            {
                                when: (row) => row.id === myStudentGroups[0]?.id,
                                style: {
                                    fontWeight: "bold", // Đặt kiểu in đậm cho dòng có id === 3
                                },
                            },
                        ]}
                    />
                </div>
            </div>

            {/* Modal Tạo chánh mới */}
            <Modal show={isCreateModalOpen} onHide={() => setIsCreateModalOpen(false)} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Thêm chánh mới</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        {/* Tên chánh */}
                        <Form.Group controlId="groupName">
                            <Form.Label>Tên chánh</Form.Label>
                            <Form.Control
                                type="text"
                                value={newGroup.groupName}
                                required
                                onChange={(e) => setNewGroup({ ...newGroup, groupName: e.target.value })}
                            />
                        </Form.Group>

                        {/* Dropdown giới tính */}
                        <Form.Group controlId="groupGender" className="mt-3">
                            <Form.Label>Giới tính</Form.Label>
                            <Form.Select
                                value={newGroup.gender !== null ? newGroup.gender.toString() : ""}
                                required
                                onChange={(e) => {
                                    const value = e.target.value !== "" ? (parseInt(e.target.value) as SD_Gender) : null
                                    setNewGroup({ ...newGroup, gender: value, supervisorIds: [] }) // Reset supervisorIds khi thay đổi giới tính
                                }}
                            >
                                {genderOptions?.map((option) => (
                                    <option key={option.value} value={option.value}>
                                        {option.label}
                                    </option>
                                ))}
                            </Form.Select>
                        </Form.Group>

                        {/* Dropdown Huynh trưởng với react-select */}
                        <Form.Group controlId="groupSupervisors" className="mt-3">
                            <Form.Label>Huynh trưởng</Form.Label>
                            {availableSupervisorsLoading ? (
                                <div>Đang tải danh sách huynh trưởng...</div>
                            ) : (
                                <Select
                                    isMulti
                                    options={
                                        newGroup.gender !== null
                                            ? availableSupervisorsData?.result
                                                  ?.filter(
                                                      (supervisor: supervisorModel) =>
                                                          supervisor.gender === newGroup.gender,
                                                  )
                                                  .map((supervisor: supervisorModel) => ({
                                                      value: supervisor.id,
                                                      label: supervisor.userName,
                                                  }))
                                            : []
                                    }
                                    value={newGroup.supervisorIds.map((id) => {
                                        const supervisor = availableSupervisorsData?.result?.find(
                                            (s: supervisorModel) => s.id === id,
                                        )
                                        return { value: id, label: supervisor?.userName }
                                    })}
                                    onChange={(selectedOptions) => {
                                        const selectedIds = selectedOptions
                                            ? selectedOptions.map((option: any) => option.value)
                                            : []
                                        setNewGroup({
                                            ...newGroup,
                                            supervisorIds: selectedIds,
                                        })
                                    }}
                                    placeholder="Chọn Huynh trưởng..."
                                    noOptionsMessage={() =>
                                        newGroup.gender === null
                                            ? "Vui lòng chọn giới tính của chánh trước"
                                            : "Không có Huynh trưởng khả dụng"
                                    }
                                />
                            )}
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button
                        variant="secondary"
                        onClick={() => {
                            setIsCreateModalOpen(false)
                            setNewGroup({ groupName: "", gender: null, supervisorIds: [] }) // Reset form khi đóng modal
                        }}
                    >
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleCreateGroup} disabled={isCreatingGroup}>
                        {isCreatingGroup ? "Đang tạo..." : "Tạo chánh"}
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Modal Chỉnh sửa chánh */}
            <Modal show={isEditModalOpen} onHide={() => setIsEditModalOpen(false)} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Chỉnh sửa thông tin chánh</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        {/* Tên chánh */}
                        <Form.Group controlId="editGroupName">
                            <Form.Label>Tên chánh</Form.Label>
                            <Form.Control
                                type="text"
                                value={editingGroup?.groupName || ""}
                                required
                                onChange={(e) =>
                                    setEditingGroup({
                                        ...editingGroup!,
                                        groupName: e.target.value,
                                    })
                                }
                            />
                        </Form.Group>

                        {/* Dropdown giới tính */}
                        <Form.Group controlId="editGroupGender" className="mt-3">
                            <Form.Label>Giới tính</Form.Label>
                            <Form.Select
                                value={editingGender !== null ? editingGender.toString() : ""}
                                required
                                disabled
                                onChange={(e) => {
                                    const value = e.target.value !== "" ? (parseInt(e.target.value) as SD_Gender) : null
                                    setEditingGender(value)
                                    setEditingSupervisorIds([]) // Reset supervisorIds khi thay đổi giới tính
                                }}
                            >
                                {genderOptions.map((option) => (
                                    <option key={option.value} value={option.value}>
                                        {option.label}
                                    </option>
                                ))}
                            </Form.Select>
                        </Form.Group>

                        {/* Dropdown Huynh trưởng với react-select */}
                        <Form.Group controlId="editGroupSupervisors" className="mt-3">
                            <Form.Label>Huynh trưởng</Form.Label>
                            {availableSupervisorsLoading ? (
                                <div>Đang tải danh sách huynh trưởng...</div>
                            ) : (
                                <Select
                                    isMulti
                                    options={
                                        editingGender !== null
                                            ? availableSupervisorsData?.result
                                                  ?.concat(editingGroup?.supervisors || [])
                                                  ?.filter(
                                                      (supervisor: supervisorModel) =>
                                                          supervisor.gender === editingGender,
                                                  )
                                                  ?.map((supervisor: supervisorModel) => ({
                                                      value: supervisor.id,
                                                      label: supervisor.userName,
                                                  }))
                                            : []
                                    }
                                    value={editingSupervisorIds?.map((id) => {
                                        const supervisor = availableSupervisorsData?.result
                                            ?.concat(editingGroup?.supervisors || [])
                                            ?.find((s: supervisorModel) => s.id === id)
                                        return { value: id, label: supervisor?.userName }
                                    })}
                                    onChange={(selectedOptions) => {
                                        const selectedIds = selectedOptions
                                            ? selectedOptions?.map((option: any) => option.value)
                                            : []
                                        setEditingSupervisorIds(selectedIds)
                                    }}
                                    placeholder="Chọn Huynh trưởng..."
                                    noOptionsMessage={() =>
                                        editingGender === null
                                            ? "Vui lòng chọn giới tính của chánh trước"
                                            : "Không có Huynh trưởng khả dụng"
                                    }
                                />
                            )}
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button
                        variant="secondary"
                        onClick={() => {
                            setIsEditModalOpen(false)
                            setEditingGroup(null)
                            setEditingSupervisorIds([])
                            setEditingGender(null)
                        }}
                    >
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleUpdateGroup} disabled={isUpdatingGroup}>
                        {isUpdatingGroup ? "Đang cập nhật..." : "Lưu thay đổi"}
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Popup xác nhận xóa */}
            <ConfirmationPopup
                isOpen={isDeletePopupOpen}
                onClose={() => setIsDeletePopupOpen(false)}
                onConfirm={handleConfirmDelete}
                message={`Bạn có chắc chắn muốn xóa chánh <strong>${groupToDelete?.name}</strong> không? <br/> 
<span style="
  background-color: yellow; 
  color: black; 
  font-weight: bold; 
  display: block; 
  padding: 5px; 
  border-radius: 4px;
">
  Lưu ý: Những dữ liệu khác liên quan đến chánh <strong>${groupToDelete?.name}</strong> cũng sẽ bị xóa theo!
</span>`}
                title="Xác nhận xóa"
            />

            {/* Popup xác nhận phân Huynh trưởng tự động */}
            <ConfirmationPopup
                isOpen={isAutoAssignPopupOpen}
                onClose={() => setIsAutoAssignPopupOpen(false)}
                onConfirm={handleConfirmAutoAssign}
                message="Bạn có chắc chắn muốn tự động phân Huynh trưởng cho các chánh chưa có Huynh trưởng không?"
                title="Xác nhận phân Huynh trưởng tự động"
            />
        </div>
    )
}
export default ListStudentGroup
