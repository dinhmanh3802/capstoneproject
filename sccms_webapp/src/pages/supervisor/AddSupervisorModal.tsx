// AddSupervisorModal.tsx
import React, { useState, useEffect } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { Button } from "react-bootstrap"
import { toastNotify } from "../../helper"
import { useChangeUserRoleMutation, useGetAvailableSupervisorsQuery } from "../../apis/userApi"
import { SD_Gender, SD_Role, SD_UserStatus } from "../../utility/SD"

interface User {
    id: number
    fullName: string
    phoneNumber: string
    email: string
    gender: number
    status: number
}

interface AddSupervisorModalProps {
    onClose: () => void
}

function AddSupervisorModal({ onClose }: AddSupervisorModalProps) {
    const [currentPage, setCurrentPage] = useState(1)
    const [perPage, setPerPage] = useState(10)

    // Thêm state để quản lý giá trị tìm kiếm
    const [searchParams, setSearchParams] = useState({
        name: "",
        phoneNumber: "",
        email: "",
    })

    // Sử dụng API mới để lấy các Staff đang rảnh
    const { data, isLoading, refetch } = useGetAvailableSupervisorsQuery()

    const handlePageChange = (page: number) => {
        setCurrentPage(page)
    }

    // Hàm xử lý thay đổi số hàng mỗi trang
    const handlePerRowsChange = (newPerPage: number, page: number) => {
        setPerPage(newPerPage)
        setCurrentPage(page)
    }

    const [selectedRows, setSelectedRows] = useState<User[]>([])
    const [changeUserRole] = useChangeUserRoleMutation()

    const handleSelectedRowsChange = (state: any) => {
        setSelectedRows(state.selectedRows)
    }

    const handleConfirm = async () => {
        if (selectedRows.length === 0) {
            toastNotify("Vui lòng chọn ít nhất một nhân viên.", "warning")
            return
        }

        const userIds = selectedRows.map((user) => user.id)
        try {
            await changeUserRole({
                userIds,
                newRoleId: SD_Role.SUPERVISOR,
            }).unwrap()
            toastNotify("Thêm Huynh trưởng thành công!", "success")
            onClose()
        } catch (error: any) {
            toastNotify(`Có lỗi xảy ra: ${error.data?.errorMessages?.join(", ") || error.message}`, "error")
        }
    }

    // Hàm xử lý thay đổi giá trị tìm kiếm
    const handleSearchInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setSearchParams((prev) => ({
            ...prev,
            [name]: value,
        }))
    }

    // Hàm xử lý khi nhấn nút tìm kiếm
    const handleSearch = () => {
        refetch()
    }

    const columns: TableColumn<User>[] = [
        {
            name: "#",
            width: "60px",
            cell: (_row, index) => (currentPage - 1) * perPage + index + 1, // Tính số thứ tự
            ignoreRowClick: true,
            allowOverflow: true,
            button: false,
        },
        {
            name: "Họ và tên",
            selector: (row) => row.fullName,
            sortable: true,
        },
        {
            name: "Số điện thoại",
            selector: (row) => row.phoneNumber,
            sortable: true,
        },
        {
            name: "Giới tính",
            selector: (row) => (row.gender === SD_Gender.Male ? "Nam" : "Nữ"),
            sortable: true,
            maxWidth: "8rem",
        },
        {
            name: "Email",
            selector: (row) => row.email,
            sortable: true,
            minWidth: "12rem",
        },
    ]

    // Sử dụng useEffect để refetch dữ liệu khi searchParams thay đổi
    useEffect(() => {
        if (searchParams.name || searchParams.phoneNumber || searchParams.email) {
            // Lọc dữ liệu client-side vì API mới không hỗ trợ tìm kiếm
            // Hoặc bạn có thể mở rộng API để hỗ trợ tìm kiếm
            refetch()
        }
    }, [searchParams, refetch])

    // Lọc dữ liệu client-side dựa trên searchParams
    const filteredData = // @ts-ignore
        data?.result?.filter((user: User) => {
            const matchesName = user.fullName.toLowerCase().includes(searchParams.name.toLowerCase())
            const matchesPhone = user.phoneNumber.includes(searchParams.phoneNumber)
            const matchesEmail = user.email.toLowerCase().includes(searchParams.email.toLowerCase())
            return matchesName && matchesPhone && matchesEmail
        }) || []

    return (
        <div>
            {/* Thêm phần tìm kiếm */}
            <div className="mb-3">
                <div className="row">
                    <div className="col-md-4">
                        <input
                            type="text"
                            name="name"
                            value={searchParams.name}
                            onChange={handleSearchInputChange}
                            className="form-control"
                            placeholder="Tìm theo tên..."
                        />
                    </div>
                    <div className="col-md-4">
                        <input
                            type="text"
                            name="phoneNumber"
                            value={searchParams.phoneNumber}
                            onChange={handleSearchInputChange}
                            className="form-control"
                            placeholder="Tìm theo số điện thoại..."
                        />
                    </div>
                    <div className="col-md-4">
                        <input
                            type="text"
                            name="email"
                            value={searchParams.email}
                            onChange={handleSearchInputChange}
                            className="form-control"
                            placeholder="Tìm theo email..."
                        />
                    </div>
                </div>
                <div className="mt-2 text-end">
                    <Button variant="primary" onClick={handleSearch}>
                        Tìm kiếm
                    </Button>
                </div>
            </div>
            <DataTable
                columns={columns}
                data={filteredData}
                selectableRows
                onSelectedRowsChange={handleSelectedRowsChange}
                pagination
                paginationServer
                paginationTotalRows={filteredData.length}
                onChangePage={handlePageChange}
                onChangeRowsPerPage={handlePerRowsChange}
                progressPending={isLoading}
                selectableRowsHighlight
            />
            <div className="text-end mt-3">
                <Button variant="secondary" onClick={onClose} className="me-2">
                    Hủy
                </Button>
                <Button variant="primary" onClick={handleConfirm}>
                    Xác nhận
                </Button>
            </div>
        </div>
    )
}

export default AddSupervisorModal
