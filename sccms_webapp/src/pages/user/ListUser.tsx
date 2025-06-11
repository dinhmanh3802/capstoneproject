// src/pages/user/ListUser.tsx
import React, { useState, useEffect } from "react"
import { MainLoader, SearchUser, UserList } from "../../components/Page"
import { SD_Gender, SD_Role, SD_UserStatus, SD_Role_Name } from "../../utility/SD"
import { Modal, Button } from "react-bootstrap"
import CreateUser from "./CreateUser"
import { useGetUsersQuery } from "../../apis/userApi"
import BulkCreateUser from "./BulkCreateUser"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useSearchParams } from "react-router-dom"

function ListUser() {
    const [showBulkCreateModal, setShowBulkCreateModal] = useState(false)
    const handleShowBulkCreateModal = () => setShowBulkCreateModal(true)
    const handleCloseBulkCreateModal = () => setShowBulkCreateModal(false)

    // Sử dụng useSearchParams để quản lý query parameters
    const [searchParams, setSearchParams] = useSearchParams()

    // State để kiểm soát Modal
    const [showCreateUserModal, setShowCreateUserModal] = useState(false)

    // Hàm mở và đóng Modal
    const handleShowCreateUserModal = () => setShowCreateUserModal(true)
    const handleCloseCreateUserModal = () => setShowCreateUserModal(false)

    // Lấy vai trò hiện tại của người dùng từ authSlice
    const currentUserRoleName = useSelector((state: RootState) => state.auth.user?.role)

    // Xác định xem có nên loại bỏ Admin hay không dựa trên vai trò của người dùng
    const excludeAdmin = currentUserRoleName === SD_Role_Name.MANAGER

    // Hàm chuyển đổi query parameters thành object searchParams
    const getSearchParams = () => {
        return {
            name: searchParams.get("name") || "",
            phoneNumber: searchParams.get("phoneNumber") || "",
            email: searchParams.get("email") || "",
            role: searchParams.get("role") || "",
            status: searchParams.get("status") || "",
            gender: searchParams.get("gender") || "",
            page: parseInt(searchParams.get("page") || "1", 10),
            perPage: parseInt(searchParams.get("perPage") || "10", 10),
        }
    }

    // Lấy searchParams từ URL
    const currentSearchParams = getSearchParams()

    // Gọi API để lấy dữ liệu dựa trên searchParams
    const { data, isLoading, refetch } = useGetUsersQuery({
        name: currentSearchParams.name,
        phoneNumber: currentSearchParams.phoneNumber,
        roleId: currentSearchParams.role,
        email: currentSearchParams.email,
        status: currentSearchParams.status,
        gender: currentSearchParams.gender,
        page: currentSearchParams.page,
        perPage: currentSearchParams.perPage,
    })

    // Hàm xử lý khi người dùng tìm kiếm
    const handleSearch = (params: any) => {
        const newSearchParams: any = {
            name: params.name || "",
            phoneNumber: params.phoneNumber || "",
            email: params.email || "",
            role: params.role || "",
            status: params.status || "",
            gender: params.gender || "",
            page: 1, // Reset về trang đầu khi tìm kiếm mới
            perPage: 10, // Bạn có thể thay đổi giá trị mặc định
        }
        setSearchParams(newSearchParams)
    }

    // Hàm xử lý thay đổi trang và số hàng mỗi trang
    const handleTableChange = (newParams: any) => {
        const updatedParams = {
            ...currentSearchParams,
            ...newParams,
        }
        setSearchParams(updatedParams)
    }

    useEffect(() => {
        refetch()
    }, [searchParams])

    if (isLoading) return <MainLoader />

    // Lọc bỏ Admin nếu người dùng là Manager
    const filteredUsers = excludeAdmin
        ? data?.result.filter((user: any) => user.roleId !== SD_Role.ADMIN)
        : data?.result

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách người dùng</h3>
            </div>
            <SearchUser onSearch={handleSearch} excludeAdmin={excludeAdmin} initialValues={currentSearchParams} />
            <div className="container text-end mt-4">
                <Button className="btn btn-sm btn-primary" onClick={handleShowCreateUserModal}>
                    <i className="bi bi-plus-lg"></i> Tạo mới
                </Button>
                <Button className="btn btn-sm btn-success ms-2 me-2" onClick={handleShowBulkCreateModal}>
                    <i className="bi bi-cloud-upload "></i> Nhập Excel
                </Button>
            </div>
            <div className="mt-2">
                <UserList users={filteredUsers} />
            </div>

            {/* Modal tạo người dùng */}
            <Modal show={showCreateUserModal} onHide={handleCloseCreateUserModal} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Tạo Người Dùng</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <CreateUser
                        onClose={() => {
                            handleCloseCreateUserModal()
                            refetch()
                        }}
                    />{" "}
                    {/* Gọi refetch để làm mới dữ liệu */}
                </Modal.Body>
            </Modal>

            {/* Modal tạo người dùng hàng loạt */}
            <Modal show={showBulkCreateModal} onHide={handleCloseBulkCreateModal}>
                <Modal.Header closeButton>
                    <Modal.Title>Tạo Người Dùng Hàng Loạt</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <BulkCreateUser
                        onClose={() => {
                            handleCloseBulkCreateModal()
                            refetch()
                        }}
                    />
                </Modal.Body>
            </Modal>
        </div>
    )
}

export default ListUser
