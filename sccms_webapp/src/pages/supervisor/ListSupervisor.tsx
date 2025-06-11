// src/pages/Supervisor/ListSupervisor.tsx

import React, { useState, useEffect } from "react"
import { MainLoader, SupervisorList, SearchSupervisor } from "../../components/Page"
import { useGetSupervisorsQuery } from "../../apis/supervisorApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useNavigate } from "react-router-dom"
import { SD_Role_Name, SD_UserStatus } from "../../utility/SD"
import { Button, Modal } from "react-bootstrap"
import AddSupervisorModal from "./AddSupervisorModal"

interface SearchParams {
    name: string
    phoneNumber: string
    email: string
    gender: string
}

function ListSupervisor() {
    const navigate = useNavigate()
    const [showAddSupervisorModal, setShowAddSupervisorModal] = useState(false)

    const handleShowAddSupervisorModal = () => setShowAddSupervisorModal(true)
    const handleCloseAddSupervisorModal = () => setShowAddSupervisorModal(false)
    const [searchParamsState, setSearchParamsState] = useState<SearchParams>({
        name: "",
        phoneNumber: "",
        email: "",
        gender: "",
    })

    // Lấy thông tin khóa tu hiện tại từ Redux store
    const currentCourse: any = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentRole: any = useSelector((state: RootState) => state.auth.user?.role)

    // Gọi API để lấy danh sách Supervisor với trạng thái "Hoạt động"
    const { data, isLoading, refetch } = useGetSupervisorsQuery({
        courseId: currentCourse?.id ? parseInt(currentCourse.id) : 0,
        name: searchParamsState.name,
        phoneNumber: searchParamsState.phoneNumber,
        email: searchParamsState.email,
        gender: searchParamsState.gender,
        status: SD_UserStatus.ACTIVE, // Đặt luôn trạng thái là "Hoạt động"
    })

    const isManagerOrSecretary = (role: string | undefined): boolean => {
        return role == SD_Role_Name.SECRETARY || role == SD_Role_Name.MANAGER
    }

    // Hàm xử lý khi người dùng tìm kiếm
    const handleSearch = (params: any) => {
        setSearchParamsState({
            name: params.name,
            phoneNumber: params.phoneNumber,
            email: params.email,
            gender: params.gender,
        })
    }

    useEffect(() => {
        // Khi searchParams hoặc currentCourse thay đổi, refetch dữ liệu
        refetch()
    }, [searchParamsState, currentCourse, refetch])

    if (isLoading) return <MainLoader />

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách Huynh trưởng</h3>
            </div>
            <SearchSupervisor onSearch={handleSearch} currentCourse={currentCourse} />
            <div className="container text-end mt-4">
                {isManagerOrSecretary(currentRole) && (
                    <Button variant="primary" onClick={handleShowAddSupervisorModal} className="mb-3">
                        <i className="bi bi-plus-lg"></i> Thêm Huynh trưởng
                    </Button>
                )}
            </div>
            <div className="mt-2">
                {/* Truyền refetch vào SupervisorList */}
                <SupervisorList supervisors={data?.result || []} refetch={refetch} />
            </div>

            {/* Modal thêm Huynh trưởng */}
            <Modal show={showAddSupervisorModal} onHide={handleCloseAddSupervisorModal} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Thêm Huynh trưởng</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <AddSupervisorModal
                        onClose={() => {
                            handleCloseAddSupervisorModal()
                            refetch() // Làm mới danh sách sau khi thêm
                        }}
                    />
                </Modal.Body>
            </Modal>
        </div>
    )
}

export default ListSupervisor
